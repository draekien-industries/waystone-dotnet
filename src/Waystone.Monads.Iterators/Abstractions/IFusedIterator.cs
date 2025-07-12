namespace Waystone.Monads.Iterators.Abstractions;

using Options;

/// <summary>
/// An iterator that always continues to yield <see cref="None{T}" /> when
/// exhausted.
/// </summary>
/// <typeparam name="TItem">
/// The type of the item to iterate over. Must be a
/// non-nullable type.
/// </typeparam>
public interface IFusedIterator<TItem> : IIterator<TItem> where TItem : notnull
{ }
