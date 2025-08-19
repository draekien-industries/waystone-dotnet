Feature: Iter.ForEach

    Scenario: ForEach with a simple action
        Given an iterator of numbers from 1 to 5
        When I apply a mock action to each number
        Then the action should be called 5 times
