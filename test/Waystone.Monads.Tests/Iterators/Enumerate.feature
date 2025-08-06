Feature: Iter.Enumerate

    Scenario: Enumerate an Iter
        Given an "enumerable" of integers from 1 to 5
        When converting "enumerable" of integers to an iterator
        And invoking enumerate on the "enumerable" iterator of integers
        Then the "enumerable" Enumerable should return
          | Index | Value |
          | 0     | 1     |
          | 1     | 2     |
          | 2     | 3     |
          | 3     | 4     |
          | 4     | 5     |
