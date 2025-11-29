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

    Scenario: MapOrElse on sync Error Result with async factory and async map
        Given an Error result with value "Error occurred"
        And an async factory that returns "Error handled"
        And an async map that converts the value into a string
        When invoking MapOrElse with the async factory and async map on the sync Result
        Then the output should be the value "Error handled"

    Scenario: MapOrElse on async Error Result with async factory and async map
        Given an Error result with value "Error occurred"
        And the result is wrapped in a Task
        And an async factory that returns "Error handled"
        And an async map that converts the value into a string
        When invoking MapOrElse with the async factory and async map on the async Result
        Then the output should be the value "Error handled"

    Scenario: MapOrElse on async Error Result with sync factory and async map
        Given an Error result with value "Error occurred"
        And the result is wrapped in a Task
        And a sync factory that returns "Error handled"
        And an async map that converts the value into a string
        When invoking MapOrElse with the sync factory and async map on the async Result
        Then the output should be the value "Error handled"

    Scenario: MapOrElse on async Error Result with async factory and sync map
        Given an Error result with value "Error occurred"
        And the result is wrapped in a Task
        And an async factory that returns "Error handled"
        And a sync map that converts the value into a string
        When invoking MapOrElse with the async factory and sync map on the async Result
        Then the output should be the value "Error handled"

    Scenario: MapOrElse on async Error Result with sync factory and sync map
        Given an Error result with value "Error occurred"
        And the result is wrapped in a Task
        And a sync factory that returns "Error handled"
        And a sync map that converts the value into a string
        When invoking MapOrElse with the sync factory and sync map on the async Result
        Then the output should be the value "Error handled"

    Scenario: MapOrElse on ValueTask OK Result with async factory and async map
        Given an OK result with value 20
        And the result is wrapped in a ValueTask
        And an async factory that returns "Not Available"
        And an async map that converts the value into a string
        When invoking MapOrElse with the async factory and async map on the ValueTask Result
        Then the output should be the value "20"

    Scenario: MapOrElse on ValueTask Error Result with async factory and async map
        Given an Error result with value "Critical Error"
        And the result is wrapped in a ValueTask
        And an async factory that returns "Recovered"
        And an async map that converts the value into a string
        When invoking MapOrElse with the async factory and async map on the ValueTask Result
        Then the output should be the value "Recovered"

    Scenario: MapOrElse on ValueTask OK Result with sync factory and sync map
        Given an OK result with value 30
        And the result is wrapped in a ValueTask
        And a sync factory that returns "Unavailable"
        And a sync map that converts the value into a string
        When invoking MapOrElse with the sync factory and sync map on the ValueTask Result
        Then the output should be the value "30"

    Scenario: MapOrElse on ValueTask Error Result with sync factory and sync map
        Given an Error result with value "Fatal Error"
        And the result is wrapped in a ValueTask
        And a sync factory that returns "Resolved"
        And a sync map that converts the value into a string
        When invoking MapOrElse with the sync factory and sync map on the ValueTask Result
        Then the output should be the value "Resolved"

    Scenario: MapOrElse on ValueTask OK Result with async factory and sync map
        Given an OK result with value 40
        And the result is wrapped in a ValueTask
        And an async factory that returns "Not Present"
        And a sync map that converts the value into a string
        When invoking MapOrElse with the async factory and sync map on the ValueTask Result
        Then the output should be the value "40"

    Scenario: MapOrElse on ValueTask Error Result with async factory and sync map
        Given an Error result with value "Severe Error"
        And the result is wrapped in a ValueTask
        And an async factory that returns "Fixed"
        And a sync map that converts the value into a string
        When invoking MapOrElse with the async factory and sync map on the ValueTask Result
        Then the output should be the value "Fixed"

    Scenario: MapOrElse on ValueTask OK Result with sync factory and async map
        Given an OK result with value 50
        And the result is wrapped in a ValueTask
        And a sync factory that returns "Not Here"
        And an async map that converts the value into a string
        When invoking MapOrElse with the sync factory and async map on the ValueTask Result
        Then the output should be the value "50"

    Scenario: MapOrElse on ValueTask Error Result with sync factory and async map
        Given an Error result with value "Major Error"
        And the result is wrapped in a ValueTask
        And a sync factory that returns "Handled"
        And an async map that converts the value into a string
        When invoking MapOrElse with the sync factory and async map on the ValueTask Result
        Then the output should be the value "Handled"
