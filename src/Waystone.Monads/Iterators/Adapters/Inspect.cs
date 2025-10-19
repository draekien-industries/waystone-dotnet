namespace Waystone.Monads.Iterators.Adapters;

using System;

/// <summary>
/// Represents an iterator adapter that performs a specified action on
/// each element of an iteration sequence without modifying the elements.
/// </summary>
/// <typeparam name="T">
/// The type of elements being iterated over. The type
/// parameter must be non-nullable.
/// </typeparam>
/// <remarks>
/// The <see cref="Inspect{T}" /> adapter is useful for debugging or
/// side-effect operations where you need to examine the elements being iterated
/// over without altering the iteration sequence.
/// </remarks>
public sealed class Inspect<T> : Iter<T> where T : notnull
{
    internal Inspect(Iter<T> iter, Action<T> action) : base(
        iter.Map(item =>
        {
            action(item);
            return item;
        }))
    { }
}
