namespace Waystone.Monads.Iterators.Abstractions;

using System.Numerics;

/// <summary>
/// Represents a generic interface for computing a product of elements,
/// where the computation is defined by the implementation. The type implementing
/// this interface must be a non-nullable type.
/// </summary>
/// <typeparam name="TSelf">
/// The type of the object implementing this interface.
/// Must be a non-nullable type.
/// </typeparam>
public interface IProduct<TSelf>
    where TSelf : INumber<TSelf>
{
    /// <summary>Computes the product of elements provided by the iterator.</summary>
    /// <typeparam name="TIter">
    /// The type of the iterator that provides the elements.
    /// Must implement <see cref="IIterator{TSelf}" />.
    /// </typeparam>
    /// <param name="iter">
    /// The iterator that provides the elements to compute the
    /// product.
    /// </param>
    /// <returns>
    /// The product of the elements provided by the iterator as an instance of
    /// <typeparamref name="TSelf" />.
    /// </returns>
    TSelf Product<TIter>(TIter iter) where TIter : IIterator<TSelf>;
}
