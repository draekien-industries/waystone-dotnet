namespace Waystone.Monads.Iterators;

using System;
using Abstractions;

/// <summary>An iterator that maps the values of</summary>
/// <typeparam name="T">The type of the value contained in the iterator.</typeparam>
public sealed class MapIterator<T> : Iterator<T> where T : notnull
{
    /// <inheritdoc />
    public override bool MoveNext() => throw new NotImplementedException();
}
