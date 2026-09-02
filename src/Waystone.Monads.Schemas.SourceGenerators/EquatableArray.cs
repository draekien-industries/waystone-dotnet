namespace Waystone.Monads.Schemas.SourceGenerators;

using System;

/// <summary>
/// An array that compares by its contents, so a record holding one can be cached
/// by the incremental pipeline.
/// </summary>
/// <remarks>
/// A record's generated equality calls <c>Equals</c> on each member, and an array
/// compares by reference. A result holding a bare array is therefore unequal to
/// an identical one built on the next keystroke, every cached step downstream of
/// it re-runs, and nothing reports that it happened.
/// </remarks>
internal readonly struct EquatableArray<T>(T[] values)
    : IEquatable<EquatableArray<T>>
    where T : IEquatable<T>
{
    public T[] Values { get; } = values;

    public int Length => Values.Length;

    public bool Equals(EquatableArray<T> other)
    {
        if (Values.Length != other.Values.Length) return false;

        for (var index = 0; index < Values.Length; index++)
        {
            if (!Values[index].Equals(other.Values[index])) return false;
        }

        return true;
    }

    public override bool Equals(object? obj) =>
        obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        int hash = Values.Length;

        foreach (T value in Values)
        {
            hash = (hash * 397) ^ (value?.GetHashCode() ?? 0);
        }

        return hash;
    }
}
