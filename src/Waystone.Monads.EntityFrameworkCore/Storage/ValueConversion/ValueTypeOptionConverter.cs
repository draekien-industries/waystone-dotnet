namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;

/// <summary>
/// Maps an <see cref="Option{T}" /> over a value type onto a single nullable
/// column, with <see langword="null" /> standing for the none case.
/// </summary>
/// <remarks>
/// Pair this with <see cref="ReferenceTypeOptionConverter{T}" />, which covers
/// reference types. This one writes through <c>UnwrapOrNull</c> rather than
/// <c>UnwrapOrDefault</c>, because <c>Option&lt;int&gt;.UnwrapOrDefault()</c>
/// returns <c>0</c> for both <c>Some(0)</c> and <c>None</c> — a value type has no
/// null to fall back to. Lifting to <c>Nullable&lt;T&gt;</c> instead is what
/// keeps <c>Some(0)</c>, <c>Some(false)</c> and <c>Some(default(TEnum))</c>
/// distinguishable from a none option in the column.
/// <para>
/// A converted property must also be marked optional, or the provider emits a
/// <c>NOT NULL</c> column and saving a none option fails at the database rather
/// than at the model. <see cref="ModelBuilderExtensions" /> does this for you;
/// a hand-written registration has to call <c>IsRequired(false)</c> itself.
/// </para>
/// <para>
/// Registering this converter does not make <see cref="Option{T}" /> queryable.
/// A comparison against a captured option translates; a member access such as
/// <c>IsSome</c> inside a query expression does not, and throws at translation
/// time. The package README carries the forms that work.
/// </para>
/// </remarks>
/// <typeparam name="T">The value type held by a some option.</typeparam>
/// <example>
/// Registering the converter by hand, for a property the model-wide sweep should
/// not configure:
/// <code>
/// modelBuilder.Entity&lt;Person&gt;()
///             .Property(person => person.Age)
///             .HasConversion(
///                 new ValueTypeOptionConverter&lt;int&gt;(),
///                 new OptionValueComparer&lt;int&gt;())
///             .IsRequired(false);
/// </code>
/// </example>
public sealed class ValueTypeOptionConverter<T> : ValueConverter<Option<T>, T?>
    where T : struct
{
    /// <summary>
    /// Creates a converter that writes the held value of a some option and
    /// <see langword="null" /> for a none option.
    /// </summary>
    public ValueTypeOptionConverter()
        : base(
            option => option == null ? null : option.UnwrapOrNull(),
            value => Option.FromNullable(value))
    {
    }

    /// <inheritdoc
    ///     cref="ReferenceTypeOptionConverter{T}.ConvertsNulls" />
    public override bool ConvertsNulls => true;
}
