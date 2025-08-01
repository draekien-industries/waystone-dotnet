namespace Waystone.Monads.Iterators.Extensions;

using System;
using Adapters;

/// <summary>Extension methods for <see cref="Iter{T}" />.</summary>
public static class IterExtensions
{
    /// <summary>Creates an <see cref="Iter{T}" /> that clones all of its elements.</summary>
    /// <param name="iter"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Cloned<T> Cloned<T>(this Iter<T> iter) where T : ICloneable =>
        new(iter);
}
