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
