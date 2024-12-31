namespace Waystone.Monads.Exceptions;

using System;
using Options;
using Results;

/// <summary>
/// An exception that is thrown with a <see cref="None{T}" /> or
/// <see cref="Err{TOk,TErr}" /> is encountered when invoking an <c>Expect</c>
/// function.
/// </summary>
public sealed class UnmetExpectationException : SystemException
{
    internal UnmetExpectationException(string message) : base(message)
    { }

    internal static UnmetExpectationException For<TValue>(
        string message,
        TValue value) =>
        new($"{message}: {value}");
}
