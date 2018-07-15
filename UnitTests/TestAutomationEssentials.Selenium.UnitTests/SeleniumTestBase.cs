using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
            var filename = Path.GetTempFileName();
            File.Move(filename, filename);
            File.WriteAllText(filename, pageSource);
            TestContext.AddResultFile(filename);
            var driver = CreateDriver();
            var browser = new Browser("test browser", driver);
            browser.NavigateToUrl(new Uri(filename).AbsoluteUri);
            return browser;
        }

        protected IWebDriver CreateDriver()
        {
            //return new ChromeDriver();

            // This path is the default, and is always what's available on AppVeyor (https://www.appveyor.com/docs/how-to/selenium-testing/)
            var driverService = FirefoxDriverService.CreateDefaultService();
            driverService.FirefoxBinaryPath = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";

            return new FirefoxDriver(driverService);
        }
    }
}