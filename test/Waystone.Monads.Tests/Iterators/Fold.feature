Feature: Iter.Fold

    Scenario: Fold over an array
        Given an "enumerable" of integers from 1 to 5
        When converting "enumerable" of integers to an iterator
        And folding the "enumerable" integer iterator with an add operation
        Then the result of the folded "enumerable" should be 15
