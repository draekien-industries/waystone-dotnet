Feature: Iter.FlatMap

    Scenario: FlatMap a sequence
        Given an "enumerable" of words
          | Value |
          | One   |
          | Two   |
          | Three |
        When converting "enumerable" of words to an iterator
        And invoking FlatMap on the "enumerable" of words to extract characters
        Then the "enumerable" of characters FlatMap should return
          | Value |
          | O     |
          | n     |
          | e     |
          | T     |
          | w     |
          | o     |
          | T     |
          | h     |
          | r     |
          | e     |
          | e     |
