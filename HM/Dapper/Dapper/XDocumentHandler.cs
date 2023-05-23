using System.Xml.Linq;

namespace HM.Framework.Dapper
{
	internal sealed class XDocumentHandler : XmlTypeHandler<XDocument>
	{
		protected override XDocument Parse(string xml)
		{
			return XDocument.Parse(xml);
		}

		protected override string Format(XDocument xml)
		{
			return xml.ToString();
		}
	}
}
