Feature: Match Extensions for Async Option

    Scenario: Task Option Match when Some
        Given Option is Some with value 42
        And Option is wrapped in a Task "OptionTask"
        And an async "OnSome" function that returns "Value is 42"
        And an async "OnNone" function that returns "No Value"
        When invoking Match on "OptionTask" with "OnSome" and "OnNone" handlers
        Then the result should be "Value is 42"

    Scenario: Task Option Match when None
        Given Option is None
        And Option is wrapped in a Task "OptionTask"
        And an async "OnSome" function that returns "Value is 42"
        And an async "OnNone" function that returns "No Value"
        When invoking Match on "OptionTask" with "OnSome" and "OnNone" handlers
        Then the result should be "No Value"

    Scenario: ValueTask Option Match when Some
        Given Option is Some with value 100
        And Option is wrapped in a ValueTask "OptionValueTask"
        And an async "OnSome" function that returns "Value is 100"
        And an async "OnNone" function that returns "No Value"
        When invoking Match on "OptionValueTask" with "OnSome" and "OnNone" handlers
        Then the result should be "Value is 100"

    Scenario: ValueTask Option Match when None
        Given Option is None
        And Option is wrapped in a ValueTask "OptionValueTask"
        And an async "OnSome" function that returns "Value is 100"
        And an async "OnNone" function that returns "No Value"
        When invoking Match on "OptionValueTask" with "OnSome" and "OnNone" handlers
        Then the result should be "No Value"
