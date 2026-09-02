namespace Waystone.Monads.Schemas;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ConditionalSchema<T> : Schema<T, T> where T : notnull
{
    private readonly bool _appliesWhen;

    private readonly Schema<T, T> _inner;

    private readonly Func<T, bool> _predicate;

    internal ConditionalSchema(
        Schema<T, T> inner,
        Func<T, bool> predicate,
        bool appliesWhen)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));

        _predicate = predicate
                  ?? throw new ArgumentNullException(nameof(predicate));

        _appliesWhen = appliesWhen;
    }

    internal override Outcome<T> Evaluate(T input, ParseContext context) =>
        Applies(input)
            ? _inner.Evaluate(input, context)
            : Outcome<T>.Passed(input);

    internal override ValueTask<Outcome<T>> EvaluateAsync(
        T input,
        ParseContext context,
        CancellationToken cancellationToken) =>
        Applies(input)
            ? _inner.EvaluateAsync(input, context, cancellationToken)
            : new ValueTask<Outcome<T>>(Outcome<T>.Passed(input));

    private bool Applies(T input) => _predicate(input) == _appliesWhen;
}
