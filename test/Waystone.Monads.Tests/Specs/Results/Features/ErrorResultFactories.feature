@result
Feature: Result and Error factories that default the error type to Error

    Scenario: Creating an Ok result without specifying the error type
        When creating an Ok result with the value 10
        Then the error typed result should be Ok with the value 10

    Scenario: Creating an Err result from an Error without specifying the error type
        Given an Error with code "Explicit.Code" and message "something went wrong"
        When creating an Err result from the Error
        Then the error typed result should be Err with code "Explicit.Code" and message "something went wrong"

    Scenario: Trying a factory that succeeds
        When trying a factory that returns 10
        Then the error typed result should be Ok with the value 10

    Scenario: Trying a factory that throws
        When trying a factory that throws an InvalidOperationException with message "factory failed"
        Then the error typed result should be Err with code "InvalidOperation" and message "factory failed"

    Scenario: Trying an async factory that succeeds
        When trying an async factory that returns 20
        Then the error typed result should be Ok with the value 20

    Scenario: Trying an async factory that throws
        When trying an async factory that throws an InvalidOperationException with message "async factory failed"
        Then the error typed result should be Err with code "InvalidOperation" and message "async factory failed"
