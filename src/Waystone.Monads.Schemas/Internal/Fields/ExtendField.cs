namespace Waystone.Monads.Schemas.Internal.Fields;

using System;

internal sealed class ExtendField<T> : Field<Checked> where T : notnull
{
    private readonly string? _name;
    private readonly Schema<T, T> _rules;
    private readonly T _subject;

    internal ExtendField(T subject, Schema<T, T> rules, string? name = null)
    {
        _subject = subject;
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        _name = name;
    }

    internal override Outcome<Checked> EvaluateValue(ParseContext context)
    {
        ParseContext scope = _name is null ? context : context.At(_name);

        return _rules.Evaluate(_subject, scope).ToChecked();
    }

    internal override Field<Checked> WithName(string name) =>
        new ExtendField<T>(_subject, _rules, name);

    internal override void OnlyThisAssemblyMayDerive()
    {
    }
}
