@result
Feature: Unwrap Extensions for Async Result

    Scenario: Unwrap on Task OK Result
        Given an OK result with value 10
        And the result is wrapped in a Task
        When unwrapping the Task Result
        Then the unwrapped value should be 10

    Scenario: Unwrap on ValueTask OK Result
        Given an OK result with value 20
        And the result is wrapped in a ValueTask
        When unwrapping the ValueTask Result
        Then the unwrapped value should be 20

    Scenario: Unwrap on Task Error Result
        Given an Error result with value "Error occurred"
        And the result is wrapped in a Task
        When unwrapping the Task Result
        Then an UnwrapException should be thrown

    Scenario: Unwrap on ValueTask Error Result
        Given an Error result with value "Error occurred"
        And the result is wrapped in a ValueTask
        When unwrapping the ValueTask Result
        Then an UnwrapException should be thrown

    Scenario: UnwrapErr on Task Error Result
        Given an Error result with value "Error occurred"
        And the result is wrapped in a Task
        When unwrapping the error of the Task Result
        Then the unwrapped error should be "Error occurred"

    Scenario: UnwrapErr on ValueTask Error Result
        Given an Error result with value "Critical Error"
        And the result is wrapped in a ValueTask
        When unwrapping the error of the ValueTask Result
        Then the unwrapped error should be "Critical Error"

    Scenario: UnwrapErr on Task OK Result
        Given an OK result with value 10
        And the result is wrapped in a Task
        When unwrapping the error of the Task Result
        Then an UnwrapException should be thrown

    Scenario: UnwrapErr on ValueTask OK Result
        Given an OK result with value 10
        And the result is wrapped in a ValueTask
        When unwrapping the error of the ValueTask Result
        Then an UnwrapException should be thrown

    Scenario: UnwrapOr on Task OK Result
        Given an OK result with value 10
        And the result is wrapped in a Task
        When unwrapping the Task Result with a default of 99
        Then the unwrapped value should be 10

    Scenario: UnwrapOr on Task Error Result
        Given an Error result with value "Error occurred"
        And the result is wrapped in a Task
        When unwrapping the Task Result with a default of 99
        Then the unwrapped value should be 99

    Scenario: UnwrapOr on ValueTask OK Result
        Given an OK result with value 30
        And the result is wrapped in a ValueTask
        When unwrapping the ValueTask Result with a default of 99
        Then the unwrapped value should be 30

    Scenario: UnwrapOr on ValueTask Error Result
        Given an Error result with value "Fatal Error"
        And the result is wrapped in a ValueTask
        When unwrapping the ValueTask Result with a default of 99
        Then the unwrapped value should be 99

    Scenario: UnwrapOrDefault on Task OK Result
        Given an OK result with value 10
        And the result is wrapped in a Task
        When unwrapping the Task Result or its default
        Then the unwrapped value should be 10

    Scenario: UnwrapOrDefault on Task Error Result
        Given an Error result with value "Error occurred"
        And the result is wrapped in a Task
        When unwrapping the Task Result or its default
        Then the unwrapped value should be 0

    Scenario: UnwrapOrDefault on ValueTask OK Result
        Given an OK result with value 40
        And the result is wrapped in a ValueTask
        When unwrapping the ValueTask Result or its default
        Then the unwrapped value should be 40

    Scenario: UnwrapOrDefault on ValueTask Error Result
        Given an Error result with value "Severe Error"
        And the result is wrapped in a ValueTask
        When unwrapping the ValueTask Result or its default
        Then the unwrapped value should be 0
