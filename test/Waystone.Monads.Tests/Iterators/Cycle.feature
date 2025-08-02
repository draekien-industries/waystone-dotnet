Feature: Iter.Cycle

    Scenario: Cycle numbers
        Given an iterator "enumerable" of a cloneable types
        When invoking cycle on "enumerable" of cloneable types
        Then the elements of "enumerable" should repeat indefinitely
