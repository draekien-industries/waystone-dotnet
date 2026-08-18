@option
Feature: AndThen Extensions for Async Option

    Scenario: Async AndThen on Option with Some Value
        Given Option is Some with value 10
        And an async map function that returns a Some with value multiplied by 2
        When invoking async AndThen on Option
        Then the result Option should be Some with value 20

    Scenario: Async AndThen on Option with None Value
        Given Option is None
        And an async map function that returns a None
        When invoking async AndThen on Option
        Then the result Option should be None

    Scenario: Async AndThen on Option Task with Some value
        Given Option is Some with value 10
        And Option is wrapped in a Task
        And an async map function that returns a Some with value multiplied by 2
        When invoking async AndThen on Option Task
        Then the result Option should be Some with value 20

    Scenario: Async AndThen on Option Task with None value
        Given Option is None
        And Option is wrapped in a Task
        And an async map function that returns a None
        When invoking async AndThen on Option Task
        Then the result Option should be None

    Scenario: Sync AndThen on Option Task with Some Value
        Given Option is Some with value 10
        And Option is wrapped in a Task
        And a sync map function that returns a Some with value multiplied by 2
        When invoking sync AndThen on Option Task
        Then the result Option should be Some with value 20

    Scenario: Sync AndThen on Option Task with None Value
        Given Option is Some with value 10
        And Option is wrapped in a Task
        And a sync map function that returns a None
        When invoking sync AndThen on Option Task
        Then the result Option should be None

    Scenario: Async AndThen on Option ValueTask with Some value
        Given Option is Some with value 10
        And Option is wrapped in a ValueTask
        And an async map function that returns a Some with value multiplied by 2
        When invoking async AndThen on Option ValueTask
        Then the result Option should be Some with value 20

    Scenario: Async AndThen on Option ValueTask with None value
        Given Option is None
        And Option is wrapped in a ValueTask
        And an async map function that returns a None
        When invoking async AndThen on Option ValueTask
        Then the result Option should be None

    Scenario: Sync AndThen on Option ValueTask with Some Value
        Given Option is Some with value 10
        And Option is wrapped in a ValueTask
        And a sync map function that returns a Some with value multiplied by 2
        When invoking sync AndThen on Option ValueTask
        Then the result Option should be Some with value 20

    Scenario: Sync AndThen on Option ValueTask with None Value
        Given Option is None
        And Option is wrapped in a ValueTask
        And a sync map function that returns a None
        When invoking sync AndThen on Option ValueTask
        Then the result Option should be None
