using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace TestAutomationEssentials.Common.Configuration
{
	/// <summary>
	/// Provides a base class for reading values out of an Xml configuration file
	/// </summary>
	/// <remarks>
	/// Good test projects should be able to run on different environments. For example, with desktop application the path
	/// to the application may be different in every developer's machine and the test servers. In web applications or services,
	/// the URL of the application or server may be different between different environments. In order to allow that, there's a need
	/// to read some values from a configuration file.
	/// <para>
	/// Note: it's a good practice to keep these files short and simple, and keep it for its mere purpose of supporting different
	/// environments. Usualy it should contain only 1-3 parameters, like URL or path, and maybe username/password. Everything that
	/// you can deduce or create automatically is better in order to have a simple user and developer experience.
	/// </para><para>
	/// The XML file supported by this class should have one level of elements under the root, each denotes a single property value.
	/// </para><para>
	/// Here's a "recipe" for using this class in a way which is robust and user-friendly:
	/// 1. Define an XSD file that describes the supported properties. You can add &lt;annotation/&gt;s to provide additional 
	/// documentation each property. Note that this XSD file will be used by Visual Studio to provide IntelliSense support when
	/// editing the XML file
	/// 2. Create one or more XML file using the previously created XSD. You should store at least one file in Source Control
	/// that describes the environment which is used to run the tests in the CI or nightly build. If you have several predefined 
	/// environments that you want to run your tests on, you should create one XML file for each of them. Also create one for your own
	/// dev environment.
	/// 3. Create one .testsettings file for each XML file. Add the XML file to the "Deployment Items" section of the .testsettings file.
	/// 4. Add a class to your project and make it derive from <see cref="ConfigurationBase"/>
	/// 5. Add automatic properties (e.g. <code>public string AppURL { get; set; }</code> for each of parameters in the XML, and 
	/// decorate them with the <see cref="ConfigurationParameterAttribute"/> attribute, optionally specifying a default value. In case
	/// that the default value cannot be specified as a constant, instead of the attribute, call <see cref="GetValue(System.Type,string,object)"/>
	/// and specify the value at run-time.
	/// 6. Inside the relevant initialization method (e.g. <see cref="TestBase.ClassInitialize()"/>), call <see cref="TestConfig.Load{TConfiguration}"/>
	/// specifying the name of the class you've created. Optionally you can store a reference to the returned object in a member 
	/// static variable, as the configuration should not change during the entire test run.
	/// </para><para>
	/// <h2>Running from Visual Studio</h2>
	/// When you want to run your tests locally from within Visual Studio, you must use a .testsettings file such as those you created
	/// in step 3. The file you should use must refer to the XML file that has the correct values for your local environment (or of the
	/// environment that you want to use when running from Visual Studio). You can select the .testsettings file from the menu bar:
	/// Test -> Test Settings -> Select Test Settings File.
	/// Tip: If every developer in the team needs a different configuration, each one should create or edit the local configuration 
	/// file for to match his environment. If you're using TFS, in order to prevent someone from checking-in his file, you (as the 
	/// owner of the automation infrastructure) can create a new workspace on your machine with only the folder that contains the XML 
	/// file, and check it out with Lock type = "Check In - Allow other users to check out but prevent them from checking in".
	/// <h2>Running from command line</h2>
	/// If you run the tests using MSTest.exe, you should specify: /testsettings:{your.testsettings}
	/// If you run the tests using VSTest.Console.exe, you should specify: /runsettings:{your.testsettings}
	/// </para>
	/// </remarks>
	public abstract class ConfigurationBase
	{
		private XDocument _document;

		internal void Load(XDocument document)
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

		/// <summary>
		/// Returns a value with the specified element name from the configuration file
		/// </summary>
		/// <param name="elementName">The name of the element</param>
		/// <returns>The value (inner text) of the element as a string, or <b>null</b> if this element is missing</returns>
		protected string GetValue(string elementName)
		{
			return GetValue(elementName, (string)null);
		}

		/// <summary>
		/// Returns a value with the specified element name from the configuration file
		/// </summary>
		/// <typeparam name="T">The type of the value</typeparam>
		/// <param name="elementName">The name of the element</param>
		/// <param name="defaultvalue">The value (inner text) of the element, or <paramref name="defaultvalue"/> if the element is missing</param>
		/// <returns></returns>
		protected T GetValue<T>(string elementName, T defaultvalue)
		{
			return (T)GetValue(typeof (T), elementName, defaultvalue);
		}

		// TODO: support enums!
		private object GetValue(Type type, string elementName, object defaultValue)
		{
			var element = _document.Root.Element(XName.Get(elementName, XmlNamespace));
			if (element == null)
				return defaultValue;

			return Convert.ChangeType(element.Value, type);
		}

		/// <summary>
		/// When overriden in a derived class, returns the XmlNamespace that is used in the XML configuration file.
		/// </summary>
		protected abstract string XmlNamespace { get; }

		/// <summary>
		/// Marks a property as a configuration parameter
		/// </summary>
		/// <remarks>
		/// Properties marked with that attribute on classes that derive from <see cref="ConfigurationBase"/> are automatically
		/// set according to their corresponding values in the configuration file upon the call to <see cref="TestConfig.Load{TConfiguration}"/>.
		/// The property can have a private setter.
		/// <para>
		/// You can specify a default value that will be used if the elemenet does not exist in the configuration file.
		/// </para>
		/// </remarks>
		[AttributeUsage(AttributeTargets.Property)]
		protected class ConfigurationParameterAttribute : Attribute
		{
			/// <summary>
			/// Gets or sets the default value of the property that will be used in case the element does not exist in the configuration file
			/// </summary>
			public object DefaultValue { get; set; }

			/// <summary>
			/// Initialized as a new instance with <b>null</b> as the default value
			/// </summary>
			public ConfigurationParameterAttribute()
			{
			}

			/// <summary>
			/// Initiailized a new instance with the specified default value
			/// </summary>
			/// <param name="defaultValue"><see cref="DefaultValue"/></param>
			public ConfigurationParameterAttribute(object defaultValue)
			{
				DefaultValue = defaultValue;
			}
		}
	}
}