Feature: Map Extensions for Async Option

    Scenario: Map on Some Task Option
        Given Option is Some with value 10
        And Option is wrapped in a Task
        And async Map returns "mapped" + value
        When Option Task is invoked with "async" Map
        Then the result Option should be Some with value "mapped10"

    Scenario: Map on None Task Option
        Given Option is None
        And Option is wrapped in a Task
        And async Map returns "mapped" + value
        When Option Task is invoked with "async" Map
        Then the result Option should be None of "string"

    Scenario: Map on Some ValueTask Option
        Given Option is Some with value 20
        And Option is wrapped in a ValueTask
        And async Map returns "value" + value
        When Option ValueTask is invoked with "async" Map
        Then the result Option should be Some with value "value20"

    Scenario: Map on None ValueTask Option
        Given Option is None
        And Option is wrapped in a ValueTask
        And async Map returns "value" + value
        When Option ValueTask is invoked with "async" Map
        Then the result Option should be None of "string"

    Scenario: Map on Some Task Option with sync map
        Given Option is Some with value 30
        And Option is wrapped in a Task
        And sync Map returns "syncMapped" + value
        When Option Task is invoked with "sync" Map
        Then the result Option should be Some with value "syncMapped30"
