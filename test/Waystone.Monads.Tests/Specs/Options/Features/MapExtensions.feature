@option
Feature: Map Extensions for Async Option

    Scenario Outline: Map on a Some <receiver> Option applies the <variant> map
        Given Option is Some with value <value>
        And Option is wrapped in a <receiver>
        And <variant> Map returns "<prefix>" + value
        When Option <receiver> is invoked with "<variant>" Map
        Then the result Option should be Some with value "<expected>"

        Examples:
            | receiver  | value | variant | prefix     | expected     |
            | Task      | 10    | async   | mapped     | mapped10     |
            | ValueTask | 20    | async   | value      | value20      |
            | Task      | 30    | sync    | syncMapped | syncMapped30 |

    Scenario Outline: Map on a None <receiver> Option stays None without applying the map
        Given Option is None
        And Option is wrapped in a <receiver>
        And async Map returns "mapped" + value
        When Option <receiver> is invoked with "async" Map
        Then the result Option should be None of "string"

        Examples:
            | receiver  |
            | Task      |
            | ValueTask |
