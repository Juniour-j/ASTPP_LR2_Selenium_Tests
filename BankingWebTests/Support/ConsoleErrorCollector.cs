using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace BankingWebTests.Support
{
    /// <summary>
    /// Збирає помилки JavaScript, які виникають на сторінці під час сценарію.
    /// </summary>
    public static class ConsoleErrorCollector
    {
        /// <summary>
        /// Ознаки необробленого винятку.
        /// </summary>
        private static readonly string[] ExceptionMarkers =
        {
            "TypeError",
            "ReferenceError",
            "RangeError",
            "is not a function",
            "Cannot read"
        };

        /// <summary>
        /// Відкинути все, що накопичилось у журналі до початку сценарію
        /// </summary>
        public static void Install(IWebDriver driver)
        {
            ReadBrowserLog(driver);
        }

        public static IList<string> GetAll(IWebDriver driver)
        {
            return ReadBrowserLog(driver);
        }

        /// <summary>Лише ті записи, що є ознакою необробленого винятку.</summary>
        public static IList<string> GetUnhandledExceptions(IWebDriver driver)
        {
            return ReadBrowserLog(driver)
                   .Where(message => ExceptionMarkers.Any(
                       marker => message.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0))
                   .ToList();
        }


        private static IList<string> ReadBrowserLog(IWebDriver driver)
        {
            try
            {
                return driver.Manage().Logs.GetLog(LogType.Browser)
                             .Select(entry => $"[{entry.Level}] {entry.Message}")
                             .ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Не вдалося прочитати журнал консолі браузера, тому перевірити " +
                    "наявність необроблених винятків неможливо. Тест не може вважатися " +
                    "пройденим. Причина: " + ex.Message, ex);
            }
        }
    }
}
