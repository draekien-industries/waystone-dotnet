namespace Waystone.Monads.Exceptions;

using System;
using Options;
using Results;

/// <summary>
/// An exception that is thrown when a <see cref="None{T}" /> or
/// <see cref="Err{TOk,TErr}" /> is encountered when invoking an <c>Expect</c>
/// function.
/// </summary>
/// <remarks>
/// From an option the message is the text you passed to <c>Expect</c> unchanged.
/// From a result it is that text, a colon, then the value the result held, so do
/// not pass text you would not want logged beside that value. <c>ExpectErr</c> on
/// an <see cref="Ok{TOk,TErr}" /> throws this too.
/// </remarks>
public sealed class UnmetExpectationException : SystemException
{
    internal UnmetExpectationException(string message) : base(message)
    { }

    internal static UnmetExpectationException For<TValue>(
        string message,
        TValue value) =>
        new($"{message}: {value}");
}
