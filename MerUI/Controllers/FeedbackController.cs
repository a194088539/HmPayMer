using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.MerUI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.MerUI.Controllers
{
	public class FeedbackController : Controller
	{
		private UserBase user = ModelCommon.GetUserModel();

		private FeedbackBll fbll = new FeedbackBll();

		public ActionResult Index()
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

		public ActionResult AddFeedBack()
		{
			return View();
		}

		public ActionResult FeedbackList(int? pageIndex, int? pageSize, Feedback param)
		{
			Paging paging = new Paging
			{
				PageIndex = ((!pageIndex.HasValue) ? 1 : pageIndex.Value),
				PageSize = (pageSize.HasValue ? pageSize.Value : 15)
			};
			param.UserId = user.UserId;
			List<Feedback> feedbackList = fbll.GetFeedbackList(param, ref paging);
			base.ViewData["PageSize"] = paging.PageSize;
			base.ViewData["TotalCount"] = paging.TotalCount;
			base.ViewData["PageCount"] = paging.PageCount;
			base.ViewData["page"] = new PageInfo().createAjaxPageControl("page", paging.PageSize, paging.PageIndex, paging.TotalCount);
			return View(feedbackList);
		}

		public ActionResult AddFeedBackInfo(Feedback model)
		{
			ApiResult<bool> failing = ApiResult<bool>.Failing;
			model.UserId = user.UserId;
			model.Id = Guid.NewGuid().ToString();
			model.AddTime = DateTime.Now;
			if (fbll.AddFeedback(model) == 0L)
			{
				failing.message = "提交失败！";
				return failing;
			}
			failing.data = true;
			failing.IsSuccess = true;
			return failing;
		}

		public ActionResult Notice()
		{
			return View();
		}

		public ActionResult Noticeinfo(string Id)
		{
			SystemBll systemBll = new SystemBll();
			NoticeInfo noticeInfoModel = systemBll.GetNoticeInfoModel(Id, user.UserId);
			if (noticeInfoModel.IsRead == 0)
			{
				ReadNotice readNotice = new ReadNotice();
				readNotice.UserId = user.UserId;
				readNotice.NoticeId = noticeInfoModel.Id;
				systemBll.AddReadNotice(readNotice);
			}
			return View(noticeInfoModel);
		}

		public ActionResult NoticeList(int? pageIndex, int? pageSize)
		{
			Paging paging = new Paging
			{
				PageIndex = ((!pageIndex.HasValue) ? 1 : pageIndex.Value),
				PageSize = (pageSize.HasValue ? pageSize.Value : 15)
			};
            NoticeInfo param = new NoticeInfo();
            param.NoticeType = "1";
			param.UserId = user.UserId;
			List<NoticeInfo> noticeInfoPageList = new SystemBll().GetNoticeInfoPageList(param, ref paging);
			base.ViewData["PageSize"] = paging.PageSize;
			base.ViewData["TotalCount"] = paging.TotalCount;
			base.ViewData["PageCount"] = paging.PageCount;
			base.ViewData["page"] = new PageInfo().createAjaxPageControl("page", paging.PageSize, paging.PageIndex, paging.TotalCount);
			return View(noticeInfoPageList);
		}
	}
}
