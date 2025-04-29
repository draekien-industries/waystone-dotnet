namespace Waystone.Monads.Results.Errors;

using System;

internal sealed class DefaultExceptionErrorCodeFormatter<T>
    : IErrorCodeFormatter<T> where T : Exception
{
    /// <inheritdoc />
    public string Format(T value) => typeof(T).Name;
}
