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

    Scenario: Cloning an iterator
        Given an iterator "iter" of a cloneable types
        When cloning "iter" into "iter-clone"
        Then the cloned iterator "iter-clone" should yield the cloned values of "iter"

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
