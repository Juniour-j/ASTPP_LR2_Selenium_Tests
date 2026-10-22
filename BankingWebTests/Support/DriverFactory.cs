using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BankingWebTests.Support
{
    public static class DriverFactory
    {
        public static IWebDriver Create()
        {
            var options = new ChromeOptions();

            if (TestSettings.Headless)
                options.AddArgument("--headless=new");

            options.AddArgument("--window-size=1400,1000");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");

            
            options.SetLoggingPreference(LogType.Browser, LogLevel.All);

            var driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TestSettings.ImplicitWait;

            return driver;
        }
    }
}
