using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace BankingWebTests.Pages
{

    public class AccountPage : BasePage
    {
        // ─────────────── Локатори сторінки рахунку ───────────────
        private static readonly By AccountSelect   = By.Id("accountSelect");
        private static readonly By AccountInfoBox  = By.XPath("//div[contains(@class,'center')][strong]");
        private static readonly By WelcomeText     = By.CssSelector("span.fontBig");
        private static readonly By NoAccountNotice = By.CssSelector("span[ng-show='noAccount']");

        private static readonly By TransactionsTab = By.CssSelector("button[ng-click='transactions()']");
        private static readonly By DepositTab      = By.CssSelector("button[ng-click='deposit()']");
        private static readonly By WithdrawTab     = By.CssSelector("button[ng-click='withdrawl()']");
        private static readonly By LogoutButton    = By.CssSelector("button[ng-click='byebye()']");

        // ─────────────── Локатори форми операції ───────────────
        private static readonly By AmountInput     = By.CssSelector("input[ng-model='amount']");
        private static readonly By WithdrawSubmit  = By.CssSelector("form[ng-submit='withdrawl()'] button[type='submit']");
        private static readonly By DepositSubmit   = By.CssSelector("form[ng-submit='deposit()'] button[type='submit']");
        private static readonly By MessageSpan     = By.CssSelector("span[ng-show='message']");

        /// <summary>Скільки чекати на повідомлення, перш ніж вважати, що його немає.</summary>
        private static readonly TimeSpan MessageTimeout = TimeSpan.FromSeconds(3);

        public AccountPage(IWebDriver driver) : base(driver) { }

        // ═══════════════════ стан сторінки ═══════════════════════

        /// <summary>
        /// Чи відкрилася сторінка рахунку.
        ///
        /// з цим було багато мороки - асинхроність ангуляра <3
        /// </summary>
        public bool IsOpen()
        {
            try
            {
                WaitForVisible(AccountInfoBox);
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                // Клієнт без рахунків — теж коректно відкрита сторінка.
                return IsPresent(NoAccountNotice);
            }
        }

        /// <summary>Адреса, на якій зараз браузер. Для повідомлень про падіння.</summary>
        public string CurrentUrl => Driver.Url;

        /// <summary>а хто це входить?</summary>
        public string GetCustomerName() => ReadText(WelcomeText);

        public IList<string> GetAvailableAccounts()
        {
            var select = new SelectElement(WaitForVisible(AccountSelect));
            return select.Options.Select(o => o.Text.Trim()).ToList();
        }

        public AccountPage SelectAccountByIndex(int index)
        {
            new SelectElement(WaitForVisible(AccountSelect)).SelectByIndex(index);
            return this;
        }

        /// <summary>
        /// Обрати рахунок за його номером.
        /// Саме за текстом: значення пунктів мають вигляд "number:1001".
        /// </summary>
        public AccountPage SelectAccountByNumber(string accountNumber)
        {
            new SelectElement(WaitForVisible(AccountSelect)).SelectByText(accountNumber);
            return this;
        }

        /// <summary>«Account Number : … Balance : … Currency : …».</summary>
        private IList<IWebElement> InfoValues()
        {
            return WaitForVisible(AccountInfoBox).FindElements(By.TagName("strong"));
        }

        public string GetAccountNumber() => InfoValues()[0].Text.Trim();

        public string GetCurrency() => InfoValues()[2].Text.Trim();

        /// <summary>
        /// Розбираємо в інваріантній культурі: на сторінці дробова частина 
        /// відокремлена крапкою незалежно від мови системи. P.S. ви це не читаєте ж?
        /// </summary>
        public decimal GetBalance()
        {
            var text = InfoValues()[1].Text.Trim();

            if (!decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var balance))
                throw new FormatException($"Не вдалося прочитати баланс: на сторінці «{text}»");

            return balance;
        }

        // ═══════════════════ вкладки ═══════════════════

        public AccountPage OpenWithdrawForm()
        {
            Click(WithdrawTab);
            WaitForVisible(WithdrawSubmit);
            return this;
        }

        public AccountPage OpenDepositForm()
        {
            Click(DepositTab);
            WaitForVisible(DepositSubmit);
            return this;
        }

        public AccountPage OpenTransactions()
        {
            Click(TransactionsTab);
            return this;
        }

        // ═══════════════════ операції ═══════════════════

        /// <summary>
        /// Зняти вказану суму: заповнити поле й натиснути Withdraw.
        /// </summary>
        public AccountPage Withdraw(decimal amount)
        {
            OpenWithdrawForm();

            var before = ReadMessageOrEmpty();

            EnterAmount(amount);
            Click(WithdrawSubmit);

            WaitForMessageToChangeFrom(before);
            return this;
        }

        /// <summary>
        /// Поповнити рахунок.
        ///
        /// Жоден сценарій цього не викликає.вкладка Deposit
        /// на цій сторінці є, тому метод для неї тут теж є. Предметом
        /// перевірки за варіантом 2 є Withdraw, і поповнення не тестується.
        /// Раніше цей метод використовувався для підготовки балансу, але
        /// після переходу на скидання стану потреба відпала)
        ///
        /// Раніше: використовувався тільки для підготовки даних
        /// (наприклад, щоб довести баланс до потрібного рівня), а не як
        /// предмет перевірки — предметом перевірки за варіантом є Withdraw.
        /// </summary>
        public AccountPage Deposit(decimal amount)
        {
            OpenDepositForm();
            EnterAmount(amount);
            var before = ReadMessageOrEmpty();
            Click(DepositSubmit);
            WaitForMessageToChangeFrom(before);
            return this;
        }

        /// <summary>сума - поле - ввели.</summary>
        public AccountPage EnterAmount(decimal amount)
        {
            var field = WaitForVisible(AmountInput);
            field.Clear();
            field.SendKeys(amount.ToString(CultureInfo.InvariantCulture));
            return this;
        }

        /// <summary>
        /// Що зараз залишилось у полі суми.
        ///
        /// Застосунок після кожної операції очищає це поле
        /// ($scope.amount = ""). Якщо після натискання кнопки поле
        /// лишилось заповненим — значить обробник не доопрацював до кінця.
        /// </summary>
        public string GetAmountFieldValue()
        {
            return WaitForVisible(AmountInput).GetAttribute("value") ?? string.Empty;
        }

        // ═══════════════════ повідомлення ═══════════════════

        /// <summary>є повідомленння?</summary>
        public bool IsMessageDisplayed()
        {
            var deadline = DateTime.UtcNow + MessageTimeout;

            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var span = Driver.FindElement(MessageSpan);
                    if (span.Displayed && span.Text.Trim().Length > 0)
                        return true;
                }
                catch (NoSuchElementException) { }
                catch (StaleElementReferenceException) { }

                System.Threading.Thread.Sleep(100);
            }

            return false;
        }

        /// <summary>
        /// Текст повідомлення про результат операції.
        /// Якщо повідомлення не з'явилось — повертаємо порожній рядок,
        /// щоб у звіті про падіння було видно саме це, а не таймаут пошуку.
        /// </summary>
        public string GetMessage()
        {
            if (!IsMessageDisplayed())
                return string.Empty;

            return Driver.FindElement(MessageSpan).Text.Trim();
        }

        /// <summary>Поточний текст повідомлення або порожній рядок, якщо його не показано.</summary>
        private string ReadMessageOrEmpty()
        {
            try
            {
                var span = Driver.FindElement(MessageSpan);
                return span.Displayed ? span.Text.Trim() : string.Empty;
            }
            catch (NoSuchElementException) { return string.Empty; }
            catch (StaleElementReferenceException) { return string.Empty; }
        }

        /// <summary>
        /// Чекати, поки повідомлення відрізнятиметься від переданого.
        /// Виходить за таймаутом, якщо зміни не сталося — без винятку.
        /// </summary>
        private void WaitForMessageToChangeFrom(string previous)
        {
            var deadline = DateTime.UtcNow + MessageTimeout;

            while (DateTime.UtcNow < deadline)
            {
                var current = ReadMessageOrEmpty();

                if (current.Length > 0 && current != previous)
                    return;

                System.Threading.Thread.Sleep(100);
            }
        }

        // ═══════════════════ вихід ═══════════════════

        public CustomerLoginPage Logout()
        {
            Click(LogoutButton);
            return new CustomerLoginPage(Driver);
        }
    }
}
