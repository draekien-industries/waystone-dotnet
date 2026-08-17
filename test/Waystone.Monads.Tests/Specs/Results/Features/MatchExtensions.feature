@result
Feature: Match Extensions for Async Result

    Scenario Outline: MatchAsync on an Ok <receiver> result calls the Ok handler
        Given an OK result with value 10
        And the result is wrapped in a <receiver>
        And an "<okHandler>" "Ok" handler that returns no value
        And an "<errHandler>" "Error" handler that returns no value
        When invoking MatchAsync with the "<okHandler>" OK handler and "<errHandler>" Error handler on the result "<receiver>"
        Then the "<okHandler>" "Ok" handler should have been called with value 10
        And the "<errHandler>" "Error" handler should not have been called

        Examples:
            | receiver  | okHandler | errHandler |
            | Task      | async     | async      |
            | Task      | async     | sync       |
            | Task      | sync      | async      |
            | Task      | sync      | sync       |
            | ValueTask | async     | async      |
            | ValueTask | async     | sync       |
            | ValueTask | sync      | async      |
            | ValueTask | sync      | sync       |

    Scenario Outline: MatchAsync on an Error <receiver> result calls the Error handler
        Given an Error result with value "Error"
        And the result is wrapped in a <receiver>
        And an "<okHandler>" "Ok" handler that returns no value
        And an "<errHandler>" "Error" handler that returns no value
        When invoking MatchAsync with the "<okHandler>" OK handler and "<errHandler>" Error handler on the result "<receiver>"
        Then the "<errHandler>" "Error" handler should have been called with value "Error"
        And the "<okHandler>" "Ok" handler should not have been called

        Examples:
            | receiver  | okHandler | errHandler |
            | Task      | async     | async      |
            | Task      | async     | sync       |
            | Task      | sync      | async      |
            | Task      | sync      | sync       |
            | ValueTask | async     | async      |
            | ValueTask | async     | sync       |
            | ValueTask | sync      | async      |
            | ValueTask | sync      | sync       |
