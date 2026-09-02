namespace Waystone.Monads.Schemas.Internal.Combinators;

using System;

internal sealed class NamedSchema<TIn, TOut> : ContextSchema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    private readonly string _name;

    internal NamedSchema(Schema<TIn, TOut> inner, string name) : base(inner)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    protected override ParseContext Adjust(ParseContext context) =>
        context.Renamed(_name);
}
