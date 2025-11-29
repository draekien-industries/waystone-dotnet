Feature: AndThen Extensions for Async Result

    Scenario: Chaining successful async results
        Given an OK result with value 10
        And the result is wrapped in a Task
        And an async delegate that returns an OK result with value 20
        When invoking AndThenAsync with the async delegate
        Then the output should be an OK result with value 20
