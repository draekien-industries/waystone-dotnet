namespace Waystone.Monads.Iterators;

using Abstractions;
using Options;

/// <summary>
/// An implementation of an <see cref="Iterator{TItem}" /> that filters
/// elements from a source <see cref="IIterator{TItem}" /> based on a predicate
/// function.
/// </summary>
/// <typeparam name="TItem">
/// The type of items being iterated over. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class FilterIterator<TItem> : Iterator<TItem>
    where TItem : notnull
{
    private readonly Func<TItem, bool> _predicate;
    private readonly IIterator<TItem> _source;

    /// <summary>
    /// Creates an instance of the <see cref="FilterIterator{TItem}" />
    /// </summary>
    /// <param name="source">The <see cref="IIterator{TItem}" /> to filter over</param>
    /// <param name="predicate">The filter function to apply</param>
    public FilterIterator(IIterator<TItem> source, Func<TItem, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
    }

    /// <inheritdoc />
    public override Option<TItem> Next() => _source.Next().Filter(_predicate);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _source.Dispose();
    }
}
