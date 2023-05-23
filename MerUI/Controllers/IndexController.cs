using HmPMer.Business;
using HmPMer.Entity;
using HmPMer.MerUI.Models;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.MerUI.Controllers
{
	public class IndexController : Controller
	{
		private UserBase user = ModelCommon.GetUserModel();

		public ActionResult Index()
		{
			UserBaseInfo modelForId = new UserBaseBll().GetModelForId(user.UserId);
			return View(modelForId);
		}

		[HttpGet]
		public ActionResult GetPayCountList()
		{
			List<OrderCountInfo> countOrderList = new OrderBll().GetCountOrderList("", "", user.UserId);
			Hashtable hashtable = new Hashtable();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			foreach (OrderCountInfo item in countOrderList)
			{
				list.Add(item.CountDate);
				list2.Add((item.CountOrderAmt / 100m).ToString("0.00"));
				list3.Add((item.CountPayOrderAmt / 100m).ToString("0.00"));
			}
			hashtable.Add("CountDate", list);
			hashtable.Add("CountOrderAmt", list2);
			hashtable.Add("CountPayOrderAmt", list3);
			return Json(hashtable, JsonRequestBehavior.AllowGet);
		}
	}
}
