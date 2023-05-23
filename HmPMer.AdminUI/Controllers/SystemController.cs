using HM.Framework;
using HmPMer.AdminUI.Fillters;
using HmPMer.AdminUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.WebAdminUI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Controllers
{
	public class SystemController : Controller
	{
		private SystemBll bll = new SystemBll();

		[Auth(FlagStr = "EmailSetIndex")]
		public ActionResult EmailSetIndex()
		{
			EmailSet emailSetForCode = bll.GetEmailSetForCode("QQ");
			if (emailSetForCode != null)
			{
				return View(emailSetForCode);
			}
			return View();
		}

		[HttpPost]
		public ActionResult UpdateEmailSet(EmailSet Model)
		{
			ResultBase resultBase = new ResultBase();
			resultBase.Success = bll.UpdateEmailSet(Model);
			if (resultBase.Success)
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改邮箱配置";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		public ActionResult TestSendEmail(string ReceiveAddress, string Title, string Body)
		{
			ResultBase resultBase = new ResultBase();
			resultBase.Success = bll.SendMail(ReceiveAddress, Title, Body);
			if (!resultBase.Success)
			{
				resultBase.Message = "发送失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "发送测试邮件";
				behaviorLog.BlType = 1;
				behaviorLog.parm = $"ReceiveAddress={ReceiveAddress}&Title={Title}&Body={Body}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "SmsSetIndex")]
		public ActionResult SmsSetIndex()
		{
			SmsSet smsSetForCode = bll.GetSmsSetForCode("DXB");
			if (smsSetForCode != null)
			{
				return View(smsSetForCode);
			}
			return View();
		}

		[HttpPost]
		public ActionResult UpdateSmsSet(SmsSet Model)
		{
			ResultBase resultBase = new ResultBase();
			resultBase.Success = bll.UpdateSmsSet(Model);
			if (resultBase.Success)
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改短信配置";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult SendSms(string mobile, string content)
		{
			ResultBase resultBase = new ResultBase();
			resultBase.Success = bll.SendSms(mobile, content);
			if (!resultBase.Success)
			{
				resultBase.Message = "发送失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "发送测试短信";
				behaviorLog.BlType = 1;
				behaviorLog.parm = $"mobile={mobile}&content={content}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "SmsModelIndex")]
		public ActionResult SmsModelIndex()
		{
			List<SmsModel> smsModelList = bll.GetSmsModelList();
			if (smsModelList != null)
			{
				return View(smsModelList);
			}
			return View();
		}

		[HttpPost]
		public ActionResult UpdateSmsModel(List<SmsModel> Model)
		{
			ResultBase resultBase = new ResultBase();
			resultBase.Success = bll.UpdateSmsModel(Model);
			if (!resultBase.Success)
			{
				resultBase.Message = "操作成功！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改短信模板";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "SmsTransIndex")]
		public ActionResult SmsTransIndex()
		{
			return View();
		}

		public ActionResult GetSmsTransPageList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			SmsTrans parm = new SmsTrans
			{
				Mobile = Utils.GetRequest("Mobile")
			};
			ResultPage<SmsTrans> resultPage = new ResultPage<SmsTrans>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetSmsTransPageList(parm, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		public ActionResult BackData()
		{
			return View();
		}

		[HttpPost]
		public ActionResult BackDataBase(string Path)
		{
			ResultBase resultBase = new ResultBase();
			try
			{
				int num = bll.BackDataBase(Path);
			}
			catch (Exception ex)
			{
				resultBase.Success = false;
				resultBase.Message = "备份失败:" + ex.Message;
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "PayTypeQuotaIndex")]
		public ActionResult PayTypeQuotaIndex()
		{
			List<PayTypeQuota> payTypeQuotaList = bll.GetPayTypeQuotaList();
			return View(payTypeQuotaList);
		}

		[HttpPost]
		public ActionResult SetPayTypeQuota(List<PayTypeQuota> ListModel)
		{
			ResultBase resultBase = new ResultBase();
			try
			{
				long num = bll.SetPayTypeQuota(ListModel);
				if (num < 0)
				{
					resultBase.Success = false;
					resultBase.Message = "修改失败!";
				}
				else
				{
					BehaviorLog behaviorLog = new BehaviorLog();
					behaviorLog.Id = Guid.NewGuid().ToString();
					behaviorLog.BlName = "修改交易设置";
					behaviorLog.BlType = 2;
					behaviorLog.parm = ListModel.ToJson();
					behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
					behaviorLog.addTime = DateTime.Now;
					new SystemBll().InserBehaviorLog(behaviorLog);
				}
			}
			catch (Exception ex)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败:" + ex.Message;
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "WebConfigIndex")]
		public ActionResult WebConfigIndex()
		{
			List<SysConfig> sysConfig = new SysConfigBll().GetSysConfig(1);
			return View(sysConfig);
		}

		[Auth(FlagStr = "RegConfigIndex")]
		public ActionResult RegConfigIndex()
		{
			List<SysConfig> sysConfig = new SysConfigBll().GetSysConfig(2);
			return View(sysConfig);
		}

		[Auth(FlagStr = "WithdrawSetIndex")]
		public ActionResult WithdrawSetIndex()
		{
			List<SysConfig> sysConfig = new SysConfigBll().GetSysConfig(3);
			return View(sysConfig);
		}

		[HttpPost]
		public ActionResult SetSysConfig(List<SysConfig> ListModel, int Type)
		{
			ResultBase resultBase = new ResultBase();
			try
			{
				long num = new SysConfigBll().SetSysConfig(ListModel, Type);
				if (num < 0)
				{
					resultBase.Success = false;
					resultBase.Message = "修改失败!";
				}
				else
				{
					BehaviorLog behaviorLog = new BehaviorLog();
					behaviorLog.Id = Guid.NewGuid().ToString();
					behaviorLog.BlName = "修改系统字典";
					behaviorLog.BlType = 2;
					behaviorLog.parm = ListModel.ToJson();
					behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
					behaviorLog.addTime = DateTime.Now;
					new SystemBll().InserBehaviorLog(behaviorLog);
				}
			}
			catch (Exception ex)
			{
				resultBase.Success = false;
				resultBase.Message = "修改失败:" + ex.Message;
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "NoticeIndex")]
		public ActionResult NoticeIndex()
		{
			return View();
		}

		[Auth(FlagStr = "NoticeIndex")]
		public ActionResult NoticeAdd()
		{
			return View();
		}

		[Auth(FlagStr = "NoticeIndex")]
		public ActionResult NoticeUpdate(string Id)
		{
			Notice noticeModel = bll.GetNoticeModel(Id);
			return View(noticeModel);
		}

		public ActionResult GetNoticePageList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			Notice parm = new Notice
			{
				Title = Utils.GetRequest("Title")
			};
			ResultPage<Notice> resultPage = new ResultPage<Notice>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetNoticePageList(parm, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult AddNoticeInfo(Notice Model)
		{
			Model.Id = Guid.NewGuid().ToString();
			Model.Addtime = DateTime.Now;
			Model.IsRelease = 0;
			long num = bll.AddNotice(Model);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "新增失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "新增公告";
				behaviorLog.BlType = 1;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult UpdateNoticeInfo(Notice Model)
		{
			int num = bll.UpdateNotice(Model);
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
				behaviorLog.BlName = "修改公告";
				behaviorLog.BlType = 2;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult ReleaseNotice(string Id)
		{
			Notice notice = new Notice();
			notice.Id = Id;
			notice.IsRelease = 1;
			int num = bll.ReleaseNotice(notice);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "发布失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "发布公告";
				behaviorLog.BlType = 2;
				behaviorLog.parm = notice.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult DelNotice(string Id)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.DelNotice(Id);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除公告";
				behaviorLog.BlType = 3;
				behaviorLog.parm = $"Id={Id}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[Auth(FlagStr = "BankLasalleIndex")]
		public ActionResult BankLasalleIndex()
		{
			return View();
		}

		public ActionResult GetBankLasalleList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			BankLasalleInfo parm = new BankLasalleInfo
			{
				BankLasalleName = Utils.GetRequest("BankLasalleName"),
				Proid = Utils.GetRequestQueryToInt("ProvinceId", -1),
				Cityid = Utils.GetRequestQueryToInt("CityId", -1)
			};
			ResultPage<BankLasalleInfo> resultPage = new ResultPage<BankLasalleInfo>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetBankLasalleList(parm, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		public ActionResult LoadCityList(string id)
		{
			ApiResult<List<District>> success = ApiResult<List<District>>.Success;
			if (string.IsNullOrEmpty(id))
			{
				success.data = new List<District>();
				return success;
			}
			DistrictBll districtBll = new DistrictBll();
			List<District> list2 = success.data = districtBll.LoadParentId(Utils.StringToInt(id, 0));
			return success;
		}

		[HttpPost]
		public ActionResult BankLasalleUpdate(string BankLasalleCode)
		{
			return View();
		}

		[HttpPost]
		public ActionResult UpdateBankLasalle(string BankLasalleCode, int ProvinceId, int CityId)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.UpdateBankLasalle(BankLasalleCode, ProvinceId, CityId);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改银联号";
				behaviorLog.BlType = 3;
				behaviorLog.parm = $"BankLasalleCode={BankLasalleCode}&ProvinceId={ProvinceId}&CityId={CityId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}
	}
}
