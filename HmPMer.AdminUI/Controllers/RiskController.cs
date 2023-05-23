using HM.Framework;
using HmPMer.AdminUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.WebAdminUI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.AdminUI.Controllers
{
	public class RiskController : Controller
	{
		private readonly RiskBll bll = new RiskBll();

		public ActionResult RiskSchemeList()
		{
			return View();
		}

		public ActionResult RiskSchemeAddView()
		{
			return View();
		}

		public ActionResult RiskSchemeUpdateView(string id)
		{
			RiskScheme riskSchemeModel = bll.GetRiskSchemeModel(id);
			return View(riskSchemeModel);
		}

		public ActionResult RiskSettingView(string TargetId, int RiskType)
		{
			List<RiskScheme> riskSchemeList = bll.GetRiskSchemeList(RiskType, null);
			RiskSetting riskSetting = bll.GetRiskSetting(RiskType, TargetId);
			base.ViewBag.TargetId = TargetId;
			base.ViewBag.RiskType = RiskType;
			if (riskSetting != null)
			{
				base.ViewBag.RiskSchemeId = riskSetting.RiskSchemeId;
			}
			return View(riskSchemeList);
		}

		public ActionResult GetRiskSchemeList(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			RiskScheme param = new RiskScheme
			{
				RiskSchemeTaype = Utils.GetRequestToInt("RiskSchemeTaype", -1),
				UserId = Utils.GetRequestQuery("UserId")
			};
			ResultPage<RiskScheme> resultPage = new ResultPage<RiskScheme>();
			resultPage.msg = "查询成功";
			resultPage.Item = bll.GetRiskSchemeList(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		[HttpPost]
		public ActionResult RiskSchemeAdd(RiskScheme model)
		{
			model.RiskSchemeId = Guid.NewGuid().ToString();
			model.SingleMinAmt *= 100m;
			model.SingleMaxAmt *= 100m;
			if (model.IsDayAmt == 1)
			{
				model.DayAmt *= 100m;
			}
			else
			{
				model.DayAmt = decimal.Zero;
			}
			if (model.IsMonthAmt == 1)
			{
				model.MonthAmt *= 100m;
			}
			else
			{
				model.MonthAmt = decimal.Zero;
			}
			long num = bll.AddRiskScheme(model);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "新增失败！";
			}
			else
			{
				resultBase.Success = true;
				resultBase.Message = "新增成功！";
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "新增风控方案";
				behaviorLog.BlType = 1;
				behaviorLog.parm = model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult RiskSchemeUpdate(RiskScheme model)
		{
			model.SingleMinAmt *= 100m;
			model.SingleMaxAmt *= 100m;
			if (model.IsDayAmt == 1)
			{
				model.DayAmt *= 100m;
			}
			else
			{
				model.DayAmt = decimal.Zero;
			}
			if (model.IsMonthAmt == 1)
			{
				model.MonthAmt *= 100m;
			}
			else
			{
				model.MonthAmt = decimal.Zero;
			}
			bool flag = bll.UpdateRiskScheme(model);
			ResultBase resultBase = new ResultBase();
			if (flag)
			{
				resultBase.Success = true;
				resultBase.Message = "修改成功！";
			}
			else
			{
				resultBase.Message = "修改失败！";
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "修改风控方案";
				behaviorLog.BlType = 2;
				behaviorLog.parm = model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		[HttpPost]
		public ActionResult SetRiskSetting(List<RiskSetting> list, string riskSchemeId, int riskType, int aloneRisk)
		{
			ApiResult<string> failing = ApiResult<string>.Failing;
			if (list == null || list.Count == 0)
			{
				failing.message = "没有设置风控的项";
				return failing;
			}
			bool flag = bll.SetRiskSetting(list, riskSchemeId, riskType, aloneRisk);
			if (flag)
			{
				failing.IsSuccess = flag;
				failing.message = "设置成功";
			}
			else
			{
				failing.IsSuccess = flag;
				failing.message = "设置失败";
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = ((riskType == 1) ? "设置商户风控方案" : "设置代理风控方案");
				behaviorLog.BlType = 2;
				behaviorLog.parm = $"list={list.ToJson()}&riskSchemeId={riskSchemeId}&riskType={riskType}&aloneRisk={aloneRisk}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return failing;
		}

		[HttpPost]
		public ActionResult DelRiskScheme(string RiskSchemeId)
		{
			ResultBase resultBase = new ResultBase();
			int num = bll.DelRiskScheme(RiskSchemeId);
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "操作失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "删除风控方案";
				behaviorLog.BlType = 3;
				behaviorLog.parm = $"list=&RiskSchemeId={RiskSchemeId}";
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}
	}
}
