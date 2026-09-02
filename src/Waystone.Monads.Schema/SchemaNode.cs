namespace Waystone.Monads.Schemas;

using System;
using Waystone.Monads.Results;

internal abstract class SchemaNode<TIn, TOut> : Schema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    protected sealed override Result<TOut, SchemaViolation> Configure(
        TIn subject) =>
        throw new NotSupportedException(
            "A schema node evaluates directly and never routes through Configure.");
}
