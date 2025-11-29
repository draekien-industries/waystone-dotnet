Feature: InspectErr Extensions on Async Result

    Scenario: async InspectErr on Ok sync result
        Given an OK result with value 10
        And an async delegate for string returning Task
        When invoking InspectErrAsync on result with async delegate
        Then the output should be an OK result with value 10
        And the async delegate should not have been invoked

    Scenario: async InspectErr on Error sync result
        Given an Error result with value "Error"
        And an async delegate for string returning Task
        When invoking InspectErrAsync on result with async delegate
        Then the output should be an Error result with message "Error"
        And the async delegate should have been invoked once with message "Error"

    Scenario: async InspectErr on Ok async result Task
        Given an OK result with value 20
        And the result is wrapped in a Task
        And an async delegate for string returning Task
        When invoking InspectErrAsync on async Task result with async delegate
        Then the output should be an OK result with value 20
        And the async delegate should not have been invoked

    Scenario: async InspectErr on Error async result Task
        Given an Error result with value "Async Error"
        And the result is wrapped in a Task
        And an async delegate for string returning Task
        When invoking InspectErrAsync on async Task result with async delegate
        Then the output should be an Error result with message "Async Error"
        And the async delegate should have been invoked once with message "Async Error"

    Scenario: async InspectErr on Ok async result ValueTask
        Given an OK result with value 30
        And the result is wrapped in a ValueTask
        And an async delegate for string returning Task
        When invoking InspectErrAsync on async ValueTask result with async delegate
        Then the output should be an OK result with value 30
        And the async delegate should not have been invoked

    Scenario: async InspectErr on Error async result ValueTask
        Given an Error result with value "Async ValueTask Error"
        And the result is wrapped in a ValueTask
        And an async delegate for string returning Task
        When invoking InspectErrAsync on async ValueTask result with async delegate
        Then the output should be an Error result with message "Async ValueTask Error"
        And the async delegate should have been invoked once with message "Async ValueTask Error"
