# ЛР2 — Web тестування з використанням Selenium

**Викович Назарій, група ПМ-33, варіант 2**
Дисципліна: Автоматизовані системи тестування програмного продукту
Викладач: Любінський Б. Б.

---

## Що тестується

| | |
|---|---|
| Сайт | https://www.globalsqa.com/angularJs-protractor/BankingProject |
| Вхід | **Customer Login** |
| Операція за варіантом | **Withdraw** (зняття коштів) |

## Чим тестується

| Інструмент | Версія | Навіщо |
|---|---|---|
| .NET | 10.0 | платформа |
| Reqnroll.MsTest | 3.3.4 | BDD, розбір Gherkin, генерація тестів (наступник SpecFlow, який припинив підтримку) |
| MSTest | 3.6.4 | середовище виконання тестів |
| Selenium.WebDriver | 4.49.0 | керування браузером |
| Selenium.Support | 4.49.0 | `SelectElement`, `WebDriverWait` |

Окремо завантажувати `chromedriver` **не потрібно**: починаючи з Selenium 4.6
у складі бібліотеки працює Selenium Manager, який сам знаходить встановлений
Chrome і підбирає до нього драйвер відповідної версії.

---

## Як запустити

1. Відкрити `BankingWebTests.sln` у Visual Studio 2022.
2. Дочекатись, поки NuGet відновить пакети (або `Build → Rebuild Solution`).
3. `Test → Test Explorer` → **Run All Tests**.

З командного рядка:

```bash
dotnet test
```

Запустити лише коректні сценарії або лише сценарії з дефектами:

```bash
dotnet test --filter "TestCategory=positive"
dotnet test --filter "TestCategory=defect"
```

Прогін без вікна браузера (для сервера збірки):

```bash
BANK_HEADLESS=true dotnet test
```

---

## Структура проєкту

```
BankingWebTests/
├── Features/
│   ├── Withdraw.feature          ← функціональні сценарії операції Withdraw
│   └── WithdrawReliability.feature ← перевірка надійності (окремий тег)
├── StepDefinitions/
│   └── WithdrawStepDefinitions.cs ← прив'язка кроків Gherkin до коду
├── Pages/                         ← Page Object Pattern
│   ├── BasePage.cs                   спільне: драйвер і явні очікування
│   ├── HomePage.cs                   стартова сторінка
│   ├── CustomerLoginPage.cs          вибір клієнта і вхід
│   └── AccountPage.cs                рахунок + форма Withdraw
├── Support/
│   ├── TestSettings.cs            налаштування в одному місці
│   ├── DriverFactory.cs           створення браузера
│   ├── BrowserHooks.cs            відкриття/закриття браузера, знімки екрана
│   └── ConsoleErrorCollector.cs   перехоплення помилок JavaScript
└── reqnroll.json                  конфігурація Reqnroll
```

---

## Чому тестові дані детерміновані

XYZ Bank — це застосунок на AngularJS без сервера: усі дані він тримає
у `localStorage` браузера. Якщо сховище порожнє, при завантаженні
виконується `mockDataLoadService.js`, який створює:

* 5 клієнтів (Hermoine Granger, Harry Potter, Ron Weasly, Albus Dumbledore,
  Neville Longbottom), кожному по 3 рахунки — Dollar, Pound, Rupee;
* операції **тільки для першого клієнта на першому рахунку**:
  7 місяців × 28 днів, щодня «+30 і −4».

Звідси початковий баланс рахунку **1001** клієнтки Hermoine Granger:

```
7 × 28 × (30 − 4) = 196 × 26 = 5096
```

Решта рахунків порожні.

Крок `Given the application state is reset` очищає `localStorage`
і перезавантажує сторінку, тож **кожен сценарій стартує з балансу 5096**.
Саме тому у сценаріях можна писати конкретні очікувані суми, а не
«баланс зменшився приблизно на стільки ж»: це не здогадка, а точне число.

---


## Точні тексти повідомлень застосунку

Зняті з вихідного коду `withdrawlController.js` / `depositController.js`
і перевірені на живому сайті — тести звіряють їх посимвольно:

| Подія | Текст |
|---|---|
| Успішне зняття | `Transaction successful` |
| Сума більша за баланс | `Transaction Failed. You can not withdraw amount more than the balance.` |
| Успішне поповнення | `Deposit Successful` |
