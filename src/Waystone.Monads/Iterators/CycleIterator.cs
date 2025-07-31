namespace Waystone.Monads.Iterators;

using System;
using Extensions;
using Options;

/// <summary>
/// Represents an iterator that cycles through the elements in its source
/// indefinitely. When the end of the source is reached, it starts again from the
/// beginning of the source.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements in the iterator. Must be a
/// non-nullable type that implements <see cref="ICloneable" />.
/// </typeparam>
public sealed class CycleIterator<TItem> : Iterator<TItem>
    where TItem : class, ICloneable
{
    private readonly ClonedIterator<TItem> _original;
    private Iterator<TItem> _iter;

    /// <inheritdoc />
    public CycleIterator(Iterator<TItem> source) : base(source)
    {
        _original = source.Cloned();
        _iter = source;
    }

    /// <inheritdoc />
    public override Option<TItem> Next() =>
        _iter.Next()
             .Match(
                  Option.Some,
                  () =>
                  {
                      _iter = _original.Clone();
                      return _iter.Next();
                  });

    /// <inheritdoc />
    public override (int Lower, Option<int> Upper) SizeHint() =>
        (int.MaxValue, Option.Some(int.MaxValue));
}
