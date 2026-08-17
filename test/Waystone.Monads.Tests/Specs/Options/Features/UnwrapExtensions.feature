@option
Feature: Unwrap Extensions for Async Option

    Scenario Outline: Unwrap on a Some <receiver> Option returns the value
        Given Option is Some with value <value>
        And Option is wrapped in a <receiver>
        When unwrapping the <receiver> Option
        Then the unwrapped Option value should be <value>

        Examples:
            | receiver  | value |
            | Task      | 10    |
            | ValueTask | 20    |

    Scenario Outline: Unwrap on a None <receiver> Option throws
        Given Option is None
        And Option is wrapped in a <receiver>
        When unwrapping the <receiver> Option
        Then an Option UnwrapException should be thrown

        Examples:
            | receiver  |
            | Task      |
            | ValueTask |

    Scenario Outline: UnwrapOr on a Some <receiver> Option ignores the default
        Given Option is Some with value <value>
        And Option is wrapped in a <receiver>
        When unwrapping the <receiver> Option with a default of 99
        Then the unwrapped Option value should be <value>

        Examples:
            | receiver  | value |
            | Task      | 10    |
            | ValueTask | 30    |

    Scenario Outline: UnwrapOr on a None <receiver> Option returns the default
        Given Option is None
        And Option is wrapped in a <receiver>
        When unwrapping the <receiver> Option with a default of 99
        Then the unwrapped Option value should be 99

        Examples:
            | receiver  |
            | Task      |
            | ValueTask |

    Scenario Outline: UnwrapOrDefault on a Some <receiver> Option returns the value
        Given Option is Some with value <value>
        And Option is wrapped in a <receiver>
        When unwrapping the <receiver> Option or its default
        Then the unwrapped Option value should be <value>

        Examples:
            | receiver  | value |
            | Task      | 10    |
            | ValueTask | 40    |

    Scenario Outline: UnwrapOrDefault on a None <receiver> Option returns the type default
        Given Option is None
        And Option is wrapped in a <receiver>
        When unwrapping the <receiver> Option or its default
        Then the unwrapped Option value should be 0

        Examples:
            | receiver  |
            | Task      |
            | ValueTask |
