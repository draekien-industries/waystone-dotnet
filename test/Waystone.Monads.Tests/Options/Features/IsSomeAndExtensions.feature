Feature: IsSomeAnd Extensions for Async Option

    Scenario: Task Option IsSomeAnd when Some with async predicate returning "true"
        Given Option is Some with value 15
        And Option is wrapped in a Task
        And an async predicate that returns "true" for int value
        When invoking IsSomeAnd on Option Task with the async predicate
        Then the result should be "true"

    Scenario: Task Option IsSomeAnd when Some with async predicate returning "false"
        Given Option is Some with value 25
        And Option is wrapped in a Task
        And an async predicate that returns "false" for int value
        When invoking IsSomeAnd on Option Task with the async predicate
        Then the result should be "false"

    Scenario: Task Option IsSomeAnd when None with async predicate
        Given Option is None
        And Option is wrapped in a Task
        And an async predicate that returns "true" for int value
        When invoking IsSomeAnd on Option Task with the async predicate
        Then the result should be "false"

    Scenario: ValueTask Option IsSomeAnd when Some with async predicate returning "true"
        Given Option is Some with value 35
        And Option is wrapped in a ValueTask
        And an async predicate that returns "true" for int value
        When invoking IsSomeAnd on Option ValueTask with the async predicate
        Then the result should be "true"

    Scenario: ValueTask Option IsSomeAnd when Some with async predicate returning "false"
        Given Option is Some with value 45
        And Option is wrapped in a ValueTask
        And an async predicate that returns "false" for int value
        When invoking IsSomeAnd on Option ValueTask with the async predicate
        Then the result should be "false"

    Scenario: ValueTask Option IsSomeAnd when None with async predicate
        Given Option is None
        And Option is wrapped in a ValueTask
        And an async predicate that returns "true" for int value
        When invoking IsSomeAnd on Option ValueTask with the async predicate
        Then the result should be "false"
