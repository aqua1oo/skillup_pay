using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PayClientHelper
{
    #region API
    public class PayCommonConstant
    {
        /**
         * API 연동 정보
         * URL_API_DEV : 연동 개발 서버
         * URL_API_REAL : 연동 실서버 서버
         * */
        public const string URL_API_DEV = "https://testpgapi.easypay.co.kr";
        public const string URL_API_REAL = "https://pgapi.easypay.co.kr";

        public static readonly Hashtable HASH_URL_API = new Hashtable()
        {
            //인터페이스 ID로 정리
            //KICC_API0001 : SMS결제요청
            {"KICC_API0001", "/directapi/trades/directSmsUrlPayReg"},
            //KICC_API0002 : SMS결제요청취소
            {"KICC_API0002", "/directapi/trades/directSmsUrlPayRegCancel"},
            //KICC_API0003 : 결제창결제요청(거래등록)
            {"KICC_API0003", "/api/trades/webpay"},
            //KICC_API0004 : 결제창결제요청(거래승인)
            {"KICC_API0004", "/api/trades/approval"},
            //KICC_API0005 : 결제 환불/취소
            {"KICC_API0005", "/api/trades/revise"}
        };

        public static string GetApiUrl(string api)
        {
            return HASH_URL_API[api].ToString();
        }
        
        public const string KICC_MALLID_AICANDO_SMS = "05575112";
        public const string KICC_MALLID_AICANDO_WINDOW = "05575113";

        public const string KICC_PAY_CANCEL_SECRETKEY = "easypay!0h4VMXT7";

        #region SMS 결제요청
        public const string KICC_REGTYPE_SMS = "51";
        public const string KICC_REGTYPE_URL = "52";

        public const string KICC_REGSUBTYPE_PAY = "10";
        public const string KICC_REGSUBTYPE_REPAY = "11";
        public const string KICC_REGSUBTYPE_CANCEL = "40";
        public const string KICC_REGSUBTYPE_RECANCEL = "41";

        public const string KICC_CURRENCY_WON = "00";

        public const string KICC_PAYCODE_ALL = "00";
        public const string KICC_PAYCODE_CARD = "11";
        public const string KICC_PAYCODE_ACCOUNT = "21";
        public const string KICC_PAYCODE_NOACCOUNT = "22";
        public const string KICC_PAYCODE_PHONE = "31";
        #endregion

        #region SMS 결제요청취소
        public const string KICC_REVISETYPECODE_CANCEL = "10";
        public const string KICC_REVISETYPECODE_REFUND = "40";

        public const string KICC_REVISESUBTYPECODE_CANCEL = "30";
        #endregion

        #region 결제창요청
        public const string KICC_WINDOW_MODE_00 = "00";
        public const string KICC_WINDOW_RETURN_URL = "http://loacahost:56996/PayInterface/KiccPayReturn";
        public const string KICC_WINDOW_DEVICE_TYPE_PC = "pc";
        #endregion

        #region 실 결제후 완료처리
        public const string KICC_RESULT_SUCCESS = "res_cd=0000^res_msg=SUCCESS";
        public const string KICC_RESULT_FAIL = "res_cd=5001^res_msg=FAIL";
        #endregion
    }
    #endregion

    #region API REQUEST BODY, RESPONSE BODY

    #region SMS 결제요청 KICC_API0001    
    public class KICC_API0001_REQUEST
    {
        public directRegInfoRequest directRegInfo = new directRegInfoRequest();
        public directOrderInfoRequest directOrderInfo = new directOrderInfoRequest();
        public class directRegInfoRequest
        {
            public string mallId { get; set; }              //KICC에서 발급한 상점 ID
            public string regTxtype { get; set; }           //등록구분 (51 : SMS PAY 등록, 52 : URL PAY 등록)
            public string regSubtype { get; set; }          //등록세부구분 (10 : 승인요청 등록, 11 : 승인요청 재등록(SMS재발송), 40 : 취소요청 등록, 41 : 취소요청 재등록(SMS재발송))
            public string amount { get; set; }              //결제 총 금액
            public string currency { get; set; }            //통화코드 (00 : 원)
            public string payCode { get; set; }             //결제수단코드 (00 : 전체수단, 11 : 신용카드, 21 : 계좌이체, 22 : 가상계좌(무통장입금), 31 : 휴대폰소액결제)
            public string pgCno { get; set; }               //결제승인거래번호 (regSubtype : 40 일경우 필수(Noti로 받은 PG거래번호) 셋팅, regSubtype : 11,41 일 경우 필수(pgCno 등록거래번호) 셋팅)
            public string rcvMobileNo { get; set; }         //휴대폰번호 (SMS 받을 고객 휴대폰번호( ‘-‘없음) regTxtype : 51일 경우 필수)
            public string sndTelNo { get; set; }            //발신번호 (SMS 발신번호 ( ‘-‘ 없음) regTxtype : 51일 경우 필수)
            public string installmentMonth { get; set; }    //할부개월 (단일개월만 가능 예) 03)
            public string mallName { get; set; }            //가맹점명 (SMS발송시 사용할 간략한 상호명 regTxtype : 51일 경우 필수)
            public string smsPayExpr { get; set; }          //결제만료일시 (빈값일 경우 D+7)
            public string certType { get; set; }            //신용카드 인증구분 (빈값 : 일반, 0 : 인증, 1 : 비인증)
            public string dispMsg { get; set; }             //결제창 안내문구
        }

        public class directOrderInfoRequest
        {
            public string shopOrderNo { get; set; }         //가맹점 주문번호
            public long goodsAmount { get; set; }           //상품금액
            public string customerId { get; set; }          //가맹점 고객ID
            public string customerName { get; set; }        //가맹점 고객명
            public string customerMail { get; set; }        //고객 이메일
            public string goodsName { get; set; }           //상품명
            public string value1 { get; set; }              //가맹점 필드1
            public string value2 { get; set; }              //가맹점 필드2
            public string value3 { get; set; }              //가맹점 필드3
        }
    }

    public class KICC_API0001_RESPONSE
    {
        public string resCd { get; set; }                   //결과코드
        public string resMsg { get; set; }                  //결과메세지
        public string pgCno { get; set; }                   //거래등록번호
        public long amount { get; set; }                    //총 결제금액
        public string shopOrderNo { get; set; }             //주문번호
        public string expiryDate { get; set; }              //유효기간
        public string authPageUrl { get; set; }             //결제 URL (regTxtype: 51의 경우, 해당 URL이 고객번호로 발송됨, regTxtype: 52의 경우, 가맹점에서 해당 URL을 고객에게 전송)
    }
    #endregion

    #region SMS 결제요청취소 KICC_API0002
    public class KICC_API0002_REQUEST
    {
        public string mallId { get; set; }                  //KICC에서 발급한 상점 ID
        public string reviseTypeCode { get; set; }          //거래구분
        public string reviseSubTypeCode { get; set; }       //변경세부구분
        public string pgCno { get; set; }                   //거래등록번호
        public string clientIp { get; set; }                //요청자IP
        public string clientId { get; set; }                //요청자ID
    }

    public class KICC_API0002_RESPONSE
    {
        public string resCd { get; set; }                   //결과코드
        public string resMsg { get; set; }                  //결과메세지
        public string pgCno { get; set; }                   //거래등록번호
    }
    #endregion

    #region 결제창 결제요청 KICC_API0003    
    public class KICC_API0003_REQUEST
    {
        public string mallId { get; set; }                  //KICC에서 발급한 상점 ID
        public string payMethodTypeCode { get; set; }       //결제수단코드 (11 : 신용카드, 21 : 계좌이체, 22 : 가상계좌, 31 : 휴대폰, 50 : 선불결제, 60 : 간편결제, 81 : 배치인증)
        public string currency { get; set; }                //통화코드 (00 : 원)
        public long amount { get; set; }                    //결제 총 금액
        public string clientTypeCode { get; set; }          //결제창 종류 코드 (00 : 통합결제창 전용)
        public string returnUrl { get; set; }               //return url
        public string shopOrderNo { get; set; }             //가맹점 주문번호
        public string deviceTypeCode { get; set; }          //고객결제단말 (pc, mobile)
        public string langFlag { get; set; }                //언어선택 (KOR : 한국어, ENG : 영어, JPN : 일본어, CHN : 중국어)
        public string mallName { get; set; }                //가맹점명
        public string appScheme { get; set; }               //가맹점 앱 스키마 (사용X)
        public string windowType { get; set; }              //iframe용 스키마 (사용X)

        public orderInfoRequest orderInfo = new orderInfoRequest();
        public payMethodInfoRequest payMethodInfo = new payMethodInfoRequest();
        public shopValueInfoRequest shopValueInfo = new shopValueInfoRequest();
        public class orderInfoRequest
        {
            public string goodsTypeCode { get; set; }       //상품정보 구분코드 (0 : 실물, 1 : 컨텐츠)
            public string goodsName { get; set; }           //상품명
            public string serviceExpiryDate { get; set; }   //서비스 유료일자
            public customerInfoRequest customerInfo = new customerInfoRequest();

            public class customerInfoRequest
            {
                public string customerId { get; set; }      //상점 고객ID
                public string customerName { get; set; }    //상점 고객명
                public string customerMail { get; set; }    //상점 고객메일
                public string customerContactNo { get; set; }   //상점 고객연락처
                public string customerAddr { get; set; }    //상점 고객주소
                public string goodsName { get; set; }           //상품명
                public string value1 { get; set; }              //가맹점 필드1
                public string value2 { get; set; }              //가맹점 필드2
                public string value3 { get; set; }              //가맹점 필드3
            }
        }

        public class payMethodInfoRequest
        {
            public virtualAccountMethodInfoRequest virtualAccountMethodInfo = new virtualAccountMethodInfoRequest();
            public class virtualAccountMethodInfoRequest
            {
                public string expiryDate { get; set; }      //입금만료일자
                public string expiryTime { get; set; }      //입금만료시간
            }
        }

        public class shopValueInfoRequest
        {
            public string value1 { get; set; }              //필드1
            public string value2 { get; set; }              //필드2
            public string value3 { get; set; }              //필드3
            public string value4 { get; set; }              //필드4
            public string value5 { get; set; }              //필드5
            public string value6 { get; set; }              //필드6
        }
    }

    public class KICC_API0003_RESPONSE
    {
        public string resCd { get; set; }                   //결과코드
        public string resMsg { get; set; }                  //결과메세지
        public string authPageUrl { get; set; }             //결제 URL (거래인증 요청을 위한 URL)
    }

    public class KICC_API0003_RETURNURL_RESPONSE
    {
        public string resCd { get; set; }                   //결과코드
        public string resMsg { get; set; }                  //결과메세지
        public string shopOrderNo { get; set; }             //상점주문번호
        public string authorizationId { get; set; }         //거래인증번호
        public string shopValue1 { get; set; }              //기타1
        public string shopValue2 { get; set; }              //기타2
        public string shopValue3 { get; set; }              //기타3
        public string shopValue4 { get; set; }              //기타4
        public string shopValue5 { get; set; }              //기타5
        public string shopValue6 { get; set; }              //기타6
        public string shopValue7 { get; set; }              //기타7
    }
    #endregion

    #region 결제창 승인요청 KICC_API0004
    public class KICC_API0004_REQUEST
    {
        public string mallId { get; set; }                  //KICC에서 발급한 상점 ID
        public string shopTransactionId { get; set; }       //가맹점 트랜잭션ID
        public string authorizationId { get; set; }         //거래인증 거래번호 (거래응답 return url로 받은 인증값)
        public string shopOrderNo { get; set; }             //가맹점 주문번호
        public string approvalReqDate { get; set; }         //승인 요청일자
    }

    public class KICC_API0004_RESPONSE
    {
        public string resCd { get; set; }                   //결과코드
        public string resMsg { get; set; }                  //결과메시지
        public string mallId { get; set; }                  //KICC에서 발급한 상점 ID
        public string pgCno { get; set; }                   //PG 승인거래번호 (취소시 필수)
        public string shopTransactionId { get; set; }       //KICC에서 발급한 상점 ID
        public string shopOrderNo { get; set; }             //KICC에서 발급한 상점 ID
        public long amount { get; set; }                    //총 승인금액
        public string transactionDate { get; set; }         //거래일시 (yyyyMMddhhmmss)
        public string statusCode { get; set; }              //거래상태코드
        public string statusMessage { get; set; }           //거래상태메시지
        public string msgAuthValue { get; set; }            //메시지 인증값
        public string escrowUsed { get; set; }              //에스크로 사용여부

        public paymentInfoResponse paymentInfo = new paymentInfoResponse();
        public class paymentInfoResponse
        {
            public string payMethodTypeCode { get; set; }   //결제수단 코드 (11 : 신용카드)
            public string approvalNo { get; set; }          //결제수단 승인번호
            public string approvalDate { get; set; }        //결제수단 거래일시
            public string cpCode { get; set; }              //서비스사 코드
            public string multiCardAmount { get; set; }     //복합결제 승인/취소금액
            public string multiPntAmount { get; set; }      //복합결제 포인트 승인/취소금액
            public string multiCponAmount { get; set; }     //복합결제 쿠폰 승인/취소금액
        }

        public cardInfoResponse cardInfo = new cardInfoResponse();
        public class cardInfoResponse
        {
            public string cardNo { get; set; }              //카드번호
            public string issuerCode { get; set; }          //발급사코드
            public string issuerName { get; set; }          //발급사명
            public string acquirerCode { get; set; }        //매입사코드
            public string acquirerName { get; set; }        //매입사명
            public string installmentMonth { get; set; }    //할부개월
            public string freeInstallmentTypeCode { get; set; }     //무이자타입 (00 : 일반할부)
            public string cardGubun { get; set; }           //카드구분 (N : 신용, Y : 체크, G : 기프트)
            public string cardBizGubun { get; set; }        //발급주체구분 (P : 개인, C : 법인, N : 기타)
            public string partCancelUsed { get; set; }      //부분취소 가능여부 (Y, N)
            public long couponAmount { get; set; }          //즉시할인금액
            public string subCardCd { get; set; }           //제휴사 카드코드
            public string vanSno { get; set; }              //VAN 거래일련번호
        }

        public bankInfoResponse bankInfo = new bankInfoResponse();
        public class bankInfoResponse
        {
            public string bankCode { get; set; }            //은행코드
            public string bankName { get; set; }            //은행명
        }

        public virtualAccountInfoResponse virtualAccountInfo = new virtualAccountInfoResponse();
        public class virtualAccountInfoResponse
        {
            public string bankCode { get; set; }            //가상계좌은행코드
            public string bankName { get; set; }            //가상계좌은행명
            public string accountNo { get; set; }           //채번계좌번호
            public string depositName { get; set; }         //예금주 성명
            public string expiryDate { get; set; }          //계좌사용만료일
        }
    }
    #endregion

    #region 결제 후 NOTI KICC_API_NOTI

    public class KICC_API_NOTI
    {
        public string res_cd { get; set; } = "";  // 응답코드
        public string res_msg { get; set; } = "";  // 응답메시지
        public string cno { get; set; } = "";  // PG거래번호
        public string memb_id { get; set; } = "";  // 가맹점 ID
        public string amount { get; set; } = "";  // 총 결제금액
        public string order_no { get; set; } = "";  // 주문번호
        public string noti_type { get; set; } = "";  // 노티구분 (승인:10, 변경:20, 입금:30, 환불:50)
        public string auth_no { get; set; } = "";  // 승인번호
        public string tran_date { get; set; } = "";  // 승인/변경 일시
        public string card_no { get; set; } = "";  // 카드번호
        public string issuer_cd { get; set; } = "";  // 발급사코드
        public string issuer_nm { get; set; } = "";  // 발급사명
        public string acquirer_cd { get; set; } = "";  // 매입사코드
        public string acquirer_nm { get; set; } = "";  // 매입사명
        public string install_period { get; set; } = "";  // 할부개월
        public string noint { get; set; } = "";  // 무이자여부 (00:일반거래, 02:가맹점 무이자, 03:이벤트 무이자)
        public string bank_cd { get; set; } = "";  // 은행코드
        public string bank_nm { get; set; } = "";  // 은행명
        public string account_no { get; set; } = "";  // 계좌번호
        public string deposit_nm { get; set; } = "";  // 입금자명
        public string escrow_yn { get; set; } = "";  // 에스크로 사용유무 (Y:사용, N:미사용)
        public string pay_type { get; set; } = "";  // 결제수단 (11:카드, 21:계좌이체, 22:가상계좌)
        public string cash_issue_yn { get; set; } = "";  // 현금영수증 발급유무 (1:발급, 0:미발급)
        public string cash_res_cd { get; set; } = "";  // 현금영수증 결과코드
        public string cash_tran_date { get; set; } = "";  // 현금영수증 발행일시
        public string cash_auth_no { get; set; } = "";  // 현금영수증 승인번호
        public string stat_cd { get; set; } = "";  // 에스크로 상태코드 (RF02:환불완료)
        public string stat_msg { get; set; } = "";  // 에스크로 상태코드 (RF02:환불완료)
        public string tlf_sno { get; set; } = "";  // 채번 거래번호
        public string account_type { get; set; } = "";  // 채번계좌 타입 (V:1회입금/회수, F:다회입금/회수)
        public string user_id { get; set; } = "";  // 고객 ID (거래처코드)
        public string user_nm { get; set; } = "";  // 고객명 (영업자코드)        
        public string canc_date { get; set; } = "";  // 취소일시
        public string noti_subtype { get; set; } = "";  // 결제상세수단 (PSE1:환불완료)
        public string depo_bkcd { get; set; } = "";  // 입금은행코드
        public string depo_bknm { get; set; } = "";  // 입금은행명
        public string reserve1 { get; set; } = "";  // 가맹점 DATA
    }
    #endregion

    #region 결제 환불/취소 KICC_API0005
    public class KICC_API0005_REQUEST
    {
        public string mallId { get; set; }                  //KICC에서 발급한 상점 ID
        public string shopTransactionId { get; set; }       //가맹점 트랜잭션ID
        public string pgCno { get; set; }                   //KICC 거래번호
        public string reviseTypeCode { get; set; }          //변경구분 (40 : 환불)
        public long amount { get; set; }                    //환불금액
        public string clientIp { get; set; }                //요청자 IP
        public string clientId { get; set; }                //요청자 ID
        public string msgAuthValue { get; set; }            //메시지 인증값
        public string cancelReqDate { get; set; }           //취소요청일자 (yyyymmdd)

        public refundInfoRequest refundInfo = new refundInfoRequest();
        public class refundInfoRequest
        {
            public string refundBankCode { get; set; }      //환불계좌 은행코드
            public string refundAccountNo { get; set; }     //환불계좌 계좌번호
            public string refundDepositName { get; set; }   //환불계좌 예금주명
        }
    }

    public class KICC_API0005_RESPONSE
    {
        public string resCd { get; set; }                   //결과코드
        public string resMsg { get; set; }                  //결과메시지
        public string mallId { get; set; }                  //KICC에서 발급한 상점 ID
        public string shopTransactionId { get; set; }       //KICC에서 발급한 상점 ID
        public string shopOrderNo { get; set; }             //KICC에서 발급한 상점 ID
        public string oriPgCno { get; set; }                //원거래번호
        public long cancelPgCno { get; set; }               //취소거래번호
        public string transactionDate { get; set; }         //거래일시 (yyyyMMddhhmmss)
        public long cancelAmount { get; set; }              //취소금액
        public long remainAmount { get; set; }              //남은금액
        public string statusCode { get; set; }              //거래상태코드
        public string statusMessage { get; set; }           //거래상태메시지

        public reviseInfoResponse reviseInfo = new reviseInfoResponse();
        public class reviseInfoResponse
        {
            public string payMethodTypeCode { get; set; }   //결제수단코드
            public string approvalNo { get; set; }          //결제수단 취소 승인번호
            public string approvalDate { get; set; }        //결제수단 취소 승인일시

            public refundInfoResponse refundInfo = new refundInfoResponse();
            public class refundInfoResponse
            {
                public string refundDate { get; set; }      //환불예정시간
                public string depositPgCno { get; set; }    //입금거래번호
            }
        }
    }
    #endregion
    #endregion



    public abstract class FactoryController
    {
        public abstract IApiTypeFactory MakeApiTypeFactory(string gubun);
    }

    public class ConcreateApiFactory : FactoryController
    {
        public override IApiTypeFactory MakeApiTypeFactory(string gubun)
        {
            switch (gubun)
            {
                case "post":
                    return new PostApiClientHelper(); //POST API
                case "get":
                    return new PostApiClientHelper(); //GET API TO-BE : 구현예정
                default:
                    return new PostApiClientHelper();
            }
        }
    }

    public interface IApiTypeFactory
    {
        string ApiAsync(string url, Object obj);
    }

    class PostApiClientHelper : Controller, IApiTypeFactory
    {
        static HttpClient client = new HttpClient();

        public string ApiAsync(string url, Object body)
        {
            try
            {
                #region api url 생성 및 parameter jsonString으로 변환                
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string param = serializer.Serialize(body); //모든 request를 처리할 수 있도록 객체를 jsonstring으로 변환
                #endregion

                var res = HttpClientHelper.apiRequest(url, param);
                return HttpClientHelper.streamEncode(res);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }

    #region httpClientHelper
    public static class HttpClientHelper
    {
        /// <summary>
        /// GET 방식 webapi 호출
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string RequestGet(string url)
        {
            //WebRequest.CreateHttp(url).;
            using (var web = new WebClient())
            {
                web.Encoding = Encoding.UTF8;
                return web.DownloadString(url);
            }
        }

        public static HttpWebResponse apiRequest(String url, String postData)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var request = (HttpWebRequest)WebRequest.Create(url);

            Encoding euckr = Encoding.GetEncoding(65001);// 51949:  euc-kr    , 65001:utf-8
            var data = euckr.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            request.Timeout = 5000;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var result = (HttpWebResponse)request.GetResponse();
            return result;
        }
        public static String apiGetRequest(String url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "Get";
            request.Timeout = 5000;
            var result = (HttpWebResponse)request.GetResponse();

            Stream ReceiveStream = result.GetResponseStream();
            Encoding encode = Encoding.GetEncoding(65001); // 51949:  euc-kr    , 65001:utf-8

            string responseText = string.Empty;
            string reutrnText = string.Empty;
            using (StreamReader sr = new StreamReader(ReceiveStream))
            {
                responseText = sr.ReadToEnd();
            }

            char[] arr = responseText.ToArray();
            bool bln = true;
            foreach (char item in arr)
            {
                if (bln)
                {
                    if (item == '}')
                    {
                        reutrnText += item.ToString();
                        break;
                    }
                    else
                    {
                        reutrnText += item.ToString();
                    }
                }
            }
            return reutrnText;
        }
        public static String streamEncode(HttpWebResponse result)
        {
            Stream ReceiveStream = result.GetResponseStream();
            Encoding encode = Encoding.GetEncoding(65001); // 51949:  euc-kr    , 65001:utf-8

            StreamReader sr = new StreamReader(ReceiveStream, encode);
            string resultText = sr.ReadToEnd();
            return resultText;
        }
    }
    #endregion

    #region 취소요청 암복호화
    public static class HmacText
    {
        public static string CreateHash(string message, string secretKey)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretKey);
            byte[] messageByte = encoding.GetBytes(message);  // PG거래번호|결제금액|거래일시

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashMessage = hmacsha256.ComputeHash(messageByte); // hash값을 HexString 으로 변환하세요.
                                                                          // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashMessage.Length; i++)
                {
                    builder.Append(hashMessage[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
    #endregion
}