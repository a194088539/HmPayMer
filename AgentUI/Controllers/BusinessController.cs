using HmPMer.AgentUI.Models;
using HmPMer.Business;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HmPMer.AgentUI.Controllers
{
	public class BusinessController : Controller
	{
		private AccountBll bll = new AccountBll();

		private UserBaseBll ubll = new UserBaseBll();

		private UserBase user = ModelCommon.GetUserModel();

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult BusinessList(int? PageIndex, int? PageSize, UserBaseInfo param)
		{
			Paging paging = new Paging();
			paging.PageIndex = ((!PageIndex.HasValue) ? 1 : PageIndex.Value);
			paging.PageSize = (PageSize.HasValue ? PageSize.Value : 15);
			param.AgentId = user.UserId;
			param.IsEnabled = -1;
			param.UserType = -1;
			param.AloneRate = -1;
			List<UserBaseInfo> model = ubll.LoadUserPage(param, ref paging);
			base.ViewData["PageSize"] = paging.PageSize;
			base.ViewData["TotalCount"] = paging.TotalCount;
			base.ViewData["PageCount"] = paging.PageCount;
			base.ViewData["page"] = new PageInfo().createAjaxPageControl("page", paging.PageSize, paging.PageIndex, paging.TotalCount);
			return View(model);
		}

		public ActionResult Add()
		{
			return View();
		}

		public ActionResult SetPayType()
		{
			List<PayType> interfaceType = new PayBll().GetInterfaceType(user.UserId, 4);
			return View(interfaceType);
		}

		public ActionResult SetTypeRate(string UserId)
		{
			List<PayRateInfo> payRateList = new RateBll().GetPayRateList(UserId, 2);
			return View(payRateList);
		}

		public ActionResult AddUserBase(UserBase model)
		{
			ApiResult<bool> failing = ApiResult<bool>.Failing;
			if (model == null)
			{
				failing.message = "参数错误！";
				return failing;
			}
			if (string.IsNullOrEmpty(model.MobilePhone))
			{
				failing.message = "请输入电话号码";
				return failing;
			}
			if (string.IsNullOrEmpty(model.Pass))
			{
				failing.message = "请输入密码";
				return failing;
			}
			if (string.IsNullOrEmpty(model.MerName))
			{
				failing.message = "请输入商户名称";
				return failing;
			}
			UserBaseBll userBaseBll = new UserBaseBll();
			if (userBaseBll.GetModelForMobile(model.MobilePhone, "") != null)
			{
				failing.message = "此手机号码已经存在，请更换一个";
				return failing;
			}
			model.AgentId = user.UserId;
			if (userBaseBll.AddBusiness(model) == 0L)
			{
				failing.message = "添加商户失败！";
				return failing;
			}
			failing.data = true;
			failing.IsSuccess = true;
			return failing;
		}

		public ActionResult SetInterfaceType(string InterfaceCode, string PayCode)
		{
			List<InterfaceType> list = new List<InterfaceType>();
			if (!string.IsNullOrEmpty(PayCode))
			{
				string[] array = PayCode.Split(',');
				foreach (string payCode in array)
				{
					InterfaceType interfaceType = new InterfaceType();
					interfaceType.InterfaceCode = InterfaceCode;
					interfaceType.PayCode = payCode;
					interfaceType.Type = 2;
					list.Add(interfaceType);
				}
			}
			long num = new PayBll().SetInterfaceType(list, InterfaceCode);
			ApiResult<bool> failing = ApiResult<bool>.Failing;
			if (num == 0L)
			{
				failing.message = "设置失败！";
				return failing;
			}
			failing.data = true;
			failing.IsSuccess = true;
			return failing;
		}

		public ActionResult SetPayRate(List<PayRate> ListModel, string UserId)
		{
			new List<InterfaceType>();
			foreach (PayRate item in ListModel)
			{
				item.Id = Guid.NewGuid().ToString();
				item.UserId = UserId;
				item.RateType = 2;
				item.Rate /= 100m;
				item.IsEnabled = 1;
			}
			ApiResult<bool> failing = ApiResult<bool>.Failing;
			if (new RateBll().SetPayRate(ListModel, UserId, 2) == 0L)
			{
				failing.message = "设置失败！";
				return failing;
			}
			new UserBaseBll().UpdateAloneRate(1, UserId);
			failing.data = true;
			failing.IsSuccess = true;
			return failing;
		}
	}
}
