namespace Waystone.Monads.Schemas;

using System;

internal sealed class MessageSchema<TIn, TOut> : RewritingSchema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    private readonly string _template;

    internal MessageSchema(Schema<TIn, TOut> inner, string template)
        : base(inner)
    {
        _template = template
                 ?? throw new ArgumentNullException(nameof(template));
    }

    protected override Violation Rewrite(
        Violation violation,
        TIn input,
        ParseContext context) =>
        violation.Retold(_template, input, context.IsSensitive);
}
