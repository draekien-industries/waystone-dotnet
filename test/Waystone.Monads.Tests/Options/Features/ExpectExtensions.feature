Feature: Expect Extensions for Option

    Scenario: Expect on Task Some Option
        Given Option is Some with value 10
        And Option is wrapped in a Task
        When expecting a Some from the Task Option with message "Expected a Some Option"
        Then the expected Option value should be 10

    Scenario: Expect on ValueTask Some Option
        Given Option is Some with value 20
        And Option is wrapped in a ValueTask
        When expecting a Some from the ValueTask Option with message "Expected a Some Option"
        Then the expected Option value should be 20

    Scenario: Expect on Task None Option
        Given Option is None
        And Option is wrapped in a Task
        When expecting a Some from the Task Option with message "Expected a Some Option"
        Then an Option UnmetExpectationException should be thrown containing "Expected a Some Option"

    Scenario: Expect on ValueTask None Option
        Given Option is None
        And Option is wrapped in a ValueTask
        When expecting a Some from the ValueTask Option with message "Expected a Some Option"
        Then an Option UnmetExpectationException should be thrown containing "Expected a Some Option"
