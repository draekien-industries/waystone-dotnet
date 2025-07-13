namespace Waystone.Monads.Iterators.Adapters;

using System;
using Abstractions;
using Options;

/// <summary>
/// Represents a custom iterator that skips elements from the source
/// iterator while a specified predicate evaluates to true.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements in the sequence. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class SkipWhileAdapter<TItem> : Iterator<TItem>
    where TItem : notnull
{
    private readonly Func<TItem, bool> _predicate;
    private readonly IIterator<TItem> _source;

    /// <summary>
    /// An iterator implementation that skips elements from the source
    /// iterator as long as the provided predicate evaluates to true. Once an element
    /// fails the predicate, all subsequent elements are yielded from the source
    /// iterator without any further checks.
    /// </summary>
    public SkipWhileAdapter(
        IIterator<TItem> source,
        Func<TItem, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
    }

    /// <inheritdoc />
    public override Option<TItem> Next()
    {
        Option<TItem> next = _source.Next();
        while (next.Filter(_predicate).IsSome)
        {
            next = _source.Next();
        }

        return next;
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
        base.Reset();
    }
}
