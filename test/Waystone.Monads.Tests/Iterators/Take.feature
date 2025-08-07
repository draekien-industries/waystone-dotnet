Feature: Iter.Take

    Scenario: Take a number of elements from an iterator
        Given an "enumerable" of integers from 1 to 5
        When converting "enumerable" of integers to an iterator
        And taking the first 3 elements from the "enumerable" integer iterator
        Then the "enumerable" integer Take should return
          | Value |
          | 1     |
          | 2     |
          | 3     |

        When getting the "next" element of the "enumerable" integer Take
        Then the result of the "next" integer find should be None
