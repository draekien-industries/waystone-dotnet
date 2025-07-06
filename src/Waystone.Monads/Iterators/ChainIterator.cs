namespace Waystone.Monads.Iterators;

using Waystone.Monads.Iterators.Abstractions;
using Waystone.Monads.Options;
using Waystone.Monads.Primitives;

/// <summary>
/// An <see cref="Iterator{T}"/> that chains two iterators together.
/// The first iterator will be enumerated first, and if it is exhausted,
/// the second iterator will be enumerated.
/// </summary>
/// <typeparam name="T">
/// The type of the value contained in the iterator.
/// </typeparam>
public sealed class ChainIterator<T> : Iterator<T>
    where T : notnull
{
    private readonly Iterator<T> _first;
    private readonly Iterator<T> _second;

    internal ChainIterator(Iterator<T> first, Iterator<T> second)
    {
        _first = first;
        _second = second;
    }

    /// <inheritdoc/>
    public override bool MoveNext()
    {
        if (_first.MoveNext())
        {
            CurrentItem = _first.Current;
            CurrentIndex = _first.CurrentIndex;
            return true;
        }

        if (_second.MoveNext())
        {
            CurrentItem = _second.Current;
            CurrentIndex = _first.CurrentIndex + _second.CurrentIndex + 1; // +1 for the switch to the second iterator
            return true;
        }

        CurrentItem = Option.None<T>();
        return false;
    }

    /// <inheritdoc/>
    public override void Reset()
    {
        _first.Reset();
        _second.Reset();
        CurrentItem = Option.None<T>();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        _first.Dispose();
        _second.Dispose();
    }

    /// <inheritdoc/>
    public override (PosInt LowerBound, Option<PosInt> UpperBound) SizeHint()
    {
        var (firstLowerBound, firstUpperBound) = _first.SizeHint();
        var (secondLowerBound, secondUpperBound) = _second.SizeHint();

        var lowerBound = firstLowerBound + secondLowerBound;
        var upperBound = firstUpperBound.ZipWith<PosInt, PosInt>(secondUpperBound, (a, b) => a + b);
        return (lowerBound, upperBound);
    }
}