namespace Waystone.Monads.Iterators.Abstractions;

using Options;

/// <summary>
/// Represents an interface for iterating over a collection of items of
/// type <typeparamref name="TItem" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of the item to iterate over. Must be a
/// non-nullable type.
/// </typeparam>
public interface IIterator<TItem> : IEnumerator<Option<TItem>>
    where TItem : notnull
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
    (int LowerBound, Option<int> UpperBound) SizeHint() =>
        (0, Option.None<int>());

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
    int Count()
    {
        var count = 0;
        while (Next().IsSome) count++;
        return count;
    }

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
    Option<TItem> Last()
    {
        Option<TItem> last = Option.None<TItem>();
        while (Next().IsSome) last = Next();
        return last;
    }

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
    Option<TItem> Nth(int n)
    {
        var count = 0;
        Option<TItem> nth = Next();

        while (nth.IsSome)
        {
            if (count == n) return nth;
            nth = Next();
            count++;
        }

        return nth;
    }

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
    /// Invoking <see cref="StepBy" /> on an <see cref="StepByIterator{TItem}" />
    /// will result in a compounding of steps.
    /// </item>
    /// </list>
    /// </remarks>
    /// <returns>
    /// A <see cref="StepByIterator{TItem}" /> that iterates over the
    /// collection by stepping through the specified number of elements.
    /// </returns>
    StepByIterator<TItem> StepBy(int step) => new(this, step);

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
    /// A <see cref="ChainIterator{TItem}" /> that iterates over the items
    /// from both the current iterator and the provided iterator.
    /// </returns>
    ChainIterator<TItem> Chain(IIterator<TItem> other) => new(this, other);

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
    /// A <see cref="ZipIterator{TItem, TOther}" /> that combines elements
    /// from the current iterator and the specified iterator into pairs.
    /// </returns>
    ZipIterator<TItem, TOther> Zip<TOther>(IIterator<TOther> other)
        where TOther : notnull =>
        new(this, other);

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
    /// A <see cref="Waystone.Monads.Iterators.MapIterator{TItem, TOut}" />
    /// that yields the elements of the original iterator transformed by the mapping
    /// function.
    /// </returns>
    MapIterator<TItem, TOut> Map<TOut>(Func<TItem, TOut> map)
        where TOut : notnull => new(this, map);

    /// <summary>
    /// Executes the specified action for each item in the iterator until the
    /// end of the collection is reached.
    /// </summary>
    /// <param name="action">
    /// A delegate representing the operation to perform on each
    /// item of type <typeparamref name="TItem" />.
    /// </param>
    void ForEach(Action<TItem> action) => Next().Inspect(action);

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
    /// A <see cref="FilterIterator{TItem}" /> that iterates over the items
    /// filtered by the given predicate.
    /// </returns>
    FilterIterator<TItem> Filter(Func<TItem, bool> filter) => new(this, filter);

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
    /// A new
    /// <see cref="Waystone.Monads.Iterators.FilterMapIterator{TItem, TOut}" /> that
    /// applies the specified filtering and mapping function.
    /// </returns>
    FilterMapIterator<TItem, TOut> FilterMap<TOut>(
        Func<TItem, Option<TOut>> filterMap) where TOut : notnull =>
        new(this, filterMap);

    /// <summary>
    /// Creates an iterator that enumerates the items in the source iterator,
    /// associating each item with its respective index in the sequence.
    /// </summary>
    /// <returns>
    /// An <see cref="EnumerateIterator{TItem}" /> that produces tuples
    /// containing the index and the item for each item in the source iterator.
    /// </returns>
    EnumerateIterator<TItem> Enumerate() => new(this);
}
