Feature: Iter.Compare

    Scenario: Comparing two iterators of the same values
        Given an "enumerable-a" of integers from 1 to 2
        And an "enumerable-b" of integers from 1 to 2
        When comparing "enumerable-a" and "enumerable-b" of integers
        Then the Ordering should be "Equal"

    Scenario: Comparing two iterators where the left is less than the right
        Given an "enumerable-a" of integers from 1 to 2
        And an "enumerable-b" of integers from 2 to 3
        When comparing "enumerable-a" and "enumerable-b" of integers
        Then the Ordering should be "Less"

    Scenario: Comparing two iterators where the left is greater than the right
        Given an "enumerable-a" of integers from 2 to 3
        And an "enumerable-b" of integers from 1 to 2
        When comparing "enumerable-a" and "enumerable-b" of integers
        Then the Ordering should be "Greater"

    Scenario: Comparing two iterators where the both are empty
        Given an empty "enumerable-a" of integers
        And an empty "enumerable-b" of integers
        When comparing "enumerable-a" and "enumerable-b" of integers
        Then the Ordering should be "Equal"

    Scenario: Comparing two iterators where the left is empty
        Given an empty "enumerable-a" of integers
        And an "enumerable-b" of integers from 1 to 2
        When comparing "enumerable-a" and "enumerable-b" of integers
        Then the Ordering should be "Less"

    Scenario: Comparing two iterators where the right is empty
        Given an "enumerable-a" of integers from 1 to 2
        And an empty "enumerable-b" of integers
        When comparing "enumerable-a" and "enumerable-b" of integers
        Then the Ordering should be "Greater"

    Scenario: Comparing two iterators where the left is a prefix of the right
        Given an "enumerable-a" of integers from 1 to 1
        And an "enumerable-b" of integers from 1 to 2
        When comparing "enumerable-a" and "enumerable-b" of integers
        Then the Ordering should be "Less"
