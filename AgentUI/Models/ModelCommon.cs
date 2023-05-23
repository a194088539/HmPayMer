using HM.Framework;
using HmPMer.Entity;

namespace HmPMer.AgentUI.Models
{
	public class ModelCommon
	{
		public static UserBase GetUserModel()
		{
			string session = HmSession.GetSession("agent");
			if (string.IsNullOrEmpty(session))
			{
				return null;
			}
			return session.FormJson<UserBase>();
		}

		public static bool WriteUserModel(UserBase model, int min = 3600)
		{
			string val = model.ToJson();
			HmSession.SetSession("agent", val, false, model.AgentId, min);
			return true;
		}

		public static bool RemoveUserModel()
		{
			HmSession.RemoveSession("agent");
			return true;
		}

		public static UserAmt GetAgentAmt(string userId)
		{
			return null;
		}
	}
}
