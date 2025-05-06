namespace Waystone.Monads.Exceptions;

using System;
using Options;
using Results;

/// <summary>
/// An exception which is thrown when attempting to <c>Unwrap</c> a
/// <see cref="None{T}" />
/// </summary>
public class UnwrapException : SystemException
{
    internal UnwrapException(string message) : base(message)
    { }

    internal UnwrapException(string message, Exception innerException) :
        base(message, innerException)
    { }

    internal static UnwrapException<TErr>
        For<TOk, TErr>(Err<TOk, TErr> err)
        where TOk : notnull where TErr : notnull =>
        new(
            "Unwrap called on an `Error` result.",
            err.Value);

    internal static UnwrapException<TOk> For<TOk, TErr>(Ok<TOk, TErr> ok)
        where TOk : notnull where TErr : notnull =>
        new(
            "Unwrap called on an `Ok` result.",
            ok.Value);
}

/// <summary>
/// An exception which is thrown when attempting to <c>Unwrap</c> an
/// <see cref="Err{TOk,TErr}" />
/// </summary>
/// <typeparam name="T">
/// The value contained in the <see cref="Err{TOk,TErr}" />
/// </typeparam>
public sealed class UnwrapException<T> : UnwrapException
    where T : notnull
{
    /// <inheritdoc />
    internal UnwrapException(string message, T value) : base(message)
    {
        Value = value;
    }

    /// <inheritdoc />
    internal UnwrapException(
        string message,
        T value,
        Exception innerException) : base(message, innerException)
    {
        Value = value;
    }

    /// <summary>The value contained in the <see cref="Err{TOk,TErr}" /></summary>
    public T Value { get; }
}
