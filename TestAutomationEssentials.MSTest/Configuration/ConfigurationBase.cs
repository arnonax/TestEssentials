using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TestAutomationEssentials.Common;

namespace TestAutomationEssentials.MSTest.Configuration
{
	public abstract class ConfigurationBase
	{
		private XDocument _document;

		public void Load(XDocument document)
		{
			_document = document;
			LoadConfigurationParametersUsingAttribute();
		}

		private void LoadConfigurationParametersUsingAttribute()
		{
			var type = GetType();
			var properties = from property in type.GetProperties()
				where property.HasAttribute<ConfigurationParameterAttribute>()
				select property;

			foreach (var property in properties)
			{
				var propertyType = property.PropertyType;
				
				var defaultValue = property.GetCustomAttribute<ConfigurationParameterAttribute>().DefaultValue ??
								   propertyType.GetDefaultValue();

				var value = GetValue(propertyType, property.Name, defaultValue);
				property.SetValue(this, value);
			}
		}

		protected string GetValue(string elementName)
		{
			return GetValue(elementName, (string)null);
		}

		protected T GetValue<T>(string elementName, T defaultvalue)
		{
			return (T)GetValue(typeof (T), elementName, defaultvalue);
		}

		private object GetValue(Type type, string elementName, object defaultValue)
		{
			var element = _document.Root.Element(XName.Get(elementName, XmlNamespace));
			if (element == null)
				return defaultValue;

			return Convert.ChangeType(element.Value, type);
		}

		protected abstract string XmlNamespace { get; }

		[AttributeUsage(AttributeTargets.Property)]
		protected class ConfigurationParameterAttribute : Attribute
		{
			public object DefaultValue { get; set; }

			public ConfigurationParameterAttribute()
			{
			}

			public ConfigurationParameterAttribute(object defaultValue)
			{
				DefaultValue = defaultValue;
			}
		}
	}
}