@option
Feature: IsSomeAnd Extensions for Async Option

    Scenario Outline: IsSomeAnd on a Some <receiver> Option returns the predicate result
        Given Option is Some with value <value>
        And Option is wrapped in a <receiver>
        And an async predicate that returns "<predicate>" for int value
        When invoking IsSomeAnd on Option <receiver> with the async predicate
        Then the boolean result should be "<predicate>"

        Examples:
            | receiver  | value | predicate |
            | Task      | 15    | true      |
            | Task      | 25    | false     |
            | ValueTask | 35    | true      |
            | ValueTask | 45    | false     |

    Scenario Outline: IsSomeAnd on a None <receiver> Option is false without calling the predicate
        Given Option is None
        And Option is wrapped in a <receiver>
        And an async predicate that returns "true" for int value
        When invoking IsSomeAnd on Option <receiver> with the async predicate
        Then the boolean result should be "false"

        Examples:
            | receiver  |
            | Task      |
            | ValueTask |
