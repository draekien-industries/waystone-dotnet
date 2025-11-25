Feature: OkOrElse Extensions for Async Option

    Scenario: OkOrElse with Some value
        Given Option is Some with value 10
        And an async Error delegate that returns "Error occurred"
        When invoking OkOrElse on the Option with the async Error delegate
        Then the result should be an Ok Result containing the value 10

    Scenario: OkOrElse with None value
        Given Option is None
        And an async Error delegate that returns "Error occurred"
        When invoking OkOrElse on the Option with the async Error delegate
        Then the result should be an Error Result containing "Error occurred"

    Scenario: OkOrElse with Task Some value
        Given Option is Some with value 20
        And Option is wrapped in a Task
        And an async Error delegate that returns "Task Error occurred"
        When invoking OkOrElse on the Option Task with the async Error delegate
        Then the result should be an Ok Result containing the value 20

    Scenario: OkOrElse with Task None value
        Given Option is None
        And Option is wrapped in a Task
        And an async Error delegate that returns "Task Error occurred"
        When invoking OkOrElse on the Option Task with the async Error delegate
        Then the result should be an Error Result containing "Task Error occurred"

    Scenario: OkOrElse with ValueTask Some value
        Given Option is Some with value 30
        And Option is wrapped in a ValueTask
        And an async Error delegate that returns "ValueTask Error occurred"
        When invoking OkOrElse on the Option ValueTask with the async Error delegate
        Then the result should be an Ok Result containing the value 30

    Scenario: OkOrElse with ValueTask None value
        Given Option is None
        And Option is wrapped in a ValueTask
        And an async Error delegate that returns "ValueTask Error occurred"
        When invoking OkOrElse on the Option ValueTask with the async Error delegate
        Then the result should be an Error Result containing "ValueTask Error occurred"

    Scenario: OkOrElse with Task Some and synchronous Error delegate
        Given Option is Some with value 10
        And Option is wrapped in a Task
        And a synchronous Error delegate that returns "Synchronous Error occurred"
        When invoking OkOrElse on the Task Option with the synchronous Error delegate
        Then the result should be an Ok Result containing the value 10

    Scenario: OkOrElse with Task None and synchronous Error delegate
        Given Option is None
        And Option is wrapped in a Task
        And a synchronous Error delegate that returns "Synchronous Error occurred"
        When invoking OkOrElse on the Task Option with the synchronous Error delegate
        Then the result should be an Error Result containing "Synchronous Error occurred"

    Scenario: OkOrElse with ValueTask Some and synchronous Error delegate
        Given Option is Some with value 20
        And Option is wrapped in a ValueTask
        And a synchronous Error delegate that returns "Synchronous ValueTask Error occurred"
        When invoking OkOrElse on the ValueTask Option with the synchronous Error delegate
        Then the result should be an Ok Result containing the value 20

    Scenario: OkOrElse with ValueTask None and synchronous Error delegate
        Given Option is None
        And Option is wrapped in a ValueTask
        And a synchronous Error delegate that returns "Synchronous ValueTask Error occurred"
        When invoking OkOrElse on the ValueTask Option with the synchronous Error delegate
        Then the result should be an Error Result containing "Synchronous ValueTask Error occurred"
