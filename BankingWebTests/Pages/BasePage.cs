using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using BankingWebTests.Support;

namespace BankingWebTests.Pages
{
    /// <summary>
    /// Базовий клас усіх сторінок (Page Object Pattern).
    /// локаторів тут немає - шукати далі.
    /// </summary>
    public abstract class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;

        protected BasePage(IWebDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Wait = new WebDriverWait(driver, TestSettings.ExplicitWait);
        }

        /// <summary>
        /// Дочекатись, поки елемент з'явиться у DOM і стане видимим.
        /// </summary>
        protected IWebElement WaitForVisible(By locator)
        {
            return Wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(locator);
                    return element.Displayed ? element : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });
        }

        protected void Click(By locator)
        {
            WaitForVisible(locator).Click();
        }

        protected void Type(By locator, string text)
        {
            var element = WaitForVisible(locator);
            element.Clear();
            element.SendKeys(text);
        }

        protected string ReadText(By locator)
        {
            return WaitForVisible(locator).Text.Trim();
        }

        protected bool IsPresent(By locator)
        {
            try
            {
                return Driver.FindElement(locator).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        /// <summary>
        /// Виконати JavaScript на сторінці.
        ///
        /// Потрібен рівно в одному місці — щоб очистити localStorage перед
        /// сценарієм. P.S я не знав що ще добавити 
        /// </summary>
        protected object ExecuteScript(string script, params object[] args)
        {
            return ((IJavaScriptExecutor)Driver).ExecuteScript(script, args);
        }
    }
}
