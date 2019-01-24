# AlipayServerAPIDemo
支付宝支付服务端（.Net）demo，WebAPI

服务端主要工作为加签、验签（异步通知验证）。

### 加签

核心步骤：

```java
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
```

支付宝服务端SDK中提供了一个IAopClient对象的SdkExcute方法，需要传入对应的request类AlipayTradeAppPayRequest。实例化IAopClient对象时需要传入请求网关(gateway)、APPID、应用私钥、支付宝公钥、调用的接口版本、编码方式、签名算法类型以及是否从文件中读取密钥的一个bool类型值。



<br>

### 验签

验签过程支付宝AlipaySignature类提供了验签方法RSACheckV1和RSACheckV2，区别就不细说，这里使用RSACheckV1方法，第一个参数便是支付宝返回的异步通知结果，第二个参数传入支付宝公钥，同样最后一个参数传false从代码读取支付宝公钥，传true从pem 文件中读取。


```java
bool flag = AlipaySignature.RSACheckV1(result, ALIPAY_PUBLIC_KEY, CHARSET, sign_type, true);
```





