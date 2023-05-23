using HM.Framework;
using HmPMer.Entity;

namespace HmPMer.MerUI.Models
{
	public class ModelCommon
	{
		public static UserBase GetUserModel()
		{
			string session = HmSession.GetSession("user");
			if (string.IsNullOrEmpty(session))
			{
				return null;
			}
			return session.FormJson<UserBase>();
		}

		public static bool WriteUserModel(UserBase model, int min = 3600)
		{
			string val = model.ToJson();
			HmSession.SetSession("user", val, false, model.UserId, min);
			return true;
		}

		public static bool RemoveUserModel()
		{
			HmSession.RemoveSession("user");
			return true;
		}
	}
}
