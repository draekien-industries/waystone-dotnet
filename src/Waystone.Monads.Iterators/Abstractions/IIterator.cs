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
}
