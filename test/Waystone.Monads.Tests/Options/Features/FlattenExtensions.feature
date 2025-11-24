Feature: Flatten Extensions for Async Option

    Scenario: Flattening an Async Option of Some value
        Given Option is Some with value 10
        And the Option is wrapped in an Option
        And the Option of Option is wrapped in a Task
        When invoking flatten on the Task of Option Option
        Then the result Option should be Some with value 10

    Scenario: Flattening an Async Option of None value
        Given Option is None
        And the Option is wrapped in an Option
        And the Option of Option is wrapped in a Task
        When invoking flatten on the Task of Option Option
        Then the result Option should be None

    Scenario: Flattening an Async Option of Some value (ValueTask)
        Given Option is Some with value 10
        And the Option is wrapped in an Option
        And the Option of Option is wrapped in a ValueTask
        When invoking flatten on the ValueTask of Option Option
        Then the result Option should be Some with value 10

    Scenario: Flattening an Async Option of None value (ValueTask)
        Given Option is None
        And the Option is wrapped in an Option
        And the Option of Option is wrapped in a ValueTask
        When invoking flatten on the ValueTask of Option Option
        Then the result Option should be None
