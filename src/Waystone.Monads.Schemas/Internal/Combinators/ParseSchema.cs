namespace Waystone.Monads.Schemas.Internal.Combinators;

internal delegate bool TextParser<T>(string text, out T value);

internal sealed class ParseSchema<TIn, TNext> : DecoratorSchema<TIn, string, TNext>
    where TIn : notnull where TNext : notnull
{
    private readonly string _message;

    private readonly TextParser<TNext> _parse;

    internal ParseSchema(
        Schema<TIn, string> inner,
        TextParser<TNext> parse,
        string message) : base(inner)
    {
        _parse = parse;
        _message = message;
    }

    protected override Outcome<TNext> Decorate(
        TIn input,
        ParseContext context,
        Outcome<string> outcome)
    {
        if (!outcome.HasValue)
        {
            return Outcome<TNext>.Failed(outcome.Violations);
        }

        return _parse(outcome.Value, out TNext next)
            ? outcome.WithValue(next)
            : Outcome<TNext>.Failed(
                Violations.Add(
                    outcome.Violations,
                    context,
                    ViolationCodeCatalog.Codes.Malformed,
                    _message,
                    outcome.Value));
    }
}
