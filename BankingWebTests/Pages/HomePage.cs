using OpenQA.Selenium;

namespace BankingWebTests.Pages
{
    /// <summary>
    /// а тут вже локатори - до них дойшли.
    /// </summary>
    public class HomePage : BasePage
    {
        private static readonly By CustomerLoginButton = By.CssSelector("button[ng-click='customer()']");
        private static readonly By ManagerLoginButton  = By.CssSelector("button[ng-click='manager()']");
        private static readonly By HomeButton          = By.CssSelector("button[ng-click='home()']");
        private static readonly By MainHeading         = By.CssSelector("strong.mainHeading");

        public HomePage(IWebDriver driver) : base(driver) { }

        /// <summary>Відкрити застосунок за заданою адресою.</summary>
        public HomePage Open(string url)
        {
            Driver.Navigate().GoToUrl(url);
            WaitForVisible(MainHeading);
            return this;
        }

        //TODO Скинути стан застосунку до початкового

        public HomePage ResetApplicationState()
        {
            ExecuteScript("window.localStorage.clear();");
            Driver.Navigate().Refresh();
            WaitForVisible(MainHeading);
            return this;
        }

        public CustomerLoginPage GoToCustomerLogin()
        {
            Click(CustomerLoginButton);
            return new CustomerLoginPage(Driver);
        }

   
        public bool IsOpen()
        {
            try
            {
                WaitForVisible(CustomerLoginButton);
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public HomePage GoHome()
        {
            Click(HomeButton);
            return this;
        }
    }
}
