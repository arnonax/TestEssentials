using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.MSTest.Configuration;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	public class ConfigurationTests
	{
		private class DummyConfigurationWithAttributes : ConfigurationBase
		{
			protected override string XmlNamespace
			{
				get { return "dummyNamespace"; }
			}

			[ConfigurationParameter]
			public string StringValue { get; private set; }

			[ConfigurationParameter]
			public int IntValue { get; private set; }

			[ConfigurationParameter(5)]
			public int IntValueWithDefault { get; private set; }
		}

		[TestMethod]
		public void TestConfigurationProvideTheConfigurationValuesUsingAttributes()
		{
			const string configFileName = "DummyConfig.xml";
			File.WriteAllText(configFileName,
				@"
<DummyConfig xmlns=""dummyNamespace"">
	<StringValue>Hello</StringValue>
	<IntValue>3</IntValue>
</DummyConfig>
"
				);
			var config = TestConfig.Load<DummyConfigurationWithAttributes>(configFileName);

			Assert.AreEqual("Hello", config.StringValue, "StringValue");
			Assert.AreEqual(3, config.IntValue, "IntValue");
			Assert.AreEqual(5, config.IntValueWithDefault, "IntWithDefaultValue");
		}

	}
}
