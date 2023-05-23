using HM.Framework;
using HmPMer.AdminUI.Fillters;
using HmPMer.AdminUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.WebAdminUI.Models;
using System;
using System.Web;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Controllers
{
	public class BusinessController : Controller
	{
		private UserBaseBll bll = new UserBaseBll();

		[Auth(FlagStr = "BusinessList")]
		public ActionResult BusinessList()
		{
			return View();
		}

		[Auth(FlagStr = "BusinessList")]
		public ActionResult BusinessUpdate(string UserId)
		{
			UserBaseInfo modelForId = bll.GetModelForId(UserId);
			return View(modelForId);
		}

		[Auth(FlagStr = "BusinessList")]
		public ActionResult AddUc()
		{
			return View();
		}

		public ActionResult GetModelUserInfo(string UserId)
		{
			ResultBase resultBase = new ResultBase();
			UserBaseInfo modelForId = bll.GetModelForId(UserId);
			if (modelForId == null)
			{
				resultBase.Success = false;
			}
			resultBase.Data = modelForId;
			return Json(resultBase);
		}

		[Auth(FlagStr = "AuditBusinessList")]
		public ActionResult AuditBusinessList()
		{
			return View();
		}

		[Auth(FlagStr = "LockBusinessList")]
		public ActionResult LockBusinessList()
		{
			return View();
		}

		public ActionResult LoadUserPage(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			UserBaseInfo userBaseInfo = new UserBaseInfo
			{
				UserId = Utils.GetRequest("UserId"),
				MerName = Utils.GetRequest("MerName"),
				MobilePhone = Utils.GetRequest("MobilePhone"),
				Email = Utils.GetRequest("Email"),
				IsEnabled = Utils.GetRequestQueryToInt("IsEnabled", -1),
				UserType = Utils.GetRequestQueryToInt("UserType", -1),
				AloneRate = Utils.GetRequestQueryToInt("AloneRate", -1),
				AgentId = Utils.GetRequest("AgentId"),
				PromId = Utils.GetRequest("PromId"),
				Type = Utils.GetRequestQueryToInt("Type", 0)
			};
			ResultPage<UserBaseInfo> resultPage = new ResultPage<UserBaseInfo>();
			if (userBaseInfo.UserType == -1)
			{
				return resultPage;
			}
			resultPage.msg = "查询成功";
			resultPage.Item = bll.LoadUserPage(userBaseInfo, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		public ActionResult GetGoto(string Id)
		{
			string str = EncryUtils.AesEncrypt(Id + "_" + DateTime.Now.ToString("yyyyMMdd"));
			string url = new SysConfigBll().GetForKey("WebUrl").Value + "/account/hmadminin?id=" + HttpUtility.UrlEncode(str);
			return new RedirectResult(url);
		}

		[HttpPost]
		public ActionResult UpIsEnabled(string userId, int IsEnabled)
		{
			int num = bll.UpIsEnabled(userId, IsEnabled);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = ((IsEnabled == 1) ? "启用商户" : "禁用商户");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"userId={userId}&IsEnabled={IsEnabled}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		public ActionResult UpdatePayRate(string UserId)
		{
			UserBaseInfo modelForId = bll.GetModelForId(UserId);
			if (modelForId != null)
			{
				return PartialView(modelForId);
			}
			return PartialView();
		}

		[HttpPost]
		public ActionResult UpBusinessAgentPay(string UserId, int AgentPay)
		{
			int num = bll.UpBusinessAgentPay(UserId, AgentPay);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = ((AgentPay == 1) ? "启用商户代付" : "禁用商户代付");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"userId={UserId}&AgentPay={AgentPay}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult AddBusiness(UserBase Model)
		{
			ResultBase resultBase = new ResultBase();
			if (bll.GetModelForMobile(Model.MobilePhone, "") != null)
			{
				resultBase.Success = false;
				resultBase.Message = "此手机号码已经存在，请更换一个";
				return Json(resultBase);
			}
			if (bll.GetModelForEmail(Model.Email, "") != null)
			{
				resultBase.Success = false;
				resultBase.Message = "此邮箱号码已经存在，请更换一个";
				return Json(resultBase);
			}
			Model.IsEnabled = 1;
			long num = bll.AddBusiness(Model);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "新增失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "新增商户";
				behaviorLog.BlType = 1;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult BusinessUpdateInfo(UserBase Model)
		{
			ResultBase resultBase = new ResultBase();
			if (bll.GetModelForMobile(Model.MobilePhone, Model.UserId) != null)
			{
				resultBase.Success = false;
				resultBase.Message = "此手机号码已经存在，请更换一个";
				return Json(resultBase);
			}
			if (bll.GetModelForEmail(Model.Email, Model.UserId) != null)
			{
				resultBase.Success = false;
				resultBase.Message = "此邮箱已经存在，请更换一个";
				return Json(resultBase);
			}
			int num = bll.UpModelInfo(Model);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改商户";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelBusiness(string UserId)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.DelBusiness(UserId, ModelCommon.GetUserModel().AdmUser);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除商户";
				behaviorLog.BlType = 3;
				behaviorLog.parm = $"UserId={UserId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "AuditWithdrawAccount")]
		public ActionResult AuditWithdrawAccount()
		{
			return View();
		}

		[Auth(FlagStr = "AuditWithdrawAccount")]
		public ActionResult AuditWithdraw(string UserId)
		{
			UserBaseInfo modelForId = bll.GetModelForId(UserId);
			return View(modelForId);
		}

		[HttpPost]
		public ActionResult AuditUserWithdraw(int WithdrawStatus, string WithdrawAuditDes, string UserId)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.AuditUserWithdraw(WithdrawStatus, WithdrawAuditDes, UserId);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "审核商户结算卡号";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"WithdrawStatus={WithdrawStatus}&WithdrawAuditDes={WithdrawAuditDes}&UserId={UserId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		public ActionResult LoadUserDetailList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			UserDetailInfo param = new UserDetailInfo
			{
				UserId = Utils.GetRequest("UserId"),
				UserType = Utils.GetRequestQueryToInt("UserType", -1),
				WithdrawStatus = Utils.GetRequestQueryToInt("WithdrawStatus", -1)
			};
			ResultPage<UserDetailInfo> resultPage = new ResultPage<UserDetailInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetUserDetailList(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[Auth(FlagStr = "AuditIdCardIndex")]
		public ActionResult AuditIdCardIndex()
		{
			return View();
		}

		[Auth(FlagStr = "AuditIdCardIndex")]
		public ActionResult AuditIdCard(string UserId)
		{
			UserBaseInfo modelForId = bll.GetModelForId(UserId);
			return View(modelForId);
		}

		public ActionResult LoadUserIdCardList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			UserDetailInfo param = new UserDetailInfo
			{
				UserId = Utils.GetRequest("UserId"),
				UserType = Utils.GetRequestQueryToInt("UserType", -1),
				IdCardStatus = Utils.GetRequestQueryToInt("IdCardStatus", -1)
			};
			ResultPage<UserDetailInfo> resultPage = new ResultPage<UserDetailInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetUserIdCardDetailList(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult AuditIdCardInfo(int IdCardStatus, string IdCardAuditDes, string UserId)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.AuditIdCard(IdCardStatus, IdCardAuditDes, UserId);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "审核商户资质";
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"IdCardStatus={IdCardStatus}&IdCardAuditDes={IdCardAuditDes}&UserId={UserId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}
	}
}
