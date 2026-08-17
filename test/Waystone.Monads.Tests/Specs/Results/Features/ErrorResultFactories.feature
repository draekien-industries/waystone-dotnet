@result
Feature: Result and Error factories that default the error type to Error

    Scenario: Creating an Ok result without specifying the error type
        When creating an Ok result with the value 10
        Then the error typed result should be Ok with the value 10

    Scenario: Creating an Err result from an Error without specifying the error type
        Given an Error with code "Explicit.Code" and message "something went wrong"
        When creating an Err result from the Error
        Then the error typed result should be Err with code "Explicit.Code" and message "something went wrong"

    Scenario: Creating an Err result from an enum and a message
        When creating an Err result from the NotFound enum value and message "the user was not found"
        Then the error typed result should be Err with code "TestErrorCodes.NotFound" and message "the user was not found"

    Scenario: Creating an Error from an enum and a message
        When creating an Error from the NotFound enum value and message "the user was not found"
        Then the Error should have code "TestErrorCodes.NotFound" and message "the user was not found"

    Scenario: Creating an Error from an enum without a message
        When creating an Error from the NotFound enum value and message ""
        Then the Error should have code "TestErrorCodes.NotFound" and message "An unexpected error occurred."

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
