Feature: Iter Map

    Scenario: Map over a list of integers
        Given an "enumerable" of integers from 1 to 5
        When converting "enumerable" of integers to an iterator
        And mapping "enumerable" of integers into strings
        Then the elements of "enumerable" Map should be the string values
          | Value |
          | 1     |
          | 2     |
          | 3     |
          | 4     |
          | 5     |
