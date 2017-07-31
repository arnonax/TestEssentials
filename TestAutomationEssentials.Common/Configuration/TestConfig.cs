using System.Xml.Linq;

namespace TestAutomationEssentials.Common.Configuration
{
	/// <summary>
	/// Provides utilities for using Test Configuration files
	/// </summary>
	public static class TestConfig
	{
		/// <summary>
		/// Loads and reads the configuration from the specified file
		/// </summary>
		/// <param name="filename">The path of the file to load</param>
		/// <typeparam name="TConfiguration">The class that exposes the values from the configuration file</typeparam>
		/// <returns>A new object of type <typeparamref name="TConfiguration"/> that represents the values that were read from the configuration file</returns>
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