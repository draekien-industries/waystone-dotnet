Feature: MapOrElse Extensions for Result

    Scenario: MapOrElse on sync OK Result with async factory and async map
        Given an OK result with value 10
        And an async factory that returns "Missing"
        And an async map that converts the value into a string
        When invoking MapOrElse with the async factory and async map on the sync Result
        Then the output should be the value "10"
