using System;
using System.Net;
using System.Net.Mail;

namespace HM.Framework
{
	public class SendEmail
	{
		public bool SendMail(string ReceiveAddress, string Title, string Body)
		{
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
			mailMessage.From = new MailAddress(Utils.GetAppSetting("SystemFrom"), Utils.GetAppSetting("SystemDisplayName"));
			try
			{
				SmtpClient smtpClient = new SmtpClient();
				smtpClient.Credentials = new NetworkCredential(Utils.GetAppSetting("SystemFrom"), Utils.GetAppSetting("SystemPassword"));
				smtpClient.Port = 25;
				smtpClient.Host = Utils.GetAppSetting("SystemServer");
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
	}
}
