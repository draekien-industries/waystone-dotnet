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
        And the size hint of the "enumerable" iterator should have a lower bound of 5
        And the size hint of the "enumerable" iterator should have an upper bound of 5

    Scenario: Size hint of an iterator
        Given an "enumerable" of integers from 1 to 5
        When converting "enumerable" of integers to an iterator
        Then the size hint of the "enumerable" iterator should have a lower bound of 5
        And the size hint of the "enumerable" iterator should have an upper bound of 5

        When invoking Next on "enumerable" iterator
        Then the size hint of the "enumerable" iterator should have a lower bound of 4
        And the size hint of the "enumerable" iterator should have an upper bound of 4

        When invoking Next on "enumerable" iterator
        Then the size hint of the "enumerable" iterator should have a lower bound of 3
        And the size hint of the "enumerable" iterator should have an upper bound of 3

        When invoking Next on "enumerable" iterator
        Then the size hint of the "enumerable" iterator should have a lower bound of 2
        And the size hint of the "enumerable" iterator should have an upper bound of 2

        When invoking Next on "enumerable" iterator
        Then the size hint of the "enumerable" iterator should have a lower bound of 1
        And the size hint of the "enumerable" iterator should have an upper bound of 1

        When invoking Next on "enumerable" iterator
        Then the size hint of the "enumerable" iterator should have a lower bound of 0
        And the size hint of the "enumerable" iterator should have an upper bound of None

    Scenario: Equality of iterators
        Given an "enumerable-a" of integers from 1 to 5
        And an "enumerable-b" of integers from 1 to 5
        When converting "enumerable-a" of integers to an iterator
        And converting "enumerable-b" of integers to an iterator
        Then the "enumerable-a" iterator of integers should be equal to "enumerable-b" iterator of integers

    Scenario: Equality of enumerables
        Given an "enumerable-a" of integers from 1 to 5
        And an "enumerable-b" of integers from 1 to 5
        Then the "enumerable-a" enumerable of integers should be equal to "enumerable-b" enumerable of integers

    Scenario: Inequality of iterators
        Given an "enumerable-a" of integers from 1 to 5
        And an "enumerable-b" of integers from 6 to 10
        When converting "enumerable-a" of integers to an iterator
        And converting "enumerable-b" of integers to an iterator
        Then the "enumerable-a" iterator of integers should not be equal to "enumerable-b" iterator of integers

        Given an "enumerable-a" of chars from "a" to "e"
        And an "enumerable-b" of integers from 1 to 5
        When converting "enumerable-a" of chars to an iterator
        And converting "enumerable-b" of integers to an iterator
        Then the "enumerable-a" iterator of chars should not be equal to "enumerable-b" iterator of integers
