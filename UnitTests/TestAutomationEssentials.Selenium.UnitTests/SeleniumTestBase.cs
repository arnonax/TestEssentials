using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
//using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.Selenium.UnitTests
{
    [TestClass]
    [DeploymentItem("chromedriver.exe")]
    [DeploymentItem("geckodriver.exe")]
    public class SeleniumTestBase : TestBase
    {
        protected Browser OpenBrowserWithPage(string pageSource)
        {
            var tempFileName = Path.GetTempFileName();
            var filename = tempFileName + ".html"; // The extension is required in order for Chrome to open it as HTML and not as text
            File.Copy(tempFileName, filename, true); // Do not delete the original file so GetTempFileName won't give it to someone else
            AddCleanupAction(() =>
            {
                // Order may matter if run in parallel
                File.Delete(filename);
                File.Delete(tempFileName);
            });
            File.WriteAllText(filename, pageSource);
            TestContext.AddResultFile(filename);
            var driver = CreateDriver();
            var browser = new Browser("test browser", driver);
            browser.NavigateToUrl(new Uri(filename).AbsoluteUri);
            return browser;
        }

        protected IWebDriver CreateDriver()
        {
            //return new OpenQA.Selenium.Chrome.ChromeDriver();

            // This path is the default, and is always what's available on AppVeyor (https://www.appveyor.com/docs/how-to/selenium-testing/)
            var driverService = FirefoxDriverService.CreateDefaultService();
            driverService.FirefoxBinaryPath = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";

            var options = new FirefoxOptions();
            options.AddArgument("--headless");
            return new FirefoxDriver(driverService, options);
        }
    }
}