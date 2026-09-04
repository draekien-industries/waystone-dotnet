namespace Waystone.Monads.Schemas.Internal.Combinators;

using System;

internal static class Rules
{
    internal static void RequireOrdered<T>(T min, T max, string parameter)
        where T : IComparable<T>
    {
        if (max.CompareTo(min) >= 0) return;

        throw new ArgumentException(
            "The upper bound orders before the lower one, so no value could pass.",
            parameter);
    }

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
