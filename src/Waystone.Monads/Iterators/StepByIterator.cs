namespace Waystone.Monads.Iterators;

using System;
using System.Collections;
using System.Collections.Generic;
using Waystone.Monads.Iterators.Abstractions;
using Waystone.Monads.Options;

/// <summary>
/// An <see cref="IIterator{T}"/> for stepping iterators by a custom amount.
/// </summary>
/// <typeparam name="T">The type of the value contained in the iterator.</typeparam>
public sealed class StepByIterator<T> : IIterator<T>
    where T : notnull
{
    private readonly int _interval;
    private readonly Iterator<T> _iterator;
    private readonly int _initialIndex;
    private bool _isFirstStep;

    internal StepByIterator(Iterator<T> iterator, int interval)
    {
        if (interval is 0)
            throw new ArgumentOutOfRangeException("Step By interval must be greater than '0'", nameof(interval));

        _interval = interval;
        _iterator = iterator;
        _initialIndex = iterator.CurrentIndex;
        _isFirstStep = true;
    }

    /// <inheritdoc/>
    public Option<T> Current => _iterator.Current;

    /// <inheritdoc/>
    object IEnumerator.Current => Current;

    /// <inheritdoc/>
    public void Dispose() => _iterator.Dispose();

    /// <inheritdoc/>
    public IEnumerator<Option<T>> GetEnumerator() => this;

    /// <inheritdoc/>
    public Option<T> Next() => MoveNext() ? Current : Option.None<T>();

    /// <inheritdoc/>
    public bool MoveNext()
    {
        if (_isFirstStep)
        {
            _isFirstStep = false;
            return _iterator.MoveNext();
        }

        for (uint i = 0; i < _interval; i++)
        {
            if (!_iterator.MoveNext()) return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _iterator.CurrentIndex = _initialIndex;
        _iterator.CurrentItem = Option.None<T>();
        _isFirstStep = true;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public (int LowerBound, Option<int> UpperBound) SizeHint()
    {
        var (lowerBound, upperBound) = _iterator.SizeHint();

        int adjustedLowerBound = Math.Max(0, (lowerBound + _interval - 1) / _interval);

        Option<int> adjustedUpperBound = upperBound
            .Map(x => (x + _interval - 1) / _interval)
            .Filter(x => x > 0);

        return (adjustedLowerBound, adjustedUpperBound);
    }

    /// <inheritdoc/>
    public Option<T> Nth(int n)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public StepByIterator<T> StepBy(int interval)
    {
        throw new NotImplementedException();
    }
}