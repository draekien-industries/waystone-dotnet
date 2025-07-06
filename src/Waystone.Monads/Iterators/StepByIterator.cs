namespace Waystone.Monads.Iterators;

using System;
using System.Collections;
using System.Collections.Generic;
using Waystone.Monads.Iterators.Abstractions;
using Waystone.Monads.Options;
using Waystone.Monads.Primitives;

/// <summary>
/// An <see cref="IIterator{T}"/> for stepping iterators by a custom amount.
/// </summary>
/// <typeparam name="T">The type of the value contained in the iterator.</typeparam>
public sealed class StepByIterator<T> : Iterator<T>
    where T : notnull
{
    private readonly PosInt _interval;
    private readonly Iterator<T> _wrapped;
    private readonly int _initialIndex;
    private bool _isFirstStep;

    internal StepByIterator(Iterator<T> iterator, int interval)
    {
        if (interval is 0)
            throw new ArgumentOutOfRangeException("Step By interval must be greater than '0'", nameof(interval));

        _interval = interval;
        _wrapped = iterator;
        _initialIndex = iterator.CurrentIndex;
        _isFirstStep = true;
    }

    /// <inheritdoc/>
    public override Option<T> Current => _wrapped.Current;

    /// <inheritdoc/>
    public override void Dispose() => _wrapped.Dispose();

    /// <inheritdoc/>
    public override Option<T> Next() => MoveNext() ? Current : Option.None<T>();

    /// <inheritdoc/>
    public override bool MoveNext()
    {
        if (_isFirstStep)
        {
            _isFirstStep = false;
            return _wrapped.MoveNext();
        }

        for (int i = 0; i < _interval; i++)
        {
            if (!_wrapped.MoveNext()) return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public override void Reset()
    {
        _wrapped.CurrentIndex = _initialIndex;
        _wrapped.CurrentItem = Option.None<T>();
        _isFirstStep = true;
    }

    /// <inheritdoc/>
    public override (PosInt LowerBound, Option<PosInt> UpperBound) SizeHint()
    {
        var (lowerBound, upperBound) = _wrapped.SizeHint();

        PosInt adjustedLowerBound = Math.Max(0, (lowerBound + _interval - 1) / _interval);

        Option<PosInt> adjustedUpperBound = upperBound
            .Map<PosInt>(x => Math.Max(0, (x + _interval - 1) / _interval))
            .Filter(x => x > 0);

        return (adjustedLowerBound, adjustedUpperBound);
    }

    /// <inheritdoc/>
    public override StepByIterator<T> StepBy(PosInt interval) => new(this, interval);
}
