#language: en
@withdraw
Feature: Withdrawing money from an XYZ Bank account
    As a customer of XYZ Bank
    I want to withdraw money from my account
    So that I can use the funds I have on it



    Background:
        Given the XYZ Bank application is open
        And the application state is reset
        And the customer "Hermoine Granger" is logged in
        And the account "1001" is selected
        And the starting balance is 5096

    # ─────────────────────────────────────────────────────────────────────
    # 1. ЗНЯТТЯ СУМИ В МЕЖАХ БАЛАНСУ
    #    Джерело вимоги: (3) зняття зменшує баланс рівно на зняту суму;
    #                    (1) текст повідомлення про успіх.
    # ─────────────────────────────────────────────────────────────────────

    @positive
    Scenario: Withdrawing part of the balance succeeds
        When the customer withdraws 100
        Then the withdrawal is reported as successful
        And the balance is 4996

    @positive
    Scenario Outline: The balance decreases by exactly the withdrawn amount
        When the customer withdraws <amount>
        Then the withdrawal is reported as successful
        And the balance is <remaining>

        Examples:
            | amount | remaining |
            | 1      | 5095      |
            | 50     | 5046      |
            | 500    | 4596      |
            | 5095   | 1         |

    @positive
    Scenario: Consecutive withdrawals are accumulated
        When the customer withdraws 1000
        And the customer withdraws 2000
        Then the balance is 2096

    @positive
    Scenario: A withdrawal affects only the selected account
        When the customer withdraws 96
        Then the balance is 5000
        When the account "1002" is selected
        Then the balance is 0
        When the account "1003" is selected
        Then the balance is 0

    @positive
    Scenario: A withdrawal does not change the account number or the currency
        When the customer withdraws 42
        Then the balance is 5054
        And the account number is "1001"
        And the currency is "Dollar"

    # Окремий сценарій, предмет якого — ФОРМУЛЮВАННЯ.
    
    @contract
    Scenario: The application reports outcomes with the exact texts it declares
        When the customer withdraws 100
        Then the message "Transaction successful" is displayed
        When the customer withdraws 999999
        Then the message "Transaction Failed. You can not withdraw amount more than the balance." is displayed

    # ─────────────────────────────────────────────────────────────────────
    # 2. МЕЖІ
    #    Джерело вимоги: (3) баланс рахунку не буває від'ємним;
    #                    (2) на кожен запит застосунок відповідає.
    # ────────────────────────────────────────────────────────────────────

    @boundary
    Scenario: Withdrawing the whole balance is answered and leaves no negative balance
        When the customer withdraws 5096
        Then a message about the result of the operation is displayed
        And the balance is not negative

    @boundary @negative
    Scenario: Withdrawing one unit more than the balance is rejected
        When the customer withdraws 5097
        Then the withdrawal is rejected
        And the balance is 5096

    @negative
    Scenario Outline: A withdrawal larger than the balance never changes the balance
        When the customer withdraws <amount>
        Then the withdrawal is rejected
        And the balance is not changed

        Examples:
            | amount  |
            | 5097    |
            | 10000   |
            | 999999  |

    @negative
    Scenario: Withdrawing from an empty account is rejected
        When the account "1002" is selected
        And the customer withdraws 1
        Then the withdrawal is rejected
        And the balance is 0

    # ─────────────────────────────────────────────────────
    # 3. НЕКОРЕКТНА СУМА
    #    Джерело вимоги: (2) на кожен запит застосунок відповідає —
    #                    для 100 і для 5097 він відповідає, отже мовчати
    #                    для 0 чи −100 не має права;
    #                    (3) сума в доларах може містити центи, і таку
    #                    суму застосунок мусить прийняти.
    # ─────────────────────────────────────────────────────

    @negative @defect @D1
    Scenario: D1 - Withdrawing zero is answered with a message
        When the customer withdraws 0
        Then a message about the result of the operation is displayed

    @negative @defect @D2
    Scenario: D2 - The amount field is cleared after a zero withdrawal
        When the customer withdraws 0
        Then the amount field is cleared

    @negative @defect @D4
    Scenario: D4 - A negative amount is answered with a message
        When the customer withdraws -100
        Then a message about the result of the operation is displayed

    # Цей сценарій ПРОХОДИТЬ, і тега @defect на ньому немає.
    # Обробник обривається до того, як щось записати, тож баланс
    # лишається недоторканим. Перевірка потрібна саме тому, що
    # дефект Д4 стосується мовчазної відмови, а не втрати коштів.
    @negative
    Scenario: A negative amount does not change the balance
        When the customer withdraws -100
        Then the balance is 5096

    # Рахунок 1001 ведеться в доларах, отже найменша одиниця — цент.
    # Сума 10.50 цілком звичайн
    # Фактично поле суми має type="number" без атрибута step, тому крок
    # за замовчуванням дорівнює одиниці, і браузер відхиляє будь-яке
    # дробове значення власною перевіркою: «Введіть дійсне значення.
    # Два найближчі дійсні значення: 10 і 11». Форма навіть не
    # відправляється — клієнт не може зняти суму з копійками взагалі.
    @positive @defect @D5
    Scenario: D5 - An amount with cents can be withdrawn
        When the customer withdraws 10.5
        Then the withdrawal is reported as successful
        And the balance is 5085.5

    # ─────────────────────────────────────────────────────────────────────
    # 4. СТАН ФОРМИ ТА ПОВІДОМЛЕННЯ
    # ─────────────────────────────────────────────────────────────────────

    @positive
    Scenario: The amount field is cleared after a successful withdrawal
        When the customer withdraws 10
        Then the withdrawal is reported as successful
        And the amount field is cleared

    @negative
    Scenario: The amount field is cleared after a rejected withdrawal
        When the customer withdraws 999999
        Then the withdrawal is rejected
        And the amount field is cleared

    @defect @D6
    Scenario: D6 - The error message disappears when another account is selected
        When the customer withdraws 999999
        Then the withdrawal is rejected
        When the account "1002" is selected
        Then no message about the result of the operation is displayed
