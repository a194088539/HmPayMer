using HM.DAL;
using HmPMer.Entity;
using System.Collections.Generic;

namespace HmPMer.Dal
{
	public class FeedbackDal
	{
		public List<Feedback> GetFeedbackList(Feedback param, ref Paging paging)
		{
			string str = " select count(*) from Feedback Where 1=1 ";
			string text = " ";
            param.UserId = DalContext.EscapeString(param.UserId);
            if (!string.IsNullOrEmpty(param.UserId))
			{
				text = text + " And UserId='" + param.UserId + "' ";
			}
			string sql = " select * from Feedback Where 1=1" + text;
			str += text;
			return DalContext.GetPage<Feedback>(sql, str, " * ", " AddTime desc  ", ref paging, param);
		}

		public long AddFeedback(Feedback Model)
		{
			return DalContext.Insert(Model);
		}

		public long AddFeedbackReply(FeedbackReply Model)
		{
			return DalContext.Insert(Model);
		}

		public Feedback GetFeedback(string Id)
		{
			return DalContext.GetModel<Feedback>(" select * from Feedback where Id=@Id ", new
			{
				Id
			});
		}

		public List<FeedbackReply> GetFeedbackReplyList(string FeedbackId)
		{
			return DalContext.GetList<FeedbackReply>(" select * from FeedbackReply where FeedbackId=@FeedbackId order by AddTime desc ", new
			{
				FeedbackId
			});
		}
	}
}
