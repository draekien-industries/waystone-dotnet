namespace Waystone.Monads.Iterators.Extensions;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Waystone.Monads.Iterators.Abstractions;

/// <summary>
/// Extensions for converting various types into an <see cref="Iterator{T}"/>.
/// </summary>
public static class IntoIterExtensions
{
    /// <summary>
    /// Converts an array of items into an <see cref="ArrayIterator{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained inside the array</typeparam>
    /// <param name="items">The array to iterate over</param>
    /// <returns>An <see cref="ArrayIterator{T}"/></returns>
    public static ArrayIterator<T> IntoIter<T>(this T[] items)
        where T : notnull => new(items);

    /// <summary>
    /// Converts any <see cref="IEnumerable{T}"/> of items into an <see cref="ArrayIterator{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained inside the array</typeparam>
    /// <param name="items">The array to iterate over</param>
    /// <returns>An <see cref="ArrayIterator{T}"/></returns>
    public static ArrayIterator<T> IntoIter<T>(this IEnumerable<T> items)
        where T : notnull => new(items.ToArray());
}