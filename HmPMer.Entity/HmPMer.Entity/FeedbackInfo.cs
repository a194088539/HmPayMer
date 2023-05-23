using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class FeedbackInfo
	{
		public Feedback feedback
		{
			get;
			set;
		}

		public List<FeedbackReply> feedbackreply
		{
			get;
			set;
		}
	}
}
