Feature: Iter.Filter

    Scenario: Filter an Iter
        Given an "enumerable" of integers from 1 to 5
        When converting "enumerable" of integers to an iterator
        And invoking filter on the "enumerable" iterator of integers with a predicate that returns true for even numbers
        Then the "enumerable" integer Filter should return
          | Value |
          | 2     |
          | 4     |
