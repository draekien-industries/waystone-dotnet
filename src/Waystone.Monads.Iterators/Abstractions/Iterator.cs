namespace Waystone.Monads.Iterators.Abstractions;

using System.Collections;
using Options;

/// <summary>
/// Represents an abstract implementation of a custom iterator for
/// iterating over a collection of items of type <typeparamref name="TItem" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of the item to iterate over. Must be a
/// non-nullable type.
/// </typeparam>
public abstract class Iterator<TItem> : IIterator<TItem> where TItem : notnull
{
    /// <inheritdoc />
    public bool MoveNext() => Next().IsSome;

    /// <inheritdoc />
    public virtual void Reset()
    {
        Position = -1;
    }

    /// <inheritdoc />
    public virtual Option<TItem> Current { get; set; } = Option.None<TItem>();

    /// <inheritdoc />
    object? IEnumerator.Current => Current;

    /// <inheritdoc />
    public int Position { get; set; } = -1;

    /// <inheritdoc />
    public abstract Option<TItem> Next();

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases the resources used by the iterator.</summary>
    /// <param name="disposing">
    /// A boolean value indicating whether to release both
    /// managed and unmanaged resources (true), or only unmanaged resources (false).
    /// </param>
    protected abstract void Dispose(bool disposing);
}
