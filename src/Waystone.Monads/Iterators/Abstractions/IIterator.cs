namespace Waystone.Monads.Iterators.Abstractions;

using System.Collections.Generic;
using Waystone.Monads.Options;

/// <summary>
/// An iterator that returns an <see cref="Option{T}"/> when
/// enumerated.
/// </summary>
/// <typeparam name="T">The type of the value contained in the iterator.</typeparam>
public interface IIterator<T> : IEnumerator<Option<T>>, IEnumerable<Option<T>>
    where T : notnull;