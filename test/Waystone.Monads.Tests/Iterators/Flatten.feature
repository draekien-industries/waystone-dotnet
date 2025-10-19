Feature: Iter.Flatten

    Scenario: Flattening a 2D iter of integers
        Given a 2D iterator of integers "2d-iter"
        When the "2d-iter" 2D iterator is flattened into "1d-iter"
        Then the "1d-iter" result should be a 1D iterator containing the integers
