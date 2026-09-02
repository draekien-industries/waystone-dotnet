namespace Waystone.Monads.Schemas;

using System;
using Waystone.Monads.Results.Errors;

internal sealed class CodeSchema<TIn, TOut> : RewritingSchema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    private readonly ErrorCode _code;

    internal CodeSchema(Schema<TIn, TOut> inner, ErrorCode code) : base(inner)
    {
        _code = code ?? throw new ArgumentNullException(nameof(code));
    }

    protected override Violation Rewrite(
        Violation violation,
        TIn input,
        ParseContext context) =>
        violation.Recoded(_code);
}
