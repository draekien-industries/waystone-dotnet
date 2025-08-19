namespace Waystone.Monads.Iterators.Adapters;

using System.Collections.Generic;
using Options;

/// <summary>
/// An <see cref="Iter{T}" /> which ends after the first
/// <see cref="None{T}" />.
/// </summary>
/// <remarks>
/// After an iterator returns <see cref="None{T}" />, it will never return
/// <see cref="Some{T}" /> again.
/// </remarks>
/// <typeparam name="T">
/// The type of elements in the sequence. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class Fuse<T> : Iter<T> where T : notnull
{
    /// <inheritdoc />
    public Fuse(IEnumerable<T> elements) : base(elements)
    { }

    /// <inheritdoc />
    internal Fuse(Iter<T> iter) : base(iter)
    { }

    /// <inheritdoc />
    internal Fuse(IEnumerable<Option<T>> elements) : base(elements)
    { }

    private bool IsFused { get; set; }

    /// <inheritdoc />
    public override Option<T> Next()
    {
        if (IsFused) return Option.None<T>();

        Option<T> next = base.Next();

        IsFused = next.IsNone;
        return next;
    }
}
