Feature: MapOr Extensions for Async Option

    Scenario: MapOr on Some Task Option
        Given Option is Some with value 10
        And Option is wrapped in a Task
        And async MapOr returns "mapped" + value
        When Option Task is invoked with "async" MapOr and default "defaultValue"
        Then the result should be "mapped10"

    Scenario: MapOr on None Task Option
        Given Option is None
        And Option is wrapped in a Task
        And async MapOr returns "mapped" + value
        When Option Task is invoked with "async" MapOr and default "defaultValue"
        Then the result should be "defaultValue"

    Scenario: MapOr on Some ValueTask Option
        Given Option is Some with value 20
        And Option is wrapped in a ValueTask
        And async MapOr returns "value" + value
        When Option ValueTask is invoked with "async" MapOr and default "defaultValue"
        Then the result should be "value20"

    Scenario: MapOr on None ValueTask Option
        Given Option is None
        And Option is wrapped in a ValueTask
        And async MapOr returns "value" + value
        When Option ValueTask is invoked with "async" MapOr and default "defaultValue"
        Then the result should be "defaultValue"

    Scenario: MapOr on Some Task Option with sync map
        Given Option is Some with value 30
        And Option is wrapped in a Task
        And sync MapOr returns "syncMapped" + value
        When Option Task is invoked with "sync" MapOr and default "defaultValue"
        Then the result should be "syncMapped30"
