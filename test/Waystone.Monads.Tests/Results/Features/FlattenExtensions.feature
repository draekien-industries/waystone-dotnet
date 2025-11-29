Feature: Flatten Extensions on Async Result

    Scenario: sync flatten Ok of Ok result
        Given an OK result with value 10
        And it is nested in an OK result
        When invoking flatten on the sync nested results
        Then the output should be an OK result with value 10

    Scenario: sync flatten Ok of Error result
        Given an OK result with value 10
        And it is nested in an Error result with value "Error"
        When invoking flatten on the sync nested results
        Then the output should be an Error result with message "Error"

    Scenario: sync flatten Error of Ok result
        Given an Error result with value "Error"
        And it is nested in an OK result
        When invoking flatten on the sync nested results
        Then the output should be an Error result with message "Error"

    Scenario: sync flatten Error of Error result
        Given an Error result with value "Error1"
        And it is nested in an Error result with value "Error2"
        When invoking flatten on the sync nested results
        Then the output should be an Error result with message "Error2"

    Scenario: async flatten Ok of Ok result Task
        Given an OK result with value 20
        And it is nested in an OK result
        And the outer result is wrapped in a Task
        When invoking flatten on the async Task result
        Then the output should be an OK result with value 20

    Scenario: async flatten Ok of Error result Task
        Given an OK result with value 20
        And it is nested in an Error result with value "Async Error"
        And the outer result is wrapped in a Task
        When invoking flatten on the async Task result
        Then the output should be an Error result with message "Async Error"

    Scenario: async flatten Error of Ok result Task
        Given an Error result with value "Async Error"
        And it is nested in an OK result
        And the outer result is wrapped in a Task
        When invoking flatten on the async Task result
        Then the output should be an Error result with message "Async Error"

    Scenario: async flatten Error of Error result Task
        Given an Error result with value "Async Error1"
        And it is nested in an Error result with value "Async Error2"
        And the outer result is wrapped in a Task
        When invoking flatten on the async Task result
        Then the output should be an Error result with message "Async Error2"
