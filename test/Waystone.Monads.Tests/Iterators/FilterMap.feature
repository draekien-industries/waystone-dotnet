Feature: Iter.FilterMap

    Scenario: FilterMap over a list of integers
        Given an "enumerable" of integers from 1 to 5
        When converting "enumerable" of integers to an iterator
        And filtering "enumerable" of integers to only even numbers and mapping them into strings
        Then the elements of "enumerable" FilterMap should be the string values
          | Value |
          | 2     |
          | 4     |
