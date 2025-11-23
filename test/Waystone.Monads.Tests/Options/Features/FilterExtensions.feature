Feature: Filter Extensions for Async Option

    Scenario: Task Option Filter when Some and Async Predicate "true"
        Given Option is Some with value 10
        And Option is wrapped in a Task
        And an async predicate that returns "true" for int value
        When invoking Filter on Option Task with the async predicate
        Then the result Option should be Some with value 10

    Scenario: Task Option Filter when Some and Async Predicate "false"
        Given Option is Some with value 20
        And Option is wrapped in a Task
        And an async predicate that returns "false" for int value
        When invoking Filter on Option Task with the async predicate
        Then the result Option should be None

    Scenario: Task Option Filter when None
        Given Option is None
        And Option is wrapped in a Task
        And an async predicate that returns "true" for int value
        When invoking Filter on Option Task with the async predicate
        Then the result Option should be None

    Scenario: Task Option Filter when Some and Sync Predicate "true"
        Given Option is Some with value 25
        And Option is wrapped in a Task
        And a sync predicate that returns "true" for int value
        When invoking Filter on Option Task with the sync predicate
        Then the result Option should be Some with value 25

    Scenario: Task Option Filter when Some and Sync Predicate "false"
        Given Option is Some with value 25
        And Option is wrapped in a Task
        And a sync predicate that returns "false" for int value
        When invoking Filter on Option Task with the sync predicate
        Then the result Option should be None

    Scenario: Task Option Filter when None and Sync Predicate
        Given Option is None
        And Option is wrapped in a Task
        And a sync predicate that returns "true" for int value
        When invoking Filter on Option Task with the sync predicate
        Then the result Option should be None

    Scenario: ValueTask Option Filter when Some and Predicate "true"
        Given Option is Some with value 30
        And Option is wrapped in a ValueTask
        And an async predicate that returns "true" for int value
        When invoking Filter on Option ValueTask with the async predicate
        Then the result Option should be Some with value 30

    Scenario: ValueTask Option Filter when Some and Predicate "false"
        Given Option is Some with value 40
        And Option is wrapped in a ValueTask
        And an async predicate that returns "false" for int value
        When invoking Filter on Option ValueTask with the async predicate
        Then the result Option should be None

    Scenario: ValueTask Option Filter when None
        Given Option is None
        And Option is wrapped in a ValueTask
        And an async predicate that returns "true" for int value
        When invoking Filter on Option ValueTask with the async predicate
        Then the result Option should be None

    Scenario: ValueTask Option Filter when Some and Sync Predicate "true"
        Given Option is Some with value 50
        And Option is wrapped in a ValueTask
        And a sync predicate that returns "true" for int value
        When invoking Filter on Option ValueTask with the sync predicate
        Then the result Option should be Some with value 50

    Scenario: ValueTask Option Filter when Some and Sync Predicate "false"
        Given Option is Some with value 60
        And Option is wrapped in a ValueTask
        And a sync predicate that returns "false" for int value
        When invoking Filter on Option ValueTask with the sync predicate
        Then the result Option should be None

    Scenario: ValueTask Option Filter when None and Sync Predicate
        Given Option is None
        And Option is wrapped in a ValueTask
        And a sync predicate that returns "true" for int value
        When invoking Filter on Option ValueTask with the sync predicate
        Then the result Option should be None
