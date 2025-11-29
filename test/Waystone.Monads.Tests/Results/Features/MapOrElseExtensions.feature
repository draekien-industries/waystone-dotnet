Feature: MapOrElse Extensions for Result

    Scenario: MapOrElse on sync OK Result with async factory and async map
        Given an OK result with value 10
        And an async factory that returns "Missing"
        And an async map that converts the value into a string
        When invoking MapOrElse with the async factory and async map on the sync Result
        Then the output should be the value "10"

    Scenario: MapOrElse on async OK result with async factory and async map
        Given an OK result with value 10
        And the result is wrapped in a Task
        And an async factory that returns "Not Found"
        And an async map that converts the value into a string
        When invoking MapOrElse with the async factory and async map on the async Result
        Then the output should be the value "10"

    Scenario: MapOrElse on async OK Result with sync factory and async map
        Given an OK result with value 10
        And the result is wrapped in a Task
        And a sync factory that returns "Not Found"
        And an async map that converts the value into a string
        When invoking MapOrElse with the sync factory and async map on the async Result
        Then the output should be the value "10"

    Scenario: MapOrElse on async OK Result with async factory and sync map
        Given an OK result with value 10
        And the result is wrapped in a Task
        And an async factory that returns "Not Found"
        And a sync map that converts the value into a string
        When invoking MapOrElse with the async factory and sync map on the async Result
        Then the output should be the value "10"

    Scenario: MapOrElse on async OK Result with sync factory and sync map
        Given an OK result with value 10
        And the result is wrapped in a Task
        And a sync factory that returns "Not Found"
        And a sync map that converts the value into a string
        When invoking MapOrElse with the sync factory and sync map on the async Result
        Then the output should be the value "10"
