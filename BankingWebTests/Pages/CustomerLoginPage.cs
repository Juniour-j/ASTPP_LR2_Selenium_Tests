using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace BankingWebTests.Pages
{
    /// <summary>
    /// Сторінка входу клієнта: випадний список імен і кнопка Login.
    /// треба знати:
    /// 1) value кожного пункту — це числовий ідентифікатор клієнта (1..5),
    ///    а не його ім'я, тому обираємо за видимим текстом (SelectByText).
    /// 2) кнопка Login має ng-show="custId != ''", тобто до вибору клієнта
    ///    вона прихована.
    /// </summary>
    public class CustomerLoginPage : BasePage
    {
        private static readonly By CustomerSelect = By.Id("userSelect");
        private static readonly By LoginButton    = By.CssSelector("form[name='myForm'] button[type='submit']");

        
        private const string BlankOptionText = "---Your Name---";

        public CustomerLoginPage(IWebDriver driver) : base(driver) { }

       
        public bool IsOpen() => IsPresent(CustomerSelect);


        public IList<string> GetAvailableCustomers()
        {
            var select = new SelectElement(WaitForVisible(CustomerSelect));

            return select.Options
                         .Select(option => option.Text.Trim())
                         .Where(text => text != BlankOptionText && text.Length > 0)
                         .ToList();
        }

        public AccountPage LoginAs(string customerName)
        {
            var select = new SelectElement(WaitForVisible(CustomerSelect));
            select.SelectByText(customerName);

            Click(LoginButton);

            return new AccountPage(Driver);
        }
    }
}
