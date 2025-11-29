Feature: Match Extensions on Async Result

    Scenario: Match on Task OK Result with async OK and async Error handlers
        Given an OK result with value 10
        And the result is wrapped in a Task
        And an "async" "Ok" handler that returns no value
        And an "async" "Error" handler that returns no value
        When invoking MatchAsync with the "async" OK handler and "async" Error handler on the result "Task"
        Then the "async" "Ok" handler should have been called with value 10
        And the "async" "Error" handler should not have been called

    Scenario: Match on Task OK Result with sync OK and async Error handlers
        Given an OK result with value 10
        And the result is wrapped in a Task
        And an "sync" "Ok" handler that returns no value
        And an "async" "Error" handler that returns no value
        When invoking MatchAsync with the "sync" OK handler and "async" Error handler on the result "Task"
        Then the "sync" "Ok" handler should have been called with value 10
        And the "async" "Error" handler should not have been called

    Scenario: Match on Task OK Result with async OK and sync Error handlers
        Given an OK result with value 10
        And the result is wrapped in a Task
        And an "async" "Ok" handler that returns no value
        And an "sync" "Error" handler that returns no value
        When invoking MatchAsync with the "async" OK handler and "sync" Error handler on the result "Task"
        Then the "async" "Ok" handler should have been called with value 10
        And the "sync" "Error" handler should not have been called

    Scenario: Match on Task OK Result with sync OK and sync Error handlers
        Given an OK result with value 10
        And the result is wrapped in a Task
        And an "sync" "Ok" handler that returns no value
        And an "sync" "Error" handler that returns no value
        When invoking MatchAsync with the "sync" OK handler and "sync" Error handler on the result "Task"
        Then the "sync" "Ok" handler should have been called with value 10
        And the "sync" "Error" handler should not have been called
