using HM.Framework;
using HM.Framework.Logging;
using HmPMer.AdminUI.Fillters;
using HmPMer.AdminUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.WebAdminUI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Controllers
{
	public class HomeController : Controller
	{
		[Auth(NoLogin = true)]
		public ActionResult Login()
		{
			return View();
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult NoFlag()
		{
			return View();
		}

		[Auth(NoLogin = true)]
		public ActionResult LoginIn(string user, string pwd, string code)
		{
			ApiResult<HmAdmin> failing = ApiResult<HmAdmin>.Failing;
			if (string.IsNullOrEmpty(user))
			{
				failing.message = "用户名不能为空";
				return failing;
			}
			if (string.IsNullOrEmpty(pwd))
			{
				failing.message = "密码不能为空";
				return failing;
			}
			if (string.IsNullOrEmpty(code))
			{
				failing.message = "验证码不能为空";
				return failing;
			}
			string text = base.Session["code"] as string;
			if (string.IsNullOrEmpty(text))
			{
				failing.message = "验证码已过期";
				return failing;
			}
			if (!text.ToUpper().Equals(code.ToUpper()))
			{
				failing.message = "验证码不正确";
				return failing;
			}
			AccountBll accountBll = new AccountBll();
			HmAdmin model = new HmAdmin
			{
				AdmUser = user.Trim(),
				AdmPass = EncryUtils.MD5(pwd.Trim())
			};
			model = accountBll.AdminLoginIn(model);
			if (model != null)
			{
				if (model.IsEnable == 0)
				{
					failing.message = "该账户被禁用，无法登录!";
					return failing;
				}
				try
				{
					accountBll.UpdateLastLoginIp(Utils.GetClientIp(), user);
					BehaviorLog behaviorLog = new BehaviorLog();
					behaviorLog.Id = Guid.NewGuid().ToString();
					behaviorLog.BlName = "管理员登录";
					behaviorLog.BlType = 4;
					behaviorLog.parm = "user=" + user + " ,ip=" + Utils.GetClientIp();
					behaviorLog.createUser = "";
					behaviorLog.addTime = DateTime.Now;
					new SystemBll().InserBehaviorLog(behaviorLog);
				}
				catch (Exception)
				{
				}
				ModelCommon.WriteUserModel(model);
				failing.IsSuccess = true;
				failing.data = new HmAdmin
				{
					ID = model.ID,
					AdmUser = model.AdmUser,
					NickName = model.NickName,
					Flag = model.Flag,
					IsEnable = model.IsEnable
				};
				failing.message = "登录成功";
			}
			else
			{
				LogUtil.InfoFormat("登录日志失败：user={0}, pwd={1}, ip={2}", user, pwd, Utils.GetClientIp());
				failing.message = "登录失败,用户名或者密码不正确！";
			}
			return failing;
		}

		[Auth(NoLogin = true)]
		public ActionResult LoginOut()
		{
			ModelCommon.RemoveUserModel();
			return new RedirectResult("/home/login");
		}

		[HttpGet]
		public ActionResult GetOrderPayScale()
		{
			OrderBll orderBll = new OrderBll();
			List<OrderPayScale> orderPayScale = orderBll.GetOrderPayScale(DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));
			Hashtable hashtable = new Hashtable();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (OrderPayScale item in orderPayScale)
			{
				list.Add(item.PayName + "(" + (item.SumAmt / 100m).ToString("0.00") + "元)");
				list2.Add(item.PayScale.ToString("0.00"));
			}
			hashtable.Add("PayName", list);
			hashtable.Add("PayScale", list2);
			return Json(hashtable, JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetOrderPayTime()
		{
			OrderBll orderBll = new OrderBll();
			List<OrderCountInfo> orderPayTime = orderBll.GetOrderPayTime(DateTime.Now.AddDays(-7.0).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));
			Hashtable hashtable = new Hashtable();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			decimal num = default(decimal);
			decimal num2 = default(decimal);
			foreach (OrderCountInfo item in orderPayTime)
			{
				num += item.TodayOrderCount;
				num2 += item.TodayPayOrderCount;
				list.Add(item.CountDate);
				list2.Add(item.TodayOrderCount.ToString());
				list3.Add(item.TodayPayOrderCount.ToString());
			}
			hashtable.Add("countorder", num);
			hashtable.Add("paycountorder", num2);
			hashtable.Add("CountDate", list);
			hashtable.Add("TodayOrderCount", list2);
			hashtable.Add("TodayPayOrderCount", list3);
			return Json(hashtable, JsonRequestBehavior.AllowGet);
		}
	}
}
