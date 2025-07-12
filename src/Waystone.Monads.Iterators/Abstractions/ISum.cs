namespace Waystone.Monads.Iterators.Abstractions;

using System.Numerics;

/// <summary>
/// Represents an interface for computing the sum of elements provided by
/// an iterator of a specific numeric type.
/// </summary>
/// <typeparam name="TSelf">
/// The numeric type of the elements to be summed. Must
/// implement <see cref="System.Numerics.INumber{T}" />.
/// </typeparam>
public interface ISum<TSelf> where TSelf : INumber<TSelf>
{
    /// <summary>Computes the sum of the elements in the provided iterator.</summary>
    /// <typeparam name="TIter">
    /// The type of the iterator that contains the elements to
    /// sum. Must implement IIterator of the respective type.
    /// </typeparam>
    /// <param name="iter">An iterator containing the elements to sum.</param>
    /// <returns>The sum of the elements in the iterator of type TSelf.</returns>
    TSelf Sum<TIter>(TIter iter) where TIter : IIterator<TSelf>;
}
