namespace Waystone.Monads.Iterators.Extensions;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides extension methods for <see cref="Iterator{TItem}" /> to
/// facilitate easier interactions and transformations.
/// </summary>
public static class IteratorExtensions
{
    /// <summary>
    /// Creates a new <see cref="CopiedIterator{TItem}" /> from the given
    /// <see cref="Iterator{TItem}" />.
    /// </summary>
    /// <typeparam name="TItem">
    /// The type of elements in the iterator. Must be a value
    /// type.
    /// </typeparam>
    /// <param name="source">
    /// The source <see cref="Iterator{TItem}" /> to be wrapped in
    /// a <see cref="CopiedIterator{TItem}" />.
    /// </param>
    /// <returns>
    /// A new <see cref="CopiedIterator{TItem}" /> containing the elements of
    /// the original <see cref="Iterator{TItem}" />.
    /// </returns>
    public static CopiedIterator<TItem> Copied<TItem>(
        this Iterator<TItem> source)
        where TItem : struct => new(source);

    /// <summary>
    /// Creates a new <see cref="ClonedIterator{TItem}" /> from the given
    /// <see cref="Iterator{TItem}" />.
    /// </summary>
    /// <typeparam name="TItem">
    /// The type of elements in the iterator. Must be a
    /// reference type and implement <see cref="ICloneable" />.
    /// </typeparam>
    /// <param name="source">
    /// The source <see cref="Iterator{TItem}" /> to be wrapped in
    /// a <see cref="ClonedIterator{TItem}" />.
    /// </param>
    /// <returns>
    /// A new <see cref="ClonedIterator{TItem}" /> containing cloned elements
    /// of the original <see cref="Iterator{TItem}" />.
    /// </returns>
    public static ClonedIterator<TItem> Cloned<TItem>(
        this Iterator<TItem> source) where TItem : class, ICloneable =>
        new(source);

    /// <summary>
    /// Cycles through the elements of the given
    /// <see cref="Iterator{TItem}" /> indefinitely. When the end of the source is
    /// reached, it starts again from the beginning.
    /// </summary>
    /// <typeparam name="TItem">
    /// The type of elements in the iterator. Must be a
    /// reference type that implements <see cref="ICloneable" />.
    /// </typeparam>
    /// <param name="source">
    /// The source <see cref="Iterator{TItem}" /> to be converted
    /// into a <see cref="CycleIterator{TItem}" />.
    /// </param>
    /// <returns>
    /// A <see cref="CycleIterator{TItem}" /> that cycles through the elements
    /// of the original <see cref="Iterator{TItem}" />.
    /// </returns>
    public static CycleIterator<TItem> Cycle<TItem>(
        this Iterator<TItem> source) where TItem : class, ICloneable =>
        new(source);

    /// <summary>
    /// Creates an <see cref="Iterator{TItem}" /> that flattens nested
    /// structures. This is useful when you have an iterator of iterators or an
    /// iterator of things that can be turned into iterators and you want to remove one
    /// level of indirection.
    /// </summary>
    /// <param name="source">The nested iterator structure.</param>
    /// <typeparam name="TItem">
    /// The type of the value contained inside the nested
    /// <see cref="IEnumerable{T}" />
    /// </typeparam>
    /// <returns>
    /// A <see cref="FlattenIterator{TItem}" /> that removes one level of
    /// nesting.
    /// </returns>
    public static FlattenIterator<TItem> Flatten<TItem>(
        this Iterator<IEnumerable<TItem>> source)
        where TItem : notnull => new(source);
}
