namespace Waystone.Monads.Iterators.Extensions;

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
    public static Iterator<TItem> Copied<TItem>(this Iterator<TItem> source)
        where TItem : struct => new CopiedIterator<TItem>(source.Source);
}
