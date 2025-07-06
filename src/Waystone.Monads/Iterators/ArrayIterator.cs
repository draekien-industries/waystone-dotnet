namespace Waystone.Monads.Iterators;

using System;
using Waystone.Monads.Iterators.Abstractions;
using Waystone.Monads.Options;
using Waystone.Monads.Primitives;

/// <summary>
/// An <see cref="Iterator{T}"/> for arrays.
/// </summary>
/// <typeparam name="T">The type of the value contained in the array.</typeparam>
public sealed class ArrayIterator<T> : Iterator<T>
    where T : notnull
{
    private readonly T[] _items;

    internal ArrayIterator(T[] items)
    {
        _items = items;
    }

    /// <summary>
    /// The length of the array
    /// </summary>
    public int Length => _items.Length;

    /// <inheritdoc/>
    public override bool MoveNext()
    {
        if (++CurrentIndex < Length)
        {
            CurrentItem = _items[CurrentIndex];
            return true;
        }

        CurrentItem = Option.None<T>();
        return false;
    }

    /// <inheritdoc/>
    public override (PosInt LowerBound, Option<PosInt> UpperBound) SizeHint()
    {
        PosInt lowerBound = Math.Max(0, Length - (CurrentIndex + 1));
        Option<PosInt> upperBound = lowerBound > 0 ? Option.Some(lowerBound) : Option.None<PosInt>();

        return CurrentIndex switch
        {
            _ when CurrentIndex < Length => (lowerBound, upperBound),
            _ => base.SizeHint()
        };
    }
}
