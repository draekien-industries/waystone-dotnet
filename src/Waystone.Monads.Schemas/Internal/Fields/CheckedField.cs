namespace Waystone.Monads.Schemas.Internal.Fields;

using System;

internal sealed class CheckedField<T> : Field<Checked> where T : notnull
{
    private readonly Field<T> _field;

    internal CheckedField(Field<T> field)
    {
        _field = field ?? throw new ArgumentNullException(nameof(field));
    }

    internal override Outcome<Checked> EvaluateValue(ParseContext context) =>
        _field.EvaluateValue(context).ToChecked();

    internal override Field<Checked> WithName(string name) =>
        new CheckedField<T>(_field.WithName(name));

    internal override void OnlyThisAssemblyMayDerive()
    {
    }
}
