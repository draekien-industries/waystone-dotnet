Feature: Iter.Fuse

    Scenario: Fused iterator returns none forever
        Given an iterator of numbers from 1 to 2
        When the number iterator is fused
        And next is invoked on number iterator 4 times
        Then the first 2 results should be some
        And the next 2 results should be none
