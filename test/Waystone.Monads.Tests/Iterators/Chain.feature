Feature: Iter.Chain

    Scenario: Chaining two iterators
        Given an "enumerable-a" of integers from 1 to 2
        And an "enumerable-b" of integers from 3 to 4
        When chaining "enumerable-a" and "enumerable-b" of integers into "enumerable"
        Then the "enumerable" iterator of integers should yield
          | Value |
          | 1     |
          | 2     |
          | 3     |
          | 4     |
