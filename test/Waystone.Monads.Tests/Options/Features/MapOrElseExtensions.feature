Feature: MapOrElse Extensions for Async Option

    Scenario: MapOrElse on Some Task Option
        Given Option is Some with value 10
        And Option is wrapped in a Task
        And async Else returns "fallback"
        And async Map returns "mapped" + value
        When MapOrElse Task is invoked with "async" Else and "async" Map
        Then the result should be "mapped10"

    Scenario: MapOrElse on None Task Option
        Given Option is None
        And Option is wrapped in a Task
        And async Else returns "fallback"
        And async Map returns "mapped" + value
        When MapOrElse Task is invoked with "async" Else and "async" Map
        Then the result should be "fallback"

    Scenario: MapOrElse on Some ValueTask Option
        Given Option is Some with value 20
        And Option is wrapped in a ValueTask
        And async Else returns "default"
        And async Map returns "value" + value
        When MapOrElse ValueTask is invoked with "async" Else and "async" Map
        Then the result should be "value20"

    Scenario: MapOrElse on None ValueTask Option
        Given Option is None
        And Option is wrapped in a ValueTask
        And async Else returns "default"
        And async Map returns "value" + value
        When MapOrElse ValueTask is invoked with "async" Else and "async" Map
        Then the result should be "default"

    Scenario: MapOrElse on Some Task Option with sync else and async map
        Given Option is Some with value 30
        And Option is wrapped in a Task
        And sync Else returns "syncFallback"
        And async Map returns "asyncMapped" + value
        When MapOrElse Task is invoked with "sync" Else and "async" Map
        Then the result should be "asyncMapped30"

    Scenario: MapOrElse on Some Task Option with async else and sync map
        Given Option is Some with value 40
        And Option is wrapped in a Task
        And async Else returns "asyncFallback"
        And sync Map returns "syncMapped" + value
        When MapOrElse Task is invoked with "async" Else and "sync" Map
        Then the result should be "syncMapped40"

    Scenario: MapOrElse on SOme Task Option with sync else and sync map
        Given Option is Some with value 50
        And Option is wrapped in a Task
        And sync Else returns "syncFallback"
        And sync Map returns "syncMapped" + value
        When MapOrElse Task is invoked with "sync" Else and "sync" Map
        Then the result should be "syncMapped50"
