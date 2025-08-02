namespace Waystone.Monads.Iterators;

/// <summary>Represents the ordering of two elements in a sequence.</summary>
public enum Ordering
{
    /// <summary>Indicates that the first element is less than the second element.</summary>
    Less = -1,

    /// <summary>Indicates that the first element is equal to the second element.</summary>
    Equal = 0,

    /// <summary>Indicates that the first element is greater than the second element.</summary>
    Greater = 1,
}
