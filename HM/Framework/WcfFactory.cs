using System;
using System.ServiceModel;

namespace HM.Framework
{
	public class WcfFactory<T> : IDisposable where T : ICommunicationObject
	{
		public string ServerName
		{
			get;
			set;
		}

		public T Client
		{
			get;
			set;
		}

		public WcfFactory(string name)
		{
			ServerName = name;
		}

		public void CreateChannel(Action<T> work)
		{
			ChannelFactory<T> channelFactory = new ChannelFactory<T>(ServerName);
			Client = channelFactory.CreateChannel();
			try
			{
				work(Client);
			}
			catch (CommunicationException)
			{
				Client.Abort();
			}
			catch (TimeoutException)
			{
				Client.Abort();
			}
			catch (Exception)
			{
				Client.Abort();
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}
