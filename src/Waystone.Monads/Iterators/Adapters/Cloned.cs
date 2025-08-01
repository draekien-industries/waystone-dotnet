namespace Waystone.Monads.Iterators.Adapters;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An iterator that clones the elements of the underlying
/// <see cref="Iter{T}" />
/// </summary>
/// <typeparam name="T">
/// The type of elements in the sequence. Must implement
/// <see cref="ICloneable" />.
/// </typeparam>
public sealed class Cloned<T> : Iter<T> where T : ICloneable
{
    /// <inheritdoc />
    internal Cloned(IEnumerable<T> items) : base(
        items.Select(x => x.Clone()).Cast<T>())
    { }

    /// <inheritdoc />
    internal Cloned(Iter<T> iter) : this(iter.Elements)
    { }
}
