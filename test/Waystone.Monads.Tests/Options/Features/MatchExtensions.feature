Feature: Match Extensions for Async Option

    Scenario: Task Option Match when Some
        Given Option is Some with value 42
        And Option is wrapped in a Task
        And an async "OnSome" function that returns "Value is 42"
        And an async "OnNone" function that returns "No Value"
        When invoking Match on Option Task with "OnSome" and "OnNone" handlers
        Then the result should be "Value is 42"

    Scenario: Task Option Match when None
        Given Option is None
        And Option is wrapped in a Task
        And an async "OnSome" function that returns "Value is 42"
        And an async "OnNone" function that returns "No Value"
        When invoking Match on Option Task with "OnSome" and "OnNone" handlers
        Then the result should be "No Value"

    Scenario: ValueTask Option Match when Some
        Given Option is Some with value 100
        And Option is wrapped in a ValueTask
        And an async "OnSome" function that returns "Value is 100"
        And an async "OnNone" function that returns "No Value"
        When invoking Match on Option ValueTask with "OnSome" and "OnNone" handlers
        Then the result should be "Value is 100"

    Scenario: ValueTask Option Match when None
        Given Option is None
        And Option is wrapped in a ValueTask
        And an async "OnSome" function that returns "Value is 100"
        And an async "OnNone" function that returns "No Value"
        When invoking Match on Option ValueTask with "OnSome" and "OnNone" handlers
        Then the result should be "No Value"
