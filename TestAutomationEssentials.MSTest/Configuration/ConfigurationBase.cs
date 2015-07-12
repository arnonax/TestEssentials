using System;
using System.Xml.Linq;

namespace TestAutomationEssentials.MSTest.Configuration
{
	public abstract class ConfigurationBase
	{
		private XDocument _document;

		public void Load(XDocument document)
		{
			_document = document;
		}

		protected string GetValue(string elementName)
		{
			return GetValue(elementName, (string)null);
		}

		protected T GetValue<T>(string elementName, T defaultvalue)
		{
			var element = _document.Root.Element(XName.Get(elementName, XmlNamespace));
			if (element == null)
				return defaultvalue;

			return (T)Convert.ChangeType(element.Value, typeof(T));
		}

		protected abstract string XmlNamespace { get; }
	}
}