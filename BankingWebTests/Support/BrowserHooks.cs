using System;
using System.IO;
using OpenQA.Selenium;
using Reqnroll;
using Reqnroll.BoDi;

namespace BankingWebTests.Support
{
    /// <summary>
    /// Хуки Reqnroll — код, який виконується довкола кожного сценарію.
    ///
    /// Це аналог [TestInitialize] / [TestCleanup] з MSTest
    /// </summary>
    [Binding]
    public class BrowserHooks
    {
        private readonly IObjectContainer _container;
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver _driver;

        public BrowserHooks(IObjectContainer container, ScenarioContext scenarioContext)
        {
            _container = container;
            _scenarioContext = scenarioContext;
        }


        [BeforeScenario]
        public void OpenBrowser()
        {
            _driver = DriverFactory.Create();
            _container.RegisterInstanceAs(_driver);
        }

        
        [AfterScenario]
        public void CloseBrowser()
        {
            try
            {
                if (_scenarioContext.TestError != null && _driver != null)
                    SaveScreenshot();
            }
            finally
            {
                _driver?.Quit();
                _driver?.Dispose();
                _driver = null;
            }
        }

        private void SaveScreenshot()
        {
            try
            {
                var folder = Path.Combine(AppContext.BaseDirectory, TestSettings.ScreenshotFolder);
                Directory.CreateDirectory(folder);

                var safeTitle = string.Join("_", _scenarioContext.ScenarioInfo.Title
                                                   .Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{safeTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var fullPath = Path.Combine(folder, fileName);

                ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile(fullPath);
                Console.WriteLine($"Знімок екрана збережено: {fullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося зберегти знімок екрана: {ex.Message}");
            }
        }
    }
}
