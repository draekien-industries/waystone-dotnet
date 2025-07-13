namespace Waystone.Monads.Iterators.Adapters;

using Abstractions;
using Options;

/// <summary>
/// Represents an iterator that combines two iterators to create a new
/// iterator which yields pairs of elements from the two input iterators.
/// </summary>
/// <typeparam name="TFirst">
/// The type of elements produced by the first iterator.
/// Must be a non-nullable type.
/// </typeparam>
/// <typeparam name="TSecond">
/// The type of elements produced by the second iterator.
/// Must be a non-nullable type.
/// </typeparam>
public sealed class ZipAdapter<TFirst, TSecond> : Iterator<(TFirst, TSecond)>
    where TFirst : notnull where TSecond : notnull
{
    private readonly IIterator<TFirst> _first;
    private readonly IIterator<TSecond> _second;

    /// <summary>
    /// Represents an iterator that combines two iterators, yielding pairs of
    /// elements from the first and second iterators respectively.
    /// </summary>
    public ZipAdapter(IIterator<TFirst> first, IIterator<TSecond> second)
    {
        _first = first;
        _second = second;
    }

    /// <inheritdoc />
    public override Option<(TFirst, TSecond)> Next() =>
        _first.Next().Zip(_second.Next());

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _first.Dispose();
        _second.Dispose();
    }

    /// <inheritdoc />
    public override void Reset()
    {
        _first.Reset();
        _second.Reset();
        base.Reset();
    }
}
