namespace Waystone.Monads.Schemas;

using System;
using Waystone.Monads.Options;

internal sealed class ForbiddenField<T> : Field<Checked> where T : notnull
{
    private readonly string _message;
    private readonly string _name;
    private readonly Option<T> _value;

    internal ForbiddenField(Option<T> value, string name, string message)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _name = name;
        _message = message ?? throw new ArgumentNullException(nameof(message));
    }

    internal override Outcome<Checked> EvaluateValue(ParseContext context)
    {
        ParseContext child = context.At(_name);

        return _value is Some<T>(T present)
            ? Outcome<Checked>.Failed(
                Violations.One(
                    child,
                    ViolationCodeCatalog.Codes.NotAllowed,
                    _message,
                    present))
            : Outcome<Checked>.Passed(Checked.Instance);
    }

    internal override void OnlyThisAssemblyMayDerive()
    {
    }
}
