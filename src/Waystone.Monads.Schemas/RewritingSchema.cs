namespace Waystone.Monads.Schemas;

internal abstract class RewritingSchema<TIn, TOut>
    : DecoratorSchema<TIn, TOut, TOut>
    where TIn : notnull where TOut : notnull
{
    protected RewritingSchema(Schema<TIn, TOut> inner) : base(inner)
    {
    }

    protected sealed override Outcome<TOut> Decorate(
        TIn input,
        ParseContext context,
        Outcome<TOut> outcome)
    {
        if (outcome.Violations.Count == 0) return outcome;

        var rewritten = new Violation[outcome.Violations.Count];

        for (var index = 0; index < rewritten.Length; index++)
        {
            rewritten[index] = Rewrite(
                outcome.Violations[index],
                input,
                context);
        }

        return outcome.WithViolations(rewritten);
    }

    protected abstract Violation Rewrite(
        Violation violation,
        TIn input,
        ParseContext context);
}
