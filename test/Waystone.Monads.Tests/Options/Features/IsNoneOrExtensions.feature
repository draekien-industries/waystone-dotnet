Feature: IsNoneOr Extensions for Async Option

    Scenario: Task Option IsNoneOr when Some with async predicate
        Given Option is Some with value 55
        And Option is wrapped in a Task
        When invoking IsNoneOr on Option Task with async predicate that returns "true"
        Then the result should be "true"

    Scenario: Task Option IsNoneOr when None with async predicate
        Given Option is None
        And Option is wrapped in a Task
        When invoking IsNoneOr on Option Task with async predicate that returns "true"
        Then the result should be "true"

    Scenario: Task Option IsNoneOr when None with async predicate that returns "false"
        Given Option is None
        And Option is wrapped in a Task
        When invoking IsNoneOr on Option Task with async predicate that returns "false"
        Then the result should be "true"

    Scenario: ValueTask Option IsNoneOr when Some with async predicate
        Given Option is Some with value 75
        And Option is wrapped in a ValueTask
        When invoking IsNoneOr on Option ValueTask with async predicate that returns "true"
        Then the result should be "true"

    Scenario: ValueTask Option IsNoneOr when None with async predicate
        Given Option is None
        And Option is wrapped in a ValueTask
        When invoking IsNoneOr on Option ValueTask with async predicate that returns "true"
        Then the result should be "true"

    Scenario: ValueTask Option IsNoneOr when None with async predicate that returns "false"
        Given Option is None
        And Option is wrapped in a ValueTask
        When invoking IsNoneOr on Option ValueTask with async predicate that returns "false"
        Then the result should be "true"

    Scenario: Task Option IsNoneOr when Some with sync predicate
        Given Option is Some with value 85
        And Option is wrapped in a Task
        When invoking IsNoneOr on Option Task with sync predicate that returns "true"
        Then the result should be "true"

    Scenario: Task Option IsNoneOr when None with sync predicate
        Given Option is None
        And Option is wrapped in a Task
        When invoking IsNoneOr on Option Task with sync predicate that returns "true"
        Then the result should be "true"

    Scenario: Task Option IsNoneOr when None with sync predicate that returns "false"
        Given Option is None
        And Option is wrapped in a Task
        When invoking IsNoneOr on Option Task with sync predicate that returns "false"
        Then the result should be "true"
