namespace Waystone.Monads.Schemas;

using System;
using Waystone.Monads.Options;

internal sealed class RequiredField<TIn, TOut> : Field<TOut>
    where TIn : notnull where TOut : notnull
{
    private const string AbsentMessage = "Expected {Path} to be present.";

    private readonly string _message;
    private readonly string _name;
    private readonly Schema<TIn, TOut> _schema;
    private readonly Option<TIn> _value;

    internal RequiredField(
        Option<TIn> value,
        Schema<TIn, TOut> schema,
        string name,
        string? message)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        _name = name;
        _message = message ?? AbsentMessage;
    }

    internal override Outcome<TOut> EvaluateValue(ParseContext context)
    {
        ParseContext child = context.At(_name);

        return _value is Some<TIn>(TIn present)
            ? _schema.Evaluate(present, child)
            : Outcome<TOut>.Failed(
                Violations.One(
                    child,
                    ViolationCodeCatalog.Codes.Incomplete,
                    _message));
    }

    internal override void OnlyThisAssemblyMayDerive()
    {
    }
}
