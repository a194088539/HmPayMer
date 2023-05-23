using System;
using System.Runtime.Serialization;

namespace HmPMer.Entity
{
	[Serializable]
	[DataContract]
	public class MenuJson
	{
		public string title
		{
			get;
			set;
		}

		public string icon
		{
			get;
			set;
		}

		public string href
		{
			get;
			set;
		}
	}
}
