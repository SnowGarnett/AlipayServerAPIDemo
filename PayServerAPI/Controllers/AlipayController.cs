using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PayServerAPI.Controllers
{
    public class AlipayController : Controller
    {

        // GET: Alipay
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 加签(将订单字符串加签，返回客户端用于调起支付宝)
        /// </summary>
        /// <param name="body"></param> 对一笔交易的具体描述信息
        /// <param name="subject"></param> 商品的标题/交易标题/订单标题/订单关键字等。
        /// <param name="out_trade_no"></param> 商户网站唯一订单号
        /// <param name="timeout_express"></param> 该笔订单允许的最晚付款时间，逾期将关闭交易。
        /// <param name="total_amount"></param> 订单总金额，单位为元，精确到小数点后两位，取值范围[0.01,100000000]
        /// <returns></returns>
        [HttpPost]
        public string GetRSASignedOrder(string body, string subject, string out_trade_no, string timeout_express, string total_amount)
        {
            return AlipayUtil.CreateRSASignedOrderString(body, subject, out_trade_no, timeout_express, total_amount);
        }

        /// <summary>
        /// 异步验签（客户端支付后，支付宝服务器将异步调用此方法）
        /// 实际支付逻辑在此操作，支付宝服务端将保证调用到此接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string GetAsynchronousYanQian()
        {
            return AlipayUtil.AsynchronousYanQian(GetRequestPost());
        }


        /// <summary>
        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组 
        /// request回来的信息组成的数组
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetRequestPost()
        {
            int i = 0;
            IDictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            coll = Request.Form;

            String[] requestItem = coll.AllKeys;

            string log = "\n=========================================" + DateTime.Now.ToString() + " 支付宝异步返回" + "=================================================\n";
            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);

                log += requestItem[i] + ":" + Request.Form[requestItem[i]] + " \n";
            }
            AlipayUtil.WriteLog(log);
            return (Dictionary<string, string>)sArray;
        }



        /// <summary>
        /// 同步验签(App正常支付，支付宝服务端同步返回的报文需要上传到这进行验签，使用服务端sdk里的RSACheckContent()方法)
        /// ps:如果客户端同步通知只做个提示，此方法可不用，以异步通知为准
        /// </summary>
        /// <returns></returns>
        public string getSynYanQian()
        {
            return AlipayUtil.GetSynYanQian(true) + "";
        }

    }
}