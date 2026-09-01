namespace Waystone.Monads.Schemas;

using System;

internal sealed class ExtendField<T> : Field<Checked> where T : notnull
{
    private readonly Schema<T, T> _rules;
    private readonly T _subject;

    internal ExtendField(T subject, Schema<T, T> rules)
    {
        _subject = subject;
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    internal override Outcome<Checked> EvaluateValue(ParseContext context)
    {
        Outcome<T> outcome = _rules.Evaluate(_subject, context);

        return outcome.Violations.Count == 0
            ? Outcome<Checked>.Passed(Checked.Instance)
            : Outcome<Checked>.Failed(outcome.Violations);
    }

    internal override void OnlyThisAssemblyMayDerive()
    {
    }
}
