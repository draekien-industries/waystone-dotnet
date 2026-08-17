@option
Feature: Unwrap Extensions for Async Option

    Scenario: Unwrap on Task Some Option
        Given Option is Some with value 10
        And Option is wrapped in a Task
        When unwrapping the Task Option
        Then the unwrapped Option value should be 10

    Scenario: Unwrap on ValueTask Some Option
        Given Option is Some with value 20
        And Option is wrapped in a ValueTask
        When unwrapping the ValueTask Option
        Then the unwrapped Option value should be 20

    Scenario: Unwrap on Task None Option
        Given Option is None
        And Option is wrapped in a Task
        When unwrapping the Task Option
        Then an Option UnwrapException should be thrown

    Scenario: Unwrap on ValueTask None Option
        Given Option is None
        And Option is wrapped in a ValueTask
        When unwrapping the ValueTask Option
        Then an Option UnwrapException should be thrown

    Scenario: UnwrapOr on Task Some Option
        Given Option is Some with value 10
        And Option is wrapped in a Task
        When unwrapping the Task Option with a default of 99
        Then the unwrapped Option value should be 10

    Scenario: UnwrapOr on Task None Option
        Given Option is None
        And Option is wrapped in a Task
        When unwrapping the Task Option with a default of 99
        Then the unwrapped Option value should be 99

    Scenario: UnwrapOr on ValueTask Some Option
        Given Option is Some with value 30
        And Option is wrapped in a ValueTask
        When unwrapping the ValueTask Option with a default of 99
        Then the unwrapped Option value should be 30

    Scenario: UnwrapOr on ValueTask None Option
        Given Option is None
        And Option is wrapped in a ValueTask
        When unwrapping the ValueTask Option with a default of 99
        Then the unwrapped Option value should be 99

    Scenario: UnwrapOrDefault on Task Some Option
        Given Option is Some with value 10
        And Option is wrapped in a Task
        When unwrapping the Task Option or its default
        Then the unwrapped Option value should be 10

    Scenario: UnwrapOrDefault on Task None Option
        Given Option is None
        And Option is wrapped in a Task
        When unwrapping the Task Option or its default
        Then the unwrapped Option value should be 0

    Scenario: UnwrapOrDefault on ValueTask Some Option
        Given Option is Some with value 40
        And Option is wrapped in a ValueTask
        When unwrapping the ValueTask Option or its default
        Then the unwrapped Option value should be 40

    Scenario: UnwrapOrDefault on ValueTask None Option
        Given Option is None
        And Option is wrapped in a ValueTask
        When unwrapping the ValueTask Option or its default
        Then the unwrapped Option value should be 0
