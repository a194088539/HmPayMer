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
	public class CountDataController : Controller
	{
		private FeedbackBll fbll = new FeedbackBll();

		public ActionResult FeedbackIndex()
		{
			return View();
		}

		public ActionResult FeedbackReply(string Id)
		{
			FeedbackInfo feedbackInfo = new FeedbackInfo();
			feedbackInfo.feedback = fbll.GetFeedback(Id);
			feedbackInfo.feedbackreply = fbll.GetFeedbackReplyList(Id);
			return View(feedbackInfo);
		}

		[HttpPost]
		public ActionResult AddFeedbackReply(FeedbackReply Model)
		{
			Model.Id = Guid.NewGuid().ToString();
			Model.AddTime = DateTime.Now;
			Model.createUser = ModelCommon.GetUserModel().AdmUser;
			Model.FRType = 2;
			long num = fbll.AddFeedbackReply(Model);
			ResultBase resultBase = new ResultBase();
			if (num <= 0)
			{
				resultBase.Success = false;
				resultBase.Message = "回复失败！";
			}
			else
			{
				BehaviorLog behaviorLog = new BehaviorLog();
				behaviorLog.Id = Guid.NewGuid().ToString();
				behaviorLog.BlName = "回复商户建议反馈";
				behaviorLog.BlType = 1;
				behaviorLog.parm = Model.ToJson();
				behaviorLog.createUser = ModelCommon.GetUserModel().AdmUser;
				behaviorLog.addTime = DateTime.Now;
				new SystemBll().InserBehaviorLog(behaviorLog);
			}
			return Json(resultBase);
		}

		public ActionResult LoadFeedbackPage(int? page, int? limit)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!page.HasValue) ? 1 : page.Value);
			paging.PageSize = (limit.HasValue ? limit.Value : 30);
			Feedback param = new Feedback
			{
				UserId = Utils.GetRequest("UserId")
			};
			ResultPage<Feedback> resultPage = new ResultPage<Feedback>();
			resultPage.msg = "查询成功";
			resultPage.Item = fbll.GetFeedbackList(param, ref paging);
			resultPage.pageIndex = paging.PageIndex;
			resultPage.pageSize = paging.PageSize;
			resultPage.totalCount = paging.TotalCount;
			resultPage.pageCount = paging.PageCount;
			return resultPage;
		}

		public ActionResult ProfitAnalysis()
		{
			return View();
		}

		public ActionResult InterfaceAnalysis()
		{
			return View();
		}

		public ActionResult ProfitAnalysisData(string date)
		{
			List<ProfitAnalysis> profitAnalysisList = new CountDataBll().GetProfitAnalysisList(date);
			return View(profitAnalysisList);
		}

		public ActionResult InterfaceAnalysisData(string date)
		{
			List<InterfaceAnalysis> interfaceAnalysisList = new CountDataBll().GetInterfaceAnalysisList(date);
			return View(interfaceAnalysisList);
		}
	}
}
