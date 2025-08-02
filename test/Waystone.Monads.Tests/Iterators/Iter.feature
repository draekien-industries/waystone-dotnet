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
