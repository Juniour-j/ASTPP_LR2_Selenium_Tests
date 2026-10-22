#language: en
@withdraw @reliability
Feature: Reliability of the Withdraw operation
    As a tester
    I want the application not to fail silently
    So that a broken operation cannot look like a working one

    # ─────────────────────────────────────────────────────────────────────
    #
    #  (Лекція 1, п. 2.3: «Надійність – здатність системи
    # стабільно виконувати свої функції без збоїв»).
    # ─────────────────────────────────────────────────────────────────────

    Background:
        Given the XYZ Bank application is open
        And the application state is reset
        And the customer "Hermoine Granger" is logged in
        And the account "1001" is selected

    @defect @D3
    Scenario: D3 - Withdrawing zero does not raise an unhandled JavaScript exception
        When the customer withdraws 0
        Then no unhandled JavaScript exception occurred
