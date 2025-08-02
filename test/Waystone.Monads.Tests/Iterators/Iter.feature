Feature: Iter

    Scenario: Converting an enumerable to an iterator
        Given an "enumerable" of integers from 1 to 5
        When converting "enumerable" of integers to an iterator
        Then the "enumerable" iterator of integers should yield
          | Value |
          | 1     |
          | 2     |
          | 3     |
          | 4     |
          | 5     |
