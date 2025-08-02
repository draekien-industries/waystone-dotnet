Feature: Iter.Collect

    Scenario: Collecting an iterator into a list
        Given an "enumerable" of integers from 1 to 5
        When collecting "enumerable" of integers into a list
        Then the "enumerable" list of integers should contain
          | Value |
          | 1     |
          | 2     |
          | 3     |
          | 4     |
          | 5     |

    Scenario: Collecting an iterator of results into a list
        Given an "enumerable" of integer results
          | Type | Value |
          | Ok   | 1     |
          | Ok   | 2     |
        When collecting "enumerable" of integer results into a list
        Then the "enumerable" should contain a list of integers with values
          | Value |
          | 1     |
          | 2     |

    Scenario: Collecting an iterator of results with errors into a list
        Given an "enumerable" of integer results
          | Type | Value |
          | Ok   | 1     |
          | Err  | Error |
        When collecting "enumerable" of integer results into a list
        Then the "enumerable" should be an Err with message "Error"
