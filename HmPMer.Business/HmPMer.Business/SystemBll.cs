using HM.Framework;
using HM.Framework.Logging;
using HmPMer.Dal;
using HmPMer.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace HmPMer.Business
{
	public class SystemBll
	{
		private readonly SystemDal _dal = new SystemDal();

		public EmailSet GetEmailSetForCode(string EmialCode)
		{
			return _dal.GetEmailSetForCode(EmialCode);
		}

		public bool UpdateEmailSet(EmailSet Model)
		{
			return _dal.UpdateEmailSet(Model);
		}

		public bool SendMail(string ReceiveAddress, string Title, string Body)
		{
			EmailSet emailSetForCode = _dal.GetEmailSetForCode("QQ");
			MailMessage mailMessage = new MailMessage();
			mailMessage.IsBodyHtml = true;
			mailMessage.Priority = MailPriority.High;
			mailMessage.Subject = Title;
			mailMessage.Body = Body;
			MailMessage mailMessage2 = mailMessage;
			string[] array = ReceiveAddress.Split(';');
			foreach (string addresses in array)
			{
				mailMessage2.To.Add(addresses);
			}
			mailMessage.From = new MailAddress(emailSetForCode.Account, emailSetForCode.displayName);
			try
			{
				SmtpClient smtpClient = new SmtpClient();
				smtpClient.Credentials = new NetworkCredential(emailSetForCode.Account, emailSetForCode.Pwd);
				smtpClient.Port = Convert.ToInt32(emailSetForCode.port);
				smtpClient.Host = emailSetForCode.Sendserver;
				smtpClient.EnableSsl = true;
				smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
				smtpClient.Send(mailMessage);
			}
			catch (Exception ex)
			{
				string message = ex.Message;
				return false;
			}
			return true;
		}

		public SmsSet GetSmsSetForCode(string SmsCode)
		{
			return _dal.GetSmsSetForCode(SmsCode);
		}

		public bool UpdateSmsSet(SmsSet Model)
		{
			return _dal.UpdateSmsSet(Model);
		}

		public bool SendSms(string mobile, string content)
		{
			string url = "http://api.smsbao.com/sms";
			SmsSet smsSetForCode = _dal.GetSmsSetForCode("DXB");
			EncryUtils.MD5(smsSetForCode.Pwd);
			HttpUtility.UrlEncode(content, Encoding.GetEncoding("utf-8"));
			string para = string.Format("?u={0}&p={1}&m={2}&c={3}&ext=0{4}", smsSetForCode.Account, EncryUtils.MD5(smsSetForCode.Pwd), mobile, content, "");
			string text = HttpUtils.SendRequest(url, para, "GET", "UTF-8");
			LogUtil.InfoFormat("短信发送返回值：" + text);
			if (text.Equals("0"))
			{
				SmsTrans smsTrans = new SmsTrans();
				smsTrans.Id = Guid.NewGuid().ToString();
				smsTrans.Mobile = mobile;
				smsTrans.Content = content;
				smsTrans.AddTime = DateTime.Now;
				_dal.AddSmsTrans(smsTrans);
				return true;
			}
			return false;
		}

		public bool SendSms(string smscode, string mobile, string code, string username, decimal money)
		{
			SmsModel smsModel = _dal.GetSmsModel(smscode);
			if (smsModel == null)
			{
				return false;
			}
			string content = smsModel.Content;
			content = content.Replace("{username}", username);
			content = content.Replace("{code}", code);
			content = content.Replace("{money}", (money / 100m).ToString("0.00"));
			return SendSms(mobile, content);
		}

		public List<SmsModel> GetSmsModelList()
		{
			return _dal.GetSmsModelList();
		}

		public bool UpdateSmsModel(List<SmsModel> Model)
		{
			return _dal.UpdateSmsModel(Model);
		}

		public SmsModel GetSmsModel(string code)
		{
			return _dal.GetSmsModel(code);
		}

		public long AddSmsTrans(SmsTrans Model)
		{
			return _dal.AddSmsTrans(Model);
		}

		public List<SmsTrans> GetSmsTransPageList(SmsTrans parm, ref Paging paging)
		{
			return _dal.GetSmsTransPageList(parm, ref paging);
		}

		public int BackDataBase(string Path)
		{
			return _dal.BackDataBase(Path);
		}

		public List<PayTypeQuota> GetPayTypeQuotaList()
		{
			return _dal.GetPayTypeQuotaList();
		}

		public long SetPayTypeQuota(List<PayTypeQuota> ListModel)
		{
			return _dal.SetPayTypeQuota(ListModel);
		}

		public PayTypeQuota GetPayTypeQuotaForPayCode(string payCode)
		{
			return _dal.GetPayTypeQuotaForPayCode(payCode);
		}

		public DateTime GetTDate(DateTime StarTime, int Day)
		{
			return _dal.GetTDate(StarTime, Day);
		}

		public DateTime GetTDDate(DateTime StarTime, int Day, int DT)
		{
			DateTime dateTime = StarTime;
			if (DT == 1)
			{
				return _dal.GetTDate(StarTime, Day).AddHours((double)StarTime.Hour).AddMinutes((double)StarTime.Minute)
					.AddSeconds((double)StarTime.Second);
			}
			return dateTime.AddDays((double)Day);
		}

		public List<Notice> GetNoticePageList(Notice parm, ref Paging paging)
		{
			return _dal.GetNoticePageList(parm, ref paging);
		}

		public List<NoticeInfo> GetNoticeInfoPageList(NoticeInfo parm, ref Paging paging)
		{
			List<NoticeInfo> noticeInfoPageList = _dal.GetNoticeInfoPageList(parm, ref paging);
			foreach (NoticeInfo item in noticeInfoPageList)
			{
				if (_dal.GetReadNotice(item.Id, parm.UserId) != null)
				{
					item.IsRead = 1;
				}
			}
			return noticeInfoPageList;
		}

		public ReadNotice GetReadNotice(string NoticeId, string UserId)
		{
			return _dal.GetReadNotice(NoticeId, UserId);
		}

		public Notice GetNoticeModel(string Id)
		{
			return _dal.GetNoticeModel(Id);
		}

		public NoticeInfo GetNoticeInfoModel(string Id, string UserId)
		{
			NoticeInfo noticeInfoModel = _dal.GetNoticeInfoModel(Id);
			if (noticeInfoModel != null && _dal.GetReadNotice(noticeInfoModel.Id, UserId) != null)
			{
				noticeInfoModel.IsRead = 1;
			}
			return noticeInfoModel;
		}

		public long AddNotice(Notice Model)
		{
			return _dal.AddNotice(Model);
		}

		public long AddReadNotice(ReadNotice Model)
		{
			return _dal.AddReadNotice(Model);
		}

		public int UpdateNotice(Notice Model)
		{
			return _dal.UpdateNotice(Model);
		}

		public int ReleaseNotice(Notice Model)
		{
			return _dal.ReleaseNotice(Model);
		}

		public int DelNotice(string Id)
		{
			return _dal.DelNotice(Id);
		}

		public NoticeInfo GetNewNoticeInfo(int NoticeType, string UserId)
		{
			NoticeInfo noticeInfo = _dal.GetNewNoticeInfo(NoticeType);
			if (noticeInfo != null)
			{
				if (_dal.GetReadNotice(noticeInfo.Id, UserId) != null)
				{
					noticeInfo.IsRead = 1;
				}
			}
			else
			{
				noticeInfo = new NoticeInfo();
			}
			return noticeInfo;
		}

		public List<NoticeInfo> GetNewListNoticeInfo(int NoticeType)
		{
			return _dal.GetNewListNoticeInfo(NoticeType);
		}

		public BankLasalle GetBankLasalleCode(string BankLasalleCode)
		{
			return _dal.GetBankLasalleCode(BankLasalleCode);
		}

		public List<BankLasalle> GetListBankLasalle(string BankCode, int Proid, int Cityid)
		{
			return _dal.GetListBankLasalle(BankCode, Proid, Cityid);
		}

		public List<BankLasalleInfo> GetBankLasalleList(BankLasalleInfo parm, ref Paging paging)
		{
			return _dal.GetBankLasalleList(parm, ref paging);
		}

		public int UpdateBankLasalle(string BankLasalleCode, int ProvinceId, int CityId)
		{
			return _dal.UpdateBankLasalle(BankLasalleCode, ProvinceId, CityId);
		}

		public long InserBehaviorLog(BehaviorLog Model)
		{
			return _dal.InserBehaviorLog(Model);
		}

		public List<BehaviorLog> GetBehaviorLogList(BehaviorLog parm, ref Paging paging)
		{
			return _dal.GetBehaviorLogList(parm, ref paging);
		}
	}
}
