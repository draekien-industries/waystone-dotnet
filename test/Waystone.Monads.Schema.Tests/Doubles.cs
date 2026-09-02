namespace Waystone.Monads.Schemas;

using System.Threading;
using System.Threading.Tasks;
using Waystone.Monads.Results;

internal sealed class PassThrough<T> : SchemaNode<T, T> where T : notnull
{
    internal override Outcome<T> Evaluate(T input, ParseContext context) =>
        Outcome<T>.Passed(input);
}

internal sealed class Rejects<T> : SchemaNode<T, T> where T : notnull
{
    private readonly string _template;

    internal Rejects(string template = "Rejected {Path}: got {Received}.")
    {
        _template = template;
    }

    internal override Outcome<T> Evaluate(T input, ParseContext context) =>
        Outcome<T>.Failed(
            Violations.One(
                context,
                ViolationCodeCatalog.Codes.Malformed,
                _template,
                input));
}

internal sealed class RefinesAndKeeps<T> : SchemaNode<T, T> where T : notnull
{
    internal override Outcome<T> Evaluate(T input, ParseContext context) =>
        Outcome<T>.Refined(
            input,
            Violations.One(
                context,
                ViolationCodeCatalog.Codes.OutOfRange,
                "Refined {Path}, kept {Received}.",
                input));
}

internal sealed class Lengths : SchemaNode<string, int>
{
    internal override Outcome<int> Evaluate(
        string input,
        ParseContext context) =>
        Outcome<int>.Passed(input.Length);
}

internal sealed class RejectsText : SchemaNode<string, int>
{
    internal override Outcome<int> Evaluate(
        string input,
        ParseContext context) =>
        Outcome<int>.Failed(
            Violations.One(
                context,
                ViolationCodeCatalog.Codes.Malformed,
                "Rejected {Path}: got {Received}.",
                input));
}

internal sealed class AsyncPassThrough<T> : SchemaNode<T, T> where T : notnull
{
    internal override Outcome<T> Evaluate(T input, ParseContext context) =>
        Outcome<T>.Passed(input);

    internal override async ValueTask<Outcome<T>> EvaluateAsync(
        T input,
        ParseContext context,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        return Outcome<T>.Passed(input);
    }
}

internal sealed class ComposedOf : Schema<string, int>
{
    private readonly Schema<string, int> _inner;

    internal ComposedOf(Schema<string, int> inner)
    {
        _inner = inner;
    }

    protected override Result<int, SchemaViolation> Configure(string subject) =>
        Schema.Required(subject, _inner)
              .EvaluateValue(ParseContext.Root)
              .ToResult();
}
