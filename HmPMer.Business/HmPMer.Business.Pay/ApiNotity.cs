using HM.Framework;
using HM.Framework.Logging;
using HmPMer.Entity;
using System;
using System.Text;

namespace HmPMer.Business.Pay
{
	public class ApiNotity
	{
		public static void NotifyOrder(OrderBase order)
		{
			if (order != null && !string.IsNullOrEmpty(order.NotifyUrl))
			{
				try
				{
					bool flag = false;
					DateTime now = DateTime.Now;
					OrderBll orderBll = new OrderBll();
					OrderNotity orderNotity = orderBll.GetOrderNotity(order.OrderId);
					if (orderNotity == null)
					{
						UserBaseInfo modelForId = new UserBaseBll().GetModelForId(order.UserId);
						orderNotity = new OrderNotity
						{
							NotityId = Guid.NewGuid().ToString(),
							OrderId = order.OrderId,
							NotityState = OrderState.Pending.ToInt(),
							NotityCount = 1,
							NotityTime = now,
							AddTime = now,
							NotityUrl = CreateNotifyUrl(order, isNotify: true, modelForId.ApiKey),
							ReturnUrl = CreateNotifyUrl(order, isNotify: false, modelForId.ApiKey)
						};
						LogUtil.Error("notify=" + orderNotity.ToJson());
						flag = orderBll.InsertOrderNotity(orderNotity);
						goto IL_013e;
					}
					if (orderNotity.NotityState == OrderState.Pending.ToInt() && !(orderNotity.NotityTime > now))
					{
						orderNotity.NotityCount++;
						orderNotity.NotityTime = now;
						flag = true;
						goto IL_013e;
					}
					goto end_IL_0012;
					IL_013e:
					if (flag)
					{
						string text2 = orderNotity.NotityContext = HttpUtils.SendRequest(orderNotity.NotityUrl, "");
						if (!string.IsNullOrEmpty(order.Version) && order.Version.Equals("HM.591") && text2.ToLower().StartsWith("opstate=0"))
						{
							orderNotity.NotityState = OrderState.Success.ToInt();
						}
						else if (text2.ToUpper().StartsWith("SUCCESS"))
						{
							orderNotity.NotityState = OrderState.Success.ToInt();
						}
						else if (orderNotity.NotityCount >= 5)
						{
							orderNotity.NotityState = OrderState.Fail.ToInt();
						}
						else
						{
							orderNotity.NotityCount++;
							orderNotity.NotityTime = orderNotity.NotityTime.Value.AddMinutes(5.0);
						}
						orderBll.EditOrderNotity(orderNotity);
					}
					end_IL_0012:;
				}
				catch (Exception exception)
				{
					LogUtil.Error("生成通知失败", exception);
				}
			}
		}

		public static string CreateNotifyUrl(OrderBase model, bool isNotify, string apiKey)
		{
			string empty = string.Empty;
			if (model == null || string.IsNullOrEmpty(apiKey))
			{
				return empty;
			}
			empty = (isNotify ? model.NotifyUrl : model.ReturnUrl);
			if (string.IsNullOrEmpty(empty))
			{
				return empty;
			}
			if (model.Version == "HM.591")
			{
				return CreateNotifyUrl_HM591(model, isNotify, apiKey);
			}
			if (model.Version == "V2.0")
			{
				return CreateNotifyUrl_V20(model, isNotify, apiKey);
			}
			return CreateNotifyUrl_Default(model, isNotify, apiKey);
		}

		private static string CreateNotifyUrl_Default(OrderBase model, bool isNotify, string apiKey)
		{
			string empty = string.Empty;
			string arg = EncryUtils.MD5(string.Format("orderid={0}&opstate={1}&ovalue={2}{3}", model.MerOrderNo, model.PayState, model.MerOrderAmt.ToString("0"), apiKey));
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("orderid={0}", model.MerOrderNo);
			stringBuilder.AppendFormat("&sysorderid={0}", model.OrderId);
			stringBuilder.AppendFormat("&opstate={0}", model.PayState);
			stringBuilder.AppendFormat("&ovalue={0}", model.MerOrderAmt.ToString("0"));
			stringBuilder.AppendFormat("&systime={0:yyyy-MM-dd HH:mm:ss}", model.PayTime);
			stringBuilder.AppendFormat("&attach={0}", model.Attach);
			stringBuilder.AppendFormat("&sign={0}", arg);
			string obj = isNotify ? model.NotifyUrl : model.ReturnUrl;
			return obj + ((obj.IndexOf('?') == -1) ? "?" : "&") + stringBuilder.ToString();
		}

		private static string CreateNotifyUrl_V20(OrderBase model, bool isNotify, string apiKey)
		{
			string empty = string.Empty;
			string arg = EncryUtils.MD5(string.Format("out_trade_no={0}&total_amount={1}&trade_status={2}{3}", model.MerOrderNo, model.MerOrderAmt.ToString("0"), (model.PayState == PayState.Success.ToInt()) ? "SUCCESS" : model.PayState.ToString(), apiKey));
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("out_trade_no={0}", model.MerOrderNo);
			stringBuilder.AppendFormat("&trade_no={0}", model.OrderId);
			stringBuilder.AppendFormat("&trade_status={0}", (model.PayState == PayState.Success.ToInt()) ? "SUCCESS" : model.PayState.ToString());
			stringBuilder.AppendFormat("&extra_return_param={0}", model.Attach);
			stringBuilder.AppendFormat("&total_amount={0}", model.MerOrderAmt.ToString("0"));
			stringBuilder.AppendFormat("&trade_time={0:yyyy-MM-dd HH:mm:ss}", model.PayTime);
			stringBuilder.AppendFormat("&sign={0}", arg);
			string obj = isNotify ? model.NotifyUrl : model.ReturnUrl;
			return obj + ((obj.IndexOf('?') == -1) ? "?" : "&") + stringBuilder.ToString();
		}

		private static string CreateNotifyUrl_HM591(OrderBase model, bool isNotify, string apiKey)
		{
			string empty = string.Empty;
			int num = model.PayState;
			string text = (model.MerOrderAmt / 100m).ToString("0.00");
			if (num == PayState.Success.ToInt())
			{
				num = 0;
			}
			else if (num == 0)
			{
				num = 1;
			}
			string arg = EncryUtils.MD5($"orderid={model.MerOrderNo}&opstate={num}&ovalue={text}{apiKey}");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("orderid={0}", model.MerOrderNo);
			stringBuilder.AppendFormat("&sysorderid={0}", model.OrderId);
			stringBuilder.AppendFormat("&opstate={0}", num);
			stringBuilder.AppendFormat("&ovalue={0}", text);
			stringBuilder.AppendFormat("&systime={0:yyyy-MM-dd HH:mm:ss}", model.PayTime);
			stringBuilder.AppendFormat("&attach={0}", model.Attach);
			stringBuilder.AppendFormat("&sign={0}", arg);
			string obj = isNotify ? model.NotifyUrl : model.ReturnUrl;
			return obj + ((obj.IndexOf('?') == -1) ? "?" : "&") + stringBuilder.ToString();
		}
	}
}
