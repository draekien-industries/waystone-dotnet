namespace Waystone.Monads.Schemas;

using System;
using Waystone.Monads.Options;

internal sealed class OptionalField<TIn, TOut> : Field<Option<TOut>>
    where TIn : notnull where TOut : notnull
{
    private readonly string _name;
    private readonly Schema<TIn, TOut> _schema;
    private readonly Option<TIn> _value;

    internal OptionalField(
        Option<TIn> value,
        Schema<TIn, TOut> schema,
        string name)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        _name = name;
    }

    internal override Outcome<Option<TOut>> EvaluateValue(ParseContext context)
    {
        if (_value is not Some<TIn>(TIn present))
        {
            return Outcome<Option<TOut>>.Passed(Option.None<TOut>());
        }

        Outcome<TOut> inner = _schema.Evaluate(present, context.At(_name));

        if (!inner.HasValue)
        {
            return Outcome<Option<TOut>>.Failed(inner.Violations);
        }

        Option<TOut> lifted = Option.Some(inner.Value);

        return inner.Violations.Count == 0
            ? Outcome<Option<TOut>>.Passed(lifted)
            : Outcome<Option<TOut>>.Refined(lifted, inner.Violations);
    }

    internal override void OnlyThisAssemblyMayDerive()
    {
    }
}
