namespace Waystone.Monads.Iterators.Adapters;

using System;
using System.Collections.Generic;
using Extensions;
using Options;

/// <summary>
/// Represents an iterator that cycles through a sequence of elements of
/// type <typeparamref name="T" />. When the end of the sequence is reached, it
/// starts over from the beginning. This iterator is useful for scenarios where you
/// want to repeatedly iterate over a collection without having to recreate the
/// iterator.
/// </summary>
/// <typeparam name="T">
/// The type of elements in the sequence. Must implement
/// <see cref="ICloneable" />.
/// </typeparam>
public sealed class Cycle<T> : Iter<T>
    where T : ICloneable
{
    private readonly Cloned<T> _original;
    private Iter<T> _current;

    /// <inheritdoc />
    internal Cycle(IEnumerable<T> elements) : this(elements.IntoIter())
    { }

    /// <inheritdoc />
    internal Cycle(Iter<T> iter) : base(iter)
    {
        _original = iter.Cloned();
        _current = iter;
    }

    /// <inheritdoc />
    public override Option<T> Next()
    {
        Option<T> next = _current.Next();

        if (next.IsSome) return next;

        _current = _original.Clone();
        next = _current.Next();

        return next;
    }

    /// <inheritdoc />
    public override (int Lower, Option<int> Upper) SizeHint() =>
        (int.MaxValue, Option.Some(int.MaxValue));
}
