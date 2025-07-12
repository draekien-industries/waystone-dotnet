namespace Waystone.Monads.Iterators;

using Abstractions;
using Options;

/// <summary>
/// An iterator that skips a specified number of elements between each
/// yield when iterating over the underlying source.
/// </summary>
/// <typeparam name="TItem">
/// The type of the items contained in the iterator. Must
/// be a non-nullable type.
/// </typeparam>
/// <remarks>
/// This iterator ensures that the first element is always returned before
/// applying the skip logic. Subsequent elements are returned according to the step
/// size provided. The step size must be greater than zero; otherwise, behavior may
/// be undefined.
/// </remarks>
public sealed class StepByIterator<TItem>
    : Iterator<TItem>
    where TItem : notnull
{
    private readonly int _nth;
    private readonly IIterator<TItem> _source;
    private bool _firstTake;

    /// <summary>
    /// Creates an instance of the <see cref="StepByIterator{TItem}" />
    /// </summary>
    /// <param name="source">The <see cref="IIterator{TItem}" /> to iterate over</param>
    /// <param name="step">The number of items to step by</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="step" /> must be
    /// greater than zero
    /// </exception>
    public StepByIterator(IIterator<TItem> source, int step)
    {
        if (step <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(step),
                "Step size must be greater than zero.");
        }

        _source = source;
        _nth = step - 1;
        _firstTake = true;
    }

    /// <inheritdoc />
    public override Option<TItem> Next()
    {
        if (!_firstTake) return _source.Nth(_nth);
        _firstTake = false;
        return _source.Next();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _source.Dispose();
    }

    /// <inheritdoc />
    public override void Reset()
    {
        _source.Reset();
        _firstTake = true;
        base.Reset();
    }
}
