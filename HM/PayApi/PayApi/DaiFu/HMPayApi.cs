using HM.Framework.Logging;
using HM.Framework.PayApi.ShangDe.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace HM.Framework.PayApi.DaiFu
{
	public class HMPayApi : HMPayApiBase
	{
		public override string NOTIFY_SUCCESS => "SUCCESS";

		public override string NOTIFY_FAIL => "FAIL";

		public override bool IsWithdraw => true;

		private string GetChannelCode(HMChannel channel)
		{
            return string.Empty;
        }


		public override HMMode GetPayMode(HMChannel code)
		{
            return HMMode.输出字符串;
        }

		protected override HMPayResult PayGatewayBody(HMOrder order)
		{
			HMPayResult fail = HMPayResult.Fail;
            fail.Message = "此通道暂不支持";
            return fail;
        }

		protected override Dictionary<string, string> GetNotifyParam()
		{
            LogUtil.Debug("代付,GetNotifyParam 进入");
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            try
            {
                string[] allKeys = HttpContext.Current.Request.Form.AllKeys;
                foreach (string text in allKeys)
                {
                    string request = Utils.GetRequest(text.ToString());
                    dictionary.Add(text.ToString(), request);
                }
                LogUtil.Debug("GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("代付：GetNotifyParam", exception);
                return dictionary;
            }
        }

		protected override Dictionary<string, string> GetReturnParam()
		{
            LogUtil.Debug("代付,GetNotifyParam 进入");
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            try
            {
                string[] allKeys = HttpContext.Current.Request.Form.AllKeys;
                foreach (string text in allKeys)
                {
                    string request = Utils.GetRequest(text.ToString());
                    dictionary.Add(text.ToString(), request);
                }
                LogUtil.Debug("GetNotifyParam=" + dictionary.ToJson());
                return dictionary;
            }
            catch (Exception exception)
            {
                LogUtil.Error("代付：GetNotifyParam", exception);
                return dictionary;
            }
        }

		protected override HMNotifyResult<HMOrder> NotifyParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
			fail.Message = "参数验证失败";
			return fail;
		}

		protected override HMNotifyResult<HMOrder> ReturnParamToOrder(Dictionary<string, string> dic)
		{
			HMNotifyResult<HMOrder> fail = HMNotifyResult<HMOrder>.Fail;
            fail.Message = "参数验证失败";
            return fail;
		}

		public override HMNotifyResult<string> NotifySign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Code = HMNotifyState.Success;
			fail.Data = NOTIFY_SUCCESS;
			return fail;
		}

		public override HMNotifyResult<string> ResultSign(HMOrder order, HMSupplier supplier, HMAccount account)
		{
			HMNotifyResult<string> fail = HMNotifyResult<string>.Fail;
			fail.Code = HMNotifyState.Success;
			fail.Data = NOTIFY_SUCCESS;
			return fail;
		}

        private Dictionary<string, string> encryptWithdraw(Dictionary<string, string> paraMap)
        {
            string dataMapJson = paraMap.ToJson();


            string dataMapJsonBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(dataMapJson));
            string sign = RSACryption.SignatureFormatter(
                RSAKeyConvert.RSAPrivateKeyJava2DotNet(File.ReadAllText(base.Account.RsaPrivate)),
                dataMapJsonBase64);
            
            String reqData = RSACryption.Encrypt(RSAKeyConvert.RSAPublicKeyJava2DotNet(base.Account.RsaPublic),
               dataMapJsonBase64);

            Dictionary<String, String> requestMap = new Dictionary<String, String>();
            requestMap.Add("sign", sign);
            requestMap.Add("data", reqData);

            return requestMap;
        }


        protected override HMPayResult WithdrawGatewayBody(HMWithdraw withdraw)
		{
			HMPayResult fail = HMPayResult.Fail;

			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("operator_id", "001");
			dictionary.Add("settType", "1");
			dictionary.Add("ppType", "0");
			dictionary.Add("orderId", base.Account.AccountUser+withdraw.OrderNo);
			dictionary.Add("dfje", withdraw.Amount.ToString());
			dictionary.Add("backUrl", "127.0.0.1:8080");
			dictionary.Add("skryhmc", withdraw.BankName);
			dictionary.Add("skrkhhh", "");
            dictionary.Add("skryhzh", withdraw.BankCode);
            dictionary.Add("skrxm", withdraw.FactName);
			dictionary.Add("skrsjh", withdraw.MobilePhone);

			dictionary.Add("bz", "付款" + withdraw.FactName + "," + withdraw.Amount.ToString());

            Dictionary<String, String> requestMap = encryptWithdraw(dictionary);
            requestMap.Add("serviceCode", "DF12");//代付服务代码
            requestMap.Add("account", base.Account.AccountUser);   //商户代号（代付平台分配的唯一识别号）
            string requestStr = requestMap.ToJson();
            string result = HttpUtils.SendRequest(base.Supplier.AgentPayUrl, new Dictionary<string, string>() {{
                        "reqParam",
                        requestStr
                    }, }, "POST", "utf-8", "");

            Dictionary<string, object> dataMapJson = result.FormJson<Dictionary<string, object>>();

            String serviceCode = dataMapJson["serviceCode"].ToString();//
            String account = dataMapJson["account"].ToString();
            String respCode = dataMapJson["respCode"].ToString();
            String respInfo = dataMapJson["respInfo"].ToString();
            String sign = dataMapJson["sign"].ToString();
            String data = dataMapJson["data"].ToString();

            if (!respCode.Equals("0000"))
            {
                fail.Message = respCode + ":" + respInfo;
                return fail;
            }
            else
            {
                byte[] databyts = Convert.FromBase64String(data);

                byte[] encodedData = RSACryption.Decrypt(
                    RSAKeyConvert.RSAPrivateKeyJava2DotNet(File.ReadAllText(base.Account.RsaPrivate)),
                    databyts);
                string encodedStr = System.Text.Encoding.UTF8.GetString(encodedData);
                //只有交易成功的时候才需要验签和解密数据
                bool vfy = RSACryption.SignatureDeformatter(RSAKeyConvert.RSAPublicKeyJava2DotNet(base.Account.RsaPublic), encodedData, Convert.FromBase64String(sign));
                if(vfy)
                {
                    string rsStr = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedStr));
                    fail.Code = HMPayState.Success;
                    fail.Data = "成功";

                    return fail;
                } else
                {
                    fail.Message = "验签失败";
                    return fail;
                }
                
            }
            return fail;
        }

		protected override HMPayResult WithdrawQueryBody(HMWithdraw withdraw)
		{
            HMPayResult fail = HMPayResult.Fail;
            fail.Message = "此通道暂不支持";
            return fail;
        }
	}
}
