Feature: Iter.Find

    Scenario: Find an element in a sequence
        Given an "enumerable" of integers from 1 to 5
        When converting "enumerable" of integers to an iterator
        And finding the first element of "enumerable" integer iterator that is greater than 3
        Then the result of the "enumerable" integer find should be 4
