using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;

namespace PayServerAPI
{
    public class AlipayUtil
    {
        private static string APPID = "";//APPID
        private static string APP_PRIVATE_KEY;//商家私钥
        private static string ALIPAY_PUBLIC_KEY;//支付宝公钥
        private static string CHARSET = "utf-8";
        private static string sign_type = "RSA2";//商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，现在必须使用RSA2
        private static string format = "json";//仅支持JSON
        private static string version = "1.0";//调用的接口版本，固定为：1.0
        private static string gateway = "https://openapi.alipay.com/gateway.do";//请求网关(gateway)
        private static string product_code = "QUICK_MSECURITY_PAY";//销售产品码，商家和支付宝签约的产品码，为固定值QUICK_MSECURITY_PAY


        //支付宝服务器主动通知商户服务器里指定的页面http/https路径(异步通知地址)
        //请填入自己的接口路径
        private static string notify_url = ".../Alipay/GetAsynchronousYanQian";


        /// <summary>
        /// 生成RSA签名后的订单字符串，给客户端用的
        /// </summary>
        /// <param name="body"></param>
        /// <param name="subject"></param>
        /// <param name="out_trade_no"></param> 
        /// <param name="timeout_express"></param> 
        /// <param name="total_amount"></param> 
        /// <param name="product_code"></param>
        /// <param name="notify_url"></param> 
        /// <returns></returns>
        public static string CreateRSASignedOrderString(string body, string subject, string out_trade_no, string timeout_express, string total_amount)
        {
            //从pem文件中读取
            APP_PRIVATE_KEY = GetCurrentPathByAbs() + "rsa_private_key.pem";
            ALIPAY_PUBLIC_KEY = GetCurrentPathByAbs() + "rsa_alipay_public_key.pem";


            //最后一个参数为false直接将私钥内容写入代码，true从pem文件中读取
            IAopClient client = new DefaultAopClient(gateway, APPID, APP_PRIVATE_KEY, format, version, sign_type, ALIPAY_PUBLIC_KEY, CHARSET, true);
            //实例化具体API对应的request类,类名称和接口名称对应,当前调用接口名称如：alipay.trade.app.pay
            AlipayTradeAppPayRequest request = new AlipayTradeAppPayRequest();
            //SDK已经封装掉了公共参数，这里只需要传入业务参数。以下方法为sdk的model入参方式(model和biz_content同时存在的情况下取biz_content)。
            AlipayTradeAppPayModel model = new AlipayTradeAppPayModel();
            model.Body = body;
            model.Subject = subject;
            model.TotalAmount = total_amount;
            model.ProductCode = product_code;
            model.OutTradeNo = out_trade_no;
            model.TimeoutExpress = timeout_express;
            request.SetBizModel(model);
            request.SetNotifyUrl(notify_url);
            //这里和普通的接口调用不同，使用的是sdkExecute
            AlipayTradeAppPayResponse response = client.SdkExecute(request);

            string resp = response.Body;
            return resp;
        }

        /// <summary>
        /// 获取私钥公钥的路径(绝对路径)
        /// </summary>
        /// <returns></returns>
        private static string GetCurrentPathByAbs()
        {
            return HttpRuntime.AppDomainAppPath.ToString() + "rsa/";
        }


        /// <summary>
        /// 异步验签(验签成功后将改变支付状态,并通知支付宝)
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string AsynchronousYanQian(Dictionary<string, string> result)
        {
            //这里可以取出相关的值
            string buyer_pay_amount = result["buyer_pay_amount"];//用户在交易中支付的金额
            string out_trade_no = result["out_trade_no"];//原支付请求的商户订单号（订单id）

            //从pem文件中读取支付宝公钥
            ALIPAY_PUBLIC_KEY = GetCurrentPathByAbs() + "rsa_alipay_public_key.pem";

            bool flag;
            try
            {
                flag = AlipaySignature.RSACheckV1(result, ALIPAY_PUBLIC_KEY, CHARSET, sign_type, true);

                WriteLog("支付宝异步验签结果为：" + flag + "\n");
            }
            catch (Exception e)
            {
                return e.ToString();
            }


            if (flag)
            {
                //验签成功，将数据库中订单的支付状态改变
                if (ChangePayState())
                {
                    //如果数据库插入成功，通知支付宝已收到通知
                    return "success";
                }
                else
                {
                    return "fail";
                }

            }
            else
            {
                return "fail";
            }

        }

        /// <summary>
        /// 记录支付宝异步调用日志
        /// </summary>
        /// <param name="log"></param>
        public static void WriteLog(string log)
        {
            FileStream fs = new FileStream(".这里是日志保存路径.." + "alipayapi_log.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(log);

            sw.Flush();
            sw.Close();
            fs.Close();
        }



        /// <summary>
        /// 从数据库改变这个订单的支付状态
        /// </summary>
        private static bool ChangePayState()
        {

            //这里做数据库相关操作,操作成功返回true，否则false
            //.......
            return true;
        }



        /// <summary>
        /// 获取当前路径
        /// </summary>
        /// <returns></returns>
        private static string GetCurrentPath()
        {
            string strPath = "/rsa/";
            // Server.MapPath("./")
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath(strPath);
            }
            else //非web程序引用 
            {
                strPath = strPath.Replace("/", "\\");
                if (strPath.StartsWith("\\"))
                {
                    //strPath = strPath.Substring(strPath.IndexOf('\\', 1)).TrimStart('\\'); 
                    strPath = strPath.TrimStart('\\');
                }
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
            }
        }

        public static bool GetSynYanQian(bool isget)
        {
            ALIPAY_PUBLIC_KEY = GetCurrentPathByAbs() + "rsa_alipay_public_key.pem";

            string str = "";
            string sign = "";
            string signtype = "RSA";//签名类型 
            string[] allkeys;
            if (isget)
            {
                allkeys = HttpContext.Current.Request.QueryString.AllKeys;
            }
            else
            {
                allkeys = HttpContext.Current.Request.Form.AllKeys;
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var item in allkeys)
            {
                if (item != "sign" & item != "sign_type")
                {
                    dic.Add(item, HttpContext.Current.Request[item]);
                }
                if (item == "sign")
                {
                    sign = HttpContext.Current.Request[item];
                }
                else if (item == "sign_type")
                {
                    signtype = HttpContext.Current.Request[item];
                }
            }

            var newdic = dic.OrderBy(u => u.Key);

            foreach (var item in newdic)
            {
                str = str + item.Key + "=" + item.Value + "&";
            }
            str = str.Substring(0, str.Length - 1);
            return AlipaySignature.RSACheckContent(str, sign, ALIPAY_PUBLIC_KEY, "utf-8", signtype, true);

        }
    }
}