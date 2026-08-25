namespace Waystone.Monads.Exceptions;

using System;
using Options;
using Results;

/// <summary>
/// An exception which is thrown when attempting to <c>Unwrap</c> a
/// <see cref="None{T}" />
/// </summary>
/// <remarks>
/// The base of <see cref="UnwrapException{T}" />, which is what an <c>Unwrap</c>
/// on a result throws, so catching this catches both. The library constructs it;
/// there is no public constructor.
/// </remarks>
public class UnwrapException : SystemException
{
    internal UnwrapException(string message) : base(message)
    { }

    internal static UnwrapException<TErr>
        For<TOk, TErr>(Err<TOk, TErr> err)
        where TOk : notnull where TErr : notnull =>
        new(
            "Unwrap called on an `Err` result.",
            err.Value);

    internal static UnwrapException<TOk> For<TOk, TErr>(Ok<TOk, TErr> ok)
        where TOk : notnull where TErr : notnull =>
        new(
            "UnwrapErr called on an `Ok` result.",
            ok.Value);
}

/// <summary>
/// An exception which is thrown when attempting to <c>Unwrap</c> an
/// <see cref="Err{TOk,TErr}" />, or to <c>UnwrapErr</c> an
/// <see cref="Ok{TOk,TErr}" />
/// </summary>
/// <remarks>
/// <see cref="Value" /> carries whichever value the result actually held, so a
/// handler can report what it hit without unwrapping again.
/// </remarks>
/// <typeparam name="T">
/// The type of the value the result held: <c>TErr</c> when <c>Unwrap</c> was
/// called on an error, <c>TOk</c> when <c>UnwrapErr</c> was called on a success
/// </typeparam>
public sealed class UnwrapException<T> : UnwrapException
    where T : notnull
{
    internal UnwrapException(string message, T value) : base(message)
    {
        Value = value;
    }

    /// <summary>The value the result held when the unwrap failed.</summary>
    public T Value { get; }
}
