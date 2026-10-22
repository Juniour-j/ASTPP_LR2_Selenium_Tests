using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using BankingWebTests.Pages;
using BankingWebTests.Support;
using Reqnroll;

namespace BankingWebTests.StepDefinitions
{
    /// <summary>
    /// Прив'язка кроків сценарію Gherkin до коду
    /// </summary>
    [Binding]
    public class WithdrawStepDefinitions
    {
        private readonly IWebDriver _driver;

        private HomePage _homePage;
        private CustomerLoginPage _loginPage;
        private AccountPage _accountPage;

        private decimal _balanceBeforeOperation;

        public WithdrawStepDefinitions(IWebDriver driver)
        {
            _driver = driver;
        }

        [Given(@"the XYZ Bank application is open")]
        public void GivenTheApplicationIsOpen()
        {
            _homePage = new HomePage(_driver).Open(TestSettings.BaseUrl);

            Assert.IsTrue(_homePage.IsOpen(),
                "Стартова сторінка XYZ Bank не відкрилася або має інший вигляд");
        }

        [Given(@"the application state is reset")]
        public void GivenTheApplicationStateIsReset()
        {

            // ресетаємось
            _homePage.ResetApplicationState();

            // ловимо помилоки з самого початку
            ConsoleErrorCollector.Install(_driver);
        }

        [Given(@"the customer ""(.*)"" is logged in")]
        public void GivenTheCustomerIsLoggedIn(string customerName)
        {
            _loginPage = _homePage.GoToCustomerLogin();

            var available = _loginPage.GetAvailableCustomers();
            CollectionAssert.Contains(available.ToArray(), customerName,
                $"ПЕРЕДУМОВА НЕ ВИКОНАНА: у списку клієнтів немає «{customerName}». " +
                $"Доступні: {string.Join(", ", available)}");

            _accountPage = _loginPage.LoginAs(customerName);

            Assert.IsTrue(_accountPage.IsOpen(),
                "ПЕРЕДУМОВА НЕ ВИКОНАНА: після входу не відкрилася сторінка рахунку. " +
                $"Поточна адреса: {_accountPage.CurrentUrl}");
        }

        [Given(@"the account ""(.*)"" is selected")]
        [When(@"the account ""(.*)"" is selected")]
        public void SelectAccount(string accountNumber)
        {
            _accountPage.SelectAccountByNumber(accountNumber);

            Assert.AreEqual(accountNumber, _accountPage.GetAccountNumber(),
                "ПЕРЕДУМОВА НЕ ВИКОНАНА: обрано не той рахунок, який вимагає сценарій");
        }

        /// <summary>
        /// Початковий баланс рахунку
        /// </summary>
        [Given(@"the starting balance is (-?[\d.]+)")]
        public void GivenTheStartingBalanceIs(decimal expected)
        {
            var actual = _accountPage.GetBalance();

            Assert.AreEqual(expected, actual,
                $"ПЕРЕДУМОВА НЕ ВИКОНАНА: очікувався початковий баланс {expected}, " +
                $"фактично {actual}. Найімовірніше не спрацювало скидання стану застосунку — " +
                "сама операція Withdraw тут ні до чого.");
        }

        // ═══════════════════════════ WHEN ════════════════════════════

        [When(@"the customer withdraws (-?[\d.]+)")]
        public void WhenTheCustomerWithdraws(decimal amount)
        {
            _balanceBeforeOperation = _accountPage.GetBalance();
            _accountPage.Withdraw(amount);
        }

        // ═══════════════════════════ THEN ════════════════════════════

        [Then(@"the message ""(.*)"" is displayed")]
        public void ThenTheMessageIsDisplayed(string expectedMessage)
        {
            var actual = _accountPage.GetMessage();

            Assert.AreEqual(expectedMessage, actual,
                actual.Length == 0
                    ? $"Очікувалось повідомлення «{expectedMessage}», але сторінка не показала жодного"
                    : $"Очікувалось повідомлення «{expectedMessage}», отримано «{actual}»");
        }


        [Then(@"the withdrawal is reported as successful")]
        public void ThenTheWithdrawalIsReportedAsSuccessful()
        {
            var actual = _accountPage.GetMessage();

            Assert.AreEqual(TestSettings.WithdrawSuccessMessage, actual,
                actual.Length == 0
                    ? "Застосунок не повідомив про результат операції"
                    : $"Операція мала бути виконана, але застосунок відповів «{actual}»");
        }

        /// <summary> Формулювання відмови — у TestSettings.</summary>
        [Then(@"the withdrawal is rejected")]
        public void ThenTheWithdrawalIsRejected()
        {
            var actual = _accountPage.GetMessage();

            Assert.AreEqual(TestSettings.WithdrawRejectedMessage, actual,
                actual.Length == 0
                    ? "Застосунок не повідомив про результат операції"
                    : $"Операція мала бути відхилена, але застосунок відповів «{actual}»");
        }

        [Then(@"a message about the result of the operation is displayed")]
        public void ThenAMessageIsDisplayed()
        {
            Assert.IsTrue(_accountPage.IsMessageDisplayed(),
                "Після натискання кнопки Withdraw сторінка не показала жодного повідомлення. " +
                "Клієнт не може зрозуміти, виконано операцію чи ні.");
        }

        [Then(@"no message about the result of the operation is displayed")]
        public void ThenNoMessageIsDisplayed()
        {
            var actual = _accountPage.GetMessage();

            Assert.IsFalse(_accountPage.IsMessageDisplayed(),
                $"На сторінці залишилось повідомлення «{actual}», хоча воно вже не стосується " +
                "поточного стану і має бути прибране.");
        }

        [Then(@"the balance is (-?[\d.]+)")]
        public void ThenTheBalanceIs(decimal expected)
        {
            var actual = _accountPage.GetBalance();

            Assert.AreEqual(expected, actual,
                $"Очікувався баланс {expected}, фактично {actual}");
        }

        [Then(@"the balance is not negative")]
        public void ThenTheBalanceIsNotNegative()
        {
            var actual = _accountPage.GetBalance();

            Assert.IsTrue(actual >= 0,
                $"Баланс банківського рахунку не може бути від'ємним, фактично {actual}");
        }

        [Then(@"the balance is not changed")]
        public void ThenTheBalanceIsNotChanged()
        {
            var actual = _accountPage.GetBalance();

            Assert.AreEqual(_balanceBeforeOperation, actual,
                $"Баланс не мав змінитися: було {_balanceBeforeOperation}, стало {actual}");
        }

        [Then(@"the account number is ""(.*)""")]
        public void ThenTheAccountNumberIs(string expected)
        {
            Assert.AreEqual(expected, _accountPage.GetAccountNumber(),
                "Номер рахунку змінився після операції");
        }

        [Then(@"the currency is ""(.*)""")]
        public void ThenTheCurrencyIs(string expected)
        {
            Assert.AreEqual(expected, _accountPage.GetCurrency(),
                "Валюта рахунку змінилася після операції");
        }

        [Then(@"the amount field is cleared")]
        public void ThenTheAmountFieldIsCleared()
        {
            var value = _accountPage.GetAmountFieldValue();

            Assert.AreEqual(string.Empty, value,
                $"Поле суми мало очиститись після операції, але в ньому лишилось «{value}». " +
                "Це ознака того, що обробник операції не доопрацював до кінця.");
        }

        [Then(@"no unhandled JavaScript exception occurred")]
        public void ThenNoUnhandledJavaScriptExceptionOccurred()
        {
            var exceptions = ConsoleErrorCollector.GetUnhandledExceptions(_driver);

            Assert.AreEqual(0, exceptions.Count,
                "Під час операції у браузері виник необроблений виняток JavaScript:" +
                Environment.NewLine +
                string.Join(Environment.NewLine + "---" + Environment.NewLine, exceptions));
        }
    }
}
