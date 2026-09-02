namespace Waystone.Monads.Schemas;

using System;

internal static class Rules
{
    internal static Schema<TIn, TOut> Add<TIn, TOut>(
        Schema<TIn, TOut> schema,
        Func<TOut, bool> predicate,
        ViolationCode code,
        string message,
        object? expected = null)
        where TIn : notnull where TOut : notnull
    {
        if (schema is null) throw new ArgumentNullException(nameof(schema));

        return new CheckSchema<TIn, TOut>(
            schema,
            predicate,
            ViolationCodeCatalog.ToErrorCode(code),
            message,
            expected);
    }
}
