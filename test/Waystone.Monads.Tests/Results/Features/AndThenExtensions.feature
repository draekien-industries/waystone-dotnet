Feature: AndThen Extensions for Async Result

    Scenario: Async delegate returns OK result
        Given an OK result with value 10
        And the result is wrapped in a Task
        And an async delegate that returns an OK result with value 20
        When invoking AndThenAsync with the async delegate that returns "OK"
        Then the output should be an OK result with value 20

    Scenario: Async delegate returns Error result
        Given an OK result with value 10
        And the result is wrapped in a Task
        And an async delegate that returns an Error result with message "Error occurred"
        When invoking AndThenAsync with the async delegate that returns "Error"
        Then the output should be an Error result with message "Error occurred"

    Scenario: Sync delegate returns OK result
        Given an OK result with value 10
        And the result is wrapped in a Task
        And a sync delegate that returns an OK result with value 30
        When invoking AndThenAsync with the sync delegate that returns "OK"
        Then the output should be an OK result with value 30

    Scenario: Sync delegate returns Error result
        Given an OK result with value 10
        And the result is wrapped in a Task
        And a sync delegate that returns an Error result with message "Sync error"
        When invoking AndThenAsync with the sync delegate that returns "Error"
        Then the output should be an Error result with message "Sync error"
