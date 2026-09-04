namespace Waystone.Monads.Schemas;

using System;
using System.Threading;
using System.Threading.Tasks;
using Waystone.Monads.Results;

internal sealed class PassThrough<T> : Schema<T, T> where T : notnull
{
    internal override Outcome<T> Evaluate(T input, ParseContext context) =>
        Outcome<T>.Passed(input);
}

internal sealed class Rejects<T> : Schema<T, T> where T : notnull
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

internal sealed class RefinesAndKeeps<T> : Schema<T, T> where T : notnull
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

internal sealed class Lengths : Schema<string, int>
{
    internal override Outcome<int> Evaluate(
        string input,
        ParseContext context) =>
        Outcome<int>.Passed(input.Length);
}

internal sealed class RejectsText : Schema<string, int>
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

internal sealed class AsyncPassThrough<T> : Schema<T, T> where T : notnull
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

internal sealed class ComposedOf : SchemaConfig<string, int>
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

internal sealed class AsyncRejects<T> : Schema<T, T> where T : notnull
{
    internal override Outcome<T> Evaluate(T input, ParseContext context) =>
        throw new InvalidOperationException(
            "This double exists to prove the asynchronous path is taken.");

    internal override async ValueTask<Outcome<T>> EvaluateAsync(
        T input,
        ParseContext context,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        return Outcome<T>.Failed(
            Violations.One(
                context,
                ViolationCodeCatalog.Codes.Malformed,
                "Rejected {Path} asynchronously.",
                input));
    }
}

internal sealed class Counting<T> : Schema<T, T> where T : notnull
{
    private readonly Schema<T, T> _inner;

    internal Counting(Schema<T, T> inner)
    {
        _inner = inner;
    }

    internal int Evaluations { get; private set; }

    internal override Outcome<T> Evaluate(T input, ParseContext context)
    {
        Evaluations++;

        return _inner.Evaluate(input, context);
    }

    internal override ValueTask<Outcome<T>> EvaluateAsync(
        T input,
        ParseContext context,
        CancellationToken cancellationToken)
    {
        Evaluations++;

        return _inner.EvaluateAsync(input, context, cancellationToken);
    }
}

/// <summary>
/// A schema declared the way a consumer declares one, so that a rule inside it
/// is reached through <c>SchemaConfig.Evaluate</c> rather than directly. The
/// hand-written <c>Configure</c> body stands in for the generated ladder.
/// </summary>
internal sealed class NestedSecret : SchemaConfig<string, string>
{
    protected override Result<string, SchemaViolation> Configure(string subject)
    {
        FieldAccumulator fields = FieldAccumulator.Start();

        string token = fields.Take(
            Schema.Required(subject, new Rejects<string>()));

        return fields.HasViolations
            ? fields.Failed<string>()
            : Result.Ok<string, SchemaViolation>(token);
    }
}
