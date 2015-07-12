using System.Xml.Linq;

namespace TestAutomationEssentials.MSTest.Configuration
{
	public class TestConfig
	{
		public static TConfiguration Load<TConfiguration>(string filename)
			where TConfiguration : ConfigurationBase, new()
		{
			var configuration = new TConfiguration();
			var doc = XDocument.Load(filename);
			configuration.Load(doc);

			return configuration;
		}
	}
}