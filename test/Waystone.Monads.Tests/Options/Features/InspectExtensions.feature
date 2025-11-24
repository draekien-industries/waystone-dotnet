Feature: Inspect Extensions for Async Option

    Scenario: InspectAsync with Some value
        Given Option is Some with value 10
        And Option is wrapped in a Task
        And an async delegate
        When invoking InspectAsync on the Option Task with the async delegate
        Then the async delegate should be invoked with value 10

    Scenario: InspectAsync with Some value and synchronous delegate
        Given Option is Some with value 20
        And Option is wrapped in a Task
        And a synchronous delegate
        When invoking InspectAsync on the Option Task with the synchronous delegate
        Then the synchronous delegate should be invoked with value 20

    Scenario: InspectAsync with None value
        Given Option is None
        And Option is wrapped in a Task
        And an async delegate
        When invoking InspectAsync on the Option Task with the async delegate
        Then the async delegate should not be invoked

    Scenario: InspectAsync with None value and synchronous delegate
        Given Option is None
        And Option is wrapped in a Task
        And a synchronous delegate
        When invoking InspectAsync on the Option Task with the synchronous delegate
        Then the synchronous delegate should not be invoked

    Scenario: InspectAsync with Some value (ValueTask)
        Given Option is Some with value 30
        And Option is wrapped in a ValueTask
        And an async delegate
        When invoking InspectAsync on the Option ValueTask with the async delegate
        Then the async delegate should be invoked with value 30

    Scenario: InspectAsync with None value (ValueTask)
        Given Option is None
        And Option is wrapped in a ValueTask
        And an async delegate
        When invoking InspectAsync on the Option ValueTask with the async delegate
        Then the async delegate should not be invoked

    Scenario: InspectAsync with Some value and synchronous delegate (ValueTask)
        Given Option is Some with value 40
        And Option is wrapped in a ValueTask
        And a synchronous delegate
        When invoking InspectAsync on the Option ValueTask with the synchronous delegate
        Then the synchronous delegate should be invoked with value 40

    Scenario: InspectAsync with None value and synchronous delegate (ValueTask)
        Given Option is None
        And Option is wrapped in a ValueTask
        And a synchronous delegate
        When invoking InspectAsync on the Option ValueTask with the synchronous delegate
        Then the synchronous delegate should not be invoked
