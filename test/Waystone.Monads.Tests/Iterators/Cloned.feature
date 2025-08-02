Feature: Iter.Cloned

    Scenario: Cloning an iterator
        Given an iterator "iter" of a cloneable types
        When cloning "iter" into "iter-clone"
        Then the cloned iterator "iter-clone" should yield the cloned values of "iter"
