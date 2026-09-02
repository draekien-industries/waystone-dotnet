namespace Waystone.Monads.Schemas;

internal sealed class IdentitySchema<T> : Schema<T, T> where T : notnull
{
    private IdentitySchema()
    {
    }

    internal static Schema<T, T> Instance { get; } = new IdentitySchema<T>();

    internal override Outcome<T> Evaluate(T input, ParseContext context) =>
        Outcome<T>.Passed(input);
}
