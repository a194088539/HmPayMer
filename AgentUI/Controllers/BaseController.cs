using HM.Framework;
using HmPMer.AgentUI.Fillters;
using HmPMer.AgentUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.AgentUI.Controllers
{
	public class BaseController : Controller
	{
		[Auth(NoLogin = true)]
		public ActionResult LoadCityList(string id)
		{
			ApiResult<List<District>> success = ApiResult<List<District>>.Success;
			if (string.IsNullOrEmpty(id))
			{
				success.data = new List<District>();
				return success;
			}
			List<District> list2 = success.data = new DistrictBll().LoadParentId(Utils.StringToInt(id, 0));
			return success;
		}

		[Auth(NoLogin = true)]
		public ActionResult LoadBank(string BankCode, int Proid, int Cityid)
		{
			ApiResult<List<BankLasalle>> success = ApiResult<List<BankLasalle>>.Success;
			List<BankLasalle> list = success.data = new SystemBll().GetListBankLasalle(BankCode, Proid, Cityid);
			return success;
		}
	}
}
