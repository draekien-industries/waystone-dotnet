namespace Waystone.Monads.Schemas;

internal sealed class SensitiveSchema<TIn, TOut> : ContextSchema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    internal SensitiveSchema(Schema<TIn, TOut> inner) : base(inner)
    {
    }

    protected override ParseContext Adjust(ParseContext context) =>
        context.AsSensitive();
}
