using HmPMer.Dal;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Business
{
	public class FeedbackBll
	{
		private readonly FeedbackDal _dal = new FeedbackDal();

		public List<Feedback> GetFeedbackList(Feedback param, ref Paging paging)
		{
			return _dal.GetFeedbackList(param, ref paging);
		}

		public long AddFeedback(Feedback Model)
		{
			return _dal.AddFeedback(Model);
		}

		public long AddFeedbackReply(FeedbackReply Model)
		{
			return _dal.AddFeedbackReply(Model);
		}

		public Feedback GetFeedback(string Id)
		{
			return _dal.GetFeedback(Id);
		}

		public List<FeedbackReply> GetFeedbackReplyList(string FeedbackId)
		{
			return _dal.GetFeedbackReplyList(FeedbackId);
		}
	}
}
