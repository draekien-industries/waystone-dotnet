Feature: Inspect Iter

    Scenario: Inspecting elements of an iterator
        Given an "enumerable" of integers from 1 to 5
        When converting "enumerable" of integers to an iterator
        And inspecting each element of "enumerable" iterator of integers to log "Inspected: {0}"
        Then the log should contain
          | Message      |
          | Inspected: 1 |
          | Inspected: 2 |
          | Inspected: 3 |
          | Inspected: 4 |
          | Inspected: 5 |
        And the "enumerable" iterator of integers should yield
          | Value |
          | 1     |
          | 2     |
          | 3     |
          | 4     |
          | 5     |
