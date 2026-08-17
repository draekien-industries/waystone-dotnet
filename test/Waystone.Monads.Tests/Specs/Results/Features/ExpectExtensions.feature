@result
Feature: Expect Extensions for Async Result

    Scenario: Expect on Task OK Result
        Given an OK result with value 10
        And the result is wrapped in a Task
        When expecting an Ok from the Task Result with message "Expected an Ok Result"
        Then the expected value should be 10

    Scenario: Expect on ValueTask OK Result
        Given an OK result with value 20
        And the result is wrapped in a ValueTask
        When expecting an Ok from the ValueTask Result with message "Expected an Ok Result"
        Then the expected value should be 20

    Scenario: Expect on Task Error Result
        Given an Error result with value "Error occurred"
        And the result is wrapped in a Task
        When expecting an Ok from the Task Result with message "Expected an Ok Result"
        Then an UnmetExpectationException should be thrown containing "Expected an Ok Result"

    Scenario: Expect on ValueTask Error Result
        Given an Error result with value "Error occurred"
        And the result is wrapped in a ValueTask
        When expecting an Ok from the ValueTask Result with message "Expected an Ok Result"
        Then an UnmetExpectationException should be thrown containing "Expected an Ok Result"

    Scenario: ExpectErr on Task Error Result
        Given an Error result with value "Error occurred"
        And the result is wrapped in a Task
        When expecting an Err from the Task Result with message "Expected an Err Result"
        Then the expected error should be "Error occurred"

    Scenario: ExpectErr on ValueTask Error Result
        Given an Error result with value "Critical Error"
        And the result is wrapped in a ValueTask
        When expecting an Err from the ValueTask Result with message "Expected an Err Result"
        Then the expected error should be "Critical Error"

    Scenario: ExpectErr on Task OK Result
        Given an OK result with value 10
        And the result is wrapped in a Task
        When expecting an Err from the Task Result with message "Expected an Err Result"
        Then an UnmetExpectationException should be thrown containing "Expected an Err Result"

    Scenario: ExpectErr on ValueTask OK Result
        Given an OK result with value 10
        And the result is wrapped in a ValueTask
        When expecting an Err from the ValueTask Result with message "Expected an Err Result"
        Then an UnmetExpectationException should be thrown containing "Expected an Err Result"
