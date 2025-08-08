Feature: Iter.Flatten

    Scenario: Flattening a 2D array of integers
        Given a 2D array of integers "2d-array"
        And the "2d-array" 2D array is converted into a 2D iterator
        When the "2d-array" 2D iterator is flattened into "1d-array"
        Then the "1d-array" result should be a 1D iterator containing the integers
