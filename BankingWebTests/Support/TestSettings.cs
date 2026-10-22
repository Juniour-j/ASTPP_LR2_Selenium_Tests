using System;

namespace BankingWebTests.Support
{

    public static class TestSettings
    {

        public static string BaseUrl =>
            Environment.GetEnvironmentVariable("BANK_URL")
            ?? "https://www.globalsqa.com/angularJs-protractor/BankingProject/#/login";

        /// <summary>
        /// Клієнт, під яким виконуються сценарії.
        ///
        /// Обрано саме Hermoine Granger, бо застосунок при першому запуску
        /// створює для неї 196 операцій (по 7 місяців × 28 днів,
        /// кожного дня +30 і −4), тобто її перший рахунок 1001 має
        /// заздалегідь відомий баланс 5096. У решти клієнтів баланс нульовий,
        /// і на них неможливо перевірити успішне зняття коштів. P.S expecto patronum!
        /// </summary>
        public static string DefaultCustomer =>
            Environment.GetEnvironmentVariable("BANK_CUSTOMER") ?? "Hermoine Granger";

        public const string DefaultAccountNumber = "1001";

       
        public const string DefaultCurrency = "Dollar";

       
        public const decimal InitialBalance = 5096m;

        /// <summary>
        /// Режим без вікна браузера. За замовчуванням вимкнено
        /// </summary>
        public static bool Headless =>
            string.Equals(Environment.GetEnvironmentVariable("BANK_HEADLESS"), "true",
                          StringComparison.OrdinalIgnoreCase);

        
        public static TimeSpan ImplicitWait => TimeSpan.Zero;

       
        public static TimeSpan ExplicitWait => TimeSpan.FromSeconds(10);

        // ═══════════════════════════════════════════════════════════════
        // Повідомлення застосунку
        // ═══════════════════════════════════════════════════════════════

        public const string WithdrawSuccessMessage = "Transaction successful";

        public const string WithdrawRejectedMessage =
            "Transaction Failed. You can not withdraw amount more than the balance.";

        /// <summary>Успішне поповнення (для повноти; операція не тестується).</summary>
        public const string DepositSuccessMessage = "Deposit Successful";

        /// <summary>Тека, куди складати знімки екрана при падінні тесту.</summary>
        public static string ScreenshotFolder =>
            Environment.GetEnvironmentVariable("BANK_SCREENSHOTS") ?? "Screenshots";
    }
}
