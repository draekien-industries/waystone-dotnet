namespace Waystone.Monads.Iterators.Abstractions;

using System;

/// <summary>
/// Provides functionality to create a new copy or update the current
/// instance of an object.
/// </summary>
/// <typeparam name="T">
/// The type of object that implements the interface, which
/// must be non-nullable.
/// </typeparam>
public interface ICloneable<T> : ICloneable where T : class, ICloneable
{
    /// <summary>Creates a new copy of the object with the same value.</summary>
    /// <returns>A new object that is a copy of the current instance.</returns>
    new T Clone();

    /// <summary>
    /// Updates the current instance's state by copying the values from
    /// another instance.
    /// </summary>
    /// <param name="other">The instance from which values will be copied.</param>
    void CloneFrom(T other);
}
