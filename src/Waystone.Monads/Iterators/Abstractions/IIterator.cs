namespace Waystone.Monads.Iterators.Abstractions;

using System;
using System.Collections.Generic;
using Adapters;
using Options;

/// <summary>
/// Represents an interface for iterating over a collection of items of
/// type <typeparamref name="TItem" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of the item to iterate over. Must be a
/// non-nullable type.
/// </typeparam>
public interface IIterator<TItem>
    : IEnumerable<Option<TItem>>,
      IEnumerator<Option<TItem>> where TItem : notnull
{
    /// <summary>Gets the current index of the iterator within the collection.</summary>
    /// <remarks>
    /// This property provides the zero-based position of the iterator. It
    /// represents the index of the current item being pointed to within the
    /// collection. If the iterator has not started iterating, the value would
    /// typically indicate a position before the first element, depending on the
    /// implementation.
    /// </remarks>
    /// <value>
    /// An integer representing the current position of the iterator within the
    /// collection.
    /// </value>
    int Position { get; }

    /// <summary>
    /// Advances the iterator and returns the next item in the collection if
    /// available. If the iterator has reached the end, it returns a
    /// <see cref="Waystone.Monads.Options.None{T}" />.
    /// </summary>
    /// <returns>
    /// An <see cref="Waystone.Monads.Options.Option{TItem}" /> representing
    /// the next item in the collection, or
    /// <see cref="Waystone.Monads.Options.None{TItem}" /> if the end of the collection
    /// has been reached.
    /// </returns>
    Option<TItem> Next();

    /// <summary>
    /// Provides a hint about the size of the remaining items in the
    /// collection through a lower bound and an optional upper bound. The lower bound
    /// is guaranteed to be accurate, while the upper bound may not always be provided
    /// or accurate, depending on the iterator implementation.
    /// </summary>
    /// <returns>
    /// A tuple containing the lower bound as an integer and an optional upper
    /// bound represented as an <see cref="Option{T}" />. The lower bound is the
    /// minimum number of items remaining, while the upper bound, if present, is an
    /// estimate of the maximum number of items remaining.
    /// </returns>
    (int LowerBound, Option<int> UpperBound) SizeHint();

    /// <summary>
    /// Consumes the iterator, counting the number of iterations and returning
    /// it.
    /// </summary>
    /// <remarks>
    /// This method will call <see cref="Next" /> repeatedly until
    /// <see cref="None{T}" /> is encountered, returning the number off times it saw
    /// <see cref="Some{T}" />. Note that <see cref="Next" /> has to be called at least
    /// once even if the iterator does not have any elements.
    /// </remarks>
    /// <returns>
    /// An integer representing the total number of items remaining in the
    /// collection.
    /// </returns>
    int Count();

    /// <summary>Consumes the iterator, returning the last element.</summary>
    /// <remarks>
    /// This method will evaluate the iterator until it returns
    /// <see cref="None{T}" />. While doing so, it keeps track of the current element.
    /// After <see cref="None{T}" /> is returned, <see cref="Last" /> will then return
    /// the last element it saw.
    /// </remarks>
    /// <returns>
    /// An <see cref="Option{TItem}" /> representing the last item in the
    /// collection, or <see cref="None{TItem}" /> if the collection is empty.
    /// </returns>
    Option<TItem> Last();

    /// <summary>
    /// Returns the nth item in the collection if it exists. If the specified
    /// index is out of bounds, it returns a <see cref="None{T}" />.
    /// </summary>
    /// <remarks>
    /// Note that all preceding elements, as well as the returned element,
    /// will be consumed from the iterator. That means that the preceding elements will
    /// be discarded, and also that calling <c>Nth(0)</c> multiple times on the same
    /// iterator will return different elements.
    /// </remarks>
    /// <param name="n">
    /// The zero-based index of the item to retrieve from the
    /// collection.
    /// </param>
    /// <returns>
    /// An <see cref="Option{TItem}" /> representing the nth item in the
    /// collection, or <see cref="None{TItem}" /> if the index is out of bounds.
    /// </returns>
    Option<TItem> Nth(int n);

    /// <summary>
    /// Creates a new iterator that skips a specified number of items between
    /// each iteration of the original collection.
    /// </summary>
    /// <param name="step">
    /// The step size, representing the number of items to skip
    /// between each iteration. Must be greater than zero.
    /// </param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// The first element of the iterator will always be returned, regardless of
    /// the step given
    /// </item>
    /// <item>
    /// Invoking <see cref="StepBy" /> on an <see cref="StepByAdapter{TItem}" />
    /// will result in a compounding of steps.
    /// </item>
    /// </list>
    /// </remarks>
    /// <returns>
    /// A <see cref="StepByAdapter{TItem}" /> that iterates over the
    /// collection by stepping through the specified number of elements.
    /// </returns>
    StepByAdapter<TItem> StepBy(int step);

    /// <summary>
    /// Combines the current iterator with another iterator to create a
    /// single, chainable iterator. The resulting iterator will iterate through the
    /// items of the current iterator first, followed by the items of the provided
    /// iterator.
    /// </summary>
    /// <param name="other">
    /// The other iterator to chain after the current iterator.
    /// Must not be null.
    /// </param>
    /// <returns>
    /// A <see cref="ChainAdapter{TItem}" /> that iterates over the items from
    /// both the current iterator and the provided iterator.
    /// </returns>
    ChainAdapter<TItem> Chain(IIterator<TItem> other);

    /// <summary>
    /// Combines the elements of the current iterator with those of the
    /// specified iterator, producing a new iterator that yields pairs of elements.
    /// </summary>
    /// <typeparam name="TOther">
    /// The type of the elements in the other iterator. Must
    /// be a non-nullable type.
    /// </typeparam>
    /// <param name="other">
    /// The iterator whose elements will be combined with the
    /// elements of the current iterator.
    /// </param>
    /// <returns>
    /// A <see cref="ZipAdapter{TFirst,TSecond}" /> that combines elements
    /// from the current iterator and the specified iterator into pairs.
    /// </returns>
    ZipAdapter<TItem, TOther> Zip<TOther>(IIterator<TOther> other)
        where TOther : notnull;

    /// <summary>
    /// Applies a mapping function to each element in the iterator and returns
    /// a new iterator that yields the transformed elements.
    /// </summary>
    /// <typeparam name="TOut">
    /// The type of the elements produced by the mapping
    /// function. Must be a non-nullable type.
    /// </typeparam>
    /// <param name="map">
    /// A function that defines the transformation to apply to each
    /// element in the iterator.
    /// </param>
    /// <returns>
    /// A <see cref="MapAdapter{TIn,TOut}" /> that yields the elements of the
    /// original iterator transformed by the mapping function.
    /// </returns>
    MapAdapter<TItem, TOut> Map<TOut>(Func<TItem, TOut> map)
        where TOut : notnull;

    /// <summary>
    /// Executes the specified action for each item in the iterator until the
    /// end of the collection is reached.
    /// </summary>
    /// <param name="action">
    /// A delegate representing the operation to perform on each
    /// item of type <typeparamref name="TItem" />.
    /// </param>
    void ForEach(Action<TItem> action);

    /// <summary>
    /// Filters items in the collection based on the provided predicate. Only
    /// the items that satisfy the given predicate will remain in the resulting
    /// iterator.
    /// </summary>
    /// <param name="filter">
    /// A function that evaluates whether an item should be
    /// included in the filtered collection. The function should return true to include
    /// the item or false to exclude it.
    /// </param>
    /// <returns>
    /// A <see cref="FilterAdapter{TItem}" /> that iterates over the items
    /// filtered by the given predicate.
    /// </returns>
    FilterAdapter<TItem> Filter(Func<TItem, bool> filter);

    /// <summary>
    /// Filters and maps the items in the current iterator using the specified
    /// function. Only items for which the function returns a value are included in the
    /// resulting iterator, with the value transformed as defined by the function.
    /// </summary>
    /// <typeparam name="TOut">
    /// The type of the transformed items produced by the
    /// function.
    /// </typeparam>
    /// <param name="filterMap">
    /// A function that takes an item of type
    /// <typeparamref name="TItem" /> and returns an
    /// <see cref="Waystone.Monads.Options.Option{TOut}" />. If the function returns
    /// <see cref="Waystone.Monads.Options.None{TOut}" />, the item will be excluded
    /// from the resulting iterator. Otherwise, the resulting value is included in the
    /// iterator.
    /// </param>
    /// <returns>
    /// A new <see cref="FilterMapAdapter{TItem,TOut}" /> that applies the
    /// specified filtering and mapping function.
    /// </returns>
    FilterMapAdapter<TItem, TOut> FilterMap<TOut>(
        Func<TItem, Option<TOut>> filterMap) where TOut : notnull;

    /// <summary>
    /// Creates an iterator that enumerates the items in the source iterator,
    /// associating each item with its respective index in the sequence.
    /// </summary>
    /// <returns>
    /// An <see cref="EnumerateAdapter{TItem}" /> that produces tuples
    /// containing the index and the item for each item in the source iterator.
    /// </returns>
    EnumerateAdapter<TItem> Enumerate();

    /// <summary>
    /// Creates a new <see cref="PeekableAdapter{TItem}" /> instance that
    /// allows peeking at the next item in the sequence without advancing the iterator.
    /// </summary>
    /// <remarks>
    /// Note that the underlying iterator is still advanced when peek is
    /// called for the first time.
    /// </remarks>
    /// <returns>
    /// A <see cref="PeekableAdapter{TItem}" /> wrapping the current iterator,
    /// enabling peek functionality.
    /// </returns>
    PeekableAdapter<TItem> Peekable();

    /// <summary>
    /// Returns a new iterator that skips elements from the source iterator
    /// while the specified predicate evaluates to true.
    /// </summary>
    /// <param name="predicate">
    /// A function to test each item in the sequence. Items are
    /// skipped while this predicate returns true.
    /// </param>
    /// <returns>
    /// A <see cref="SkipWhileAdapter{TItem}" /> that represents the sequence
    /// of elements after skipping elements based on the provided predicate.
    /// </returns>
    SkipWhileAdapter<TItem> SkipWhile(Func<TItem, bool> predicate);

    /// <summary>
    /// Returns a new iterator that yields items from the current iterator as
    /// long as the provided predicate evaluates to true for each item. The iteration
    /// stops as soon as the predicate evaluates to false for an item.
    /// </summary>
    /// <param name="predicate">
    /// A function that defines the condition to evaluate for
    /// each item. The iteration continues while the predicate returns true and stops
    /// when it returns false.
    /// </param>
    /// <returns>
    /// A <see cref="TakeWhileAdapter{TItem}" /> representing an iterator that
    /// yields items while the specified predicate evaluates to true.
    /// </returns>
    TakeWhileAdapter<TItem> TakeWhile(Func<TItem, bool> predicate);

    /// <summary>
    /// Maps items using the provided mapping function while the mapping
    /// function returns a value wrapped in an <see cref="Option{TOut}" />. Iteration
    /// halts when the mapping function returns <see cref="None{TOut}" />.
    /// </summary>
    /// <param name="map">
    /// The function to apply to each item in the source collection.
    /// This function returns an <see cref="Option{TOut}" /> for each input. Returning
    /// <see cref="None{TOut}" /> indicates that iteration should stop.
    /// </param>
    /// <typeparam name="TOut">The type of the mapped items to output.</typeparam>
    /// <returns>
    /// A <see cref="MapWhileAdapter{TItem,TOut}" /> that produces the mapped
    /// items while the mapping function returns values wrapped in an
    /// <see cref="Option{TOut}" />.
    /// </returns>
    MapWhileAdapter<TItem, TOut> MapWhile<TOut>(Func<TItem, Option<TOut>> map)
        where TOut : notnull;

    /// <summary>
    /// Returns a new iterator that skips a specified number of elements from
    /// the beginning of the current collection.
    /// </summary>
    /// <param name="n">
    /// The number of elements to skip from the start of the
    /// collection.
    /// </param>
    /// <returns>
    /// A <see cref="SkipAdapter{TItem}" /> that begins iteration after the
    /// specified number of elements have been skipped.
    /// </returns>
    SkipAdapter<TItem> Skip(int n);

    /// <summary>
    /// Creates an adapter that limits the number of items an iterator can
    /// yield. When the specified limit is reached, the iterator terminates.
    /// </summary>
    /// <param name="n">
    /// The maximum number of items to yield from the source iterator.
    /// Must be a non-negative integer.
    /// </param>
    /// <returns>
    /// A <see cref="TakeAdapter{TItem}" /> that iterates over at most
    /// <paramref name="n" /> items from the source iterator.
    /// </returns>
    TakeAdapter<TItem> Take(int n);

    /// <summary>
    /// Applies a stateful computation over the elements of the iterator,
    /// producing a new iterator that yields the results of applying the computation at
    /// each step. The computation uses both the current state and the current element
    /// to produce the next state and output.
    /// </summary>
    /// <typeparam name="TState">
    /// The type of the state used in the computation. Must be
    /// a non-nullable type.
    /// </typeparam>
    /// <typeparam name="TOut">
    /// The type of the output elements of the resulting
    /// iterator. Must be a non-nullable type.
    /// </typeparam>
    /// <param name="initialState">The initial state to start the computation.</param>
    /// <param name="scan">
    /// A function that takes the current state, the current item
    /// from the underlying iterator, and produces a new state and an output element.
    /// </param>
    /// <returns>
    /// A <see cref="ScanAdapter{TItem, TState, TOut}" /> that represents the
    /// iterator yielding output based on the stateful computation.
    /// </returns>
    ScanAdapter<TItem, TState, TOut> Scan<TState, TOut>(
        TState initialState,
        Func<TState, TItem, Option<TOut>> scan)
        where TState : notnull where TOut : notnull;

    /// <summary>
    /// Projects each item in the iterator into an
    /// <see cref="IEnumerable{TOut}" />, flattens the resulting collections, and
    /// returns a new iterator over the flattened results.
    /// </summary>
    /// <typeparam name="TOut">
    /// The type of the output elements resulting from the
    /// projection. Must be a non-nullable type.
    /// </typeparam>
    /// <param name="map">
    /// A function to transform each item in the iterator into an
    /// <see cref="IEnumerable{TOut}" />.
    /// </param>
    /// <returns>
    /// A <see cref="FlatMapAdapter{TItem, TOut}" /> that represents an
    /// iterator over the flattened collection.
    /// </returns>
    FlatMapAdapter<TItem, TOut> FlatMap<TOut>(
        Func<TItem, IEnumerable<TOut>> map) where TOut : notnull;

    /// <summary>
    /// Returns an iterator that yields the first item of the current iterator
    /// and then stops. If the iterator is empty, it yields <see cref="None{TItem}" />.
    /// </summary>
    /// <returns>
    /// A <see cref="OnceAdapter{TItem}" /> that produces at most one element
    /// from the current iterator.
    /// </returns>
    OnceAdapter<TItem> Once();

    /// <summary>
    /// Transforms an iterator by applying a specified mapping function to its
    /// items and returning a new iterator that produces the mapped items.
    /// </summary>
    /// <typeparam name="TOut">
    /// The type of the items in the resulting iterator, which
    /// is the result of applying the mapping function to the source items.
    /// </typeparam>
    /// <param name="map">
    /// A function that is applied to each item in the source
    /// iterator to produce a new item of type <typeparamref name="TOut" />.
    /// </param>
    /// <returns>
    /// A <see cref="OnceWithAdapter{TItem, TOut}" /> that provides an
    /// iterator transforming items from the source iterator using the specified
    /// mapping function.
    /// </returns>
    OnceWithAdapter<TItem, TOut> OnceWith<TOut>(Func<TItem, TOut> map)
        where TOut : notnull;
}
