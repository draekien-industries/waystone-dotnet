namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Waystone.Monads.Options;

/// <summary>
/// Maps an <see cref="Option{T}" /> over a reference type onto a single nullable
/// column, with <see langword="null" /> standing for the none case.
/// </summary>
/// <remarks>
/// Pair this with <see cref="ValueTypeOptionConverter{T}" />, which covers value
/// types. The two exist as separate classes because C# resolves
/// <c>T?</c> under a <c>notnull</c> constraint to <c>T</c> rather than to
/// <c>System.Nullable&lt;T&gt;</c>, so a single class would hand
/// <c>Option&lt;int&gt;</c> a non-nullable column and silently write a none
/// option as <c>0</c>. Reaching for the wrong one of the pair is a compile
/// error, not a data-loss bug.
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
/// <typeparam name="T">The reference type held by a some option.</typeparam>
/// <example>
/// Registering the converter by hand, for a property that needs a column name
/// the model-wide sweep would not give it:
/// <code>
/// modelBuilder.Entity&lt;Person&gt;()
///             .Property(person => person.Nickname)
///             .HasConversion(
///                 new ReferenceTypeOptionConverter&lt;string&gt;(),
///                 new OptionValueComparer&lt;string&gt;())
///             .IsRequired(false)
///             .HasColumnName("nick");
/// </code>
/// </example>
public sealed class ReferenceTypeOptionConverter<T> : ValueConverter<Option<T>, T?>
    where T : class
{
    /// <summary>
    /// Creates a converter that writes the held value of a some option and
    /// <see langword="null" /> for a none option.
    /// </summary>
    public ReferenceTypeOptionConverter()
        : base(
            option => option == null ? null : option.UnwrapOrDefault(),
            value => Option.FromNullable(value))
    {
    }

    /// <summary>
    /// Gets a value indicating that this converter handles
    /// <see langword="null" /> itself rather than letting the provider
    /// short-circuit it.
    /// </summary>
    /// <remarks>
    /// Entity Framework Core skips a converter for a <see langword="null" />
    /// column by default, which would materialise a <c>NULL</c> as a
    /// <see langword="null" /> property rather than as a none option — the one
    /// thing this converter exists to prevent. Overriding this to
    /// <see langword="true" /> is what routes <c>NULL</c> through
    /// <c>ConvertFromProvider</c>.
    /// </remarks>
    public override bool ConvertsNulls => true;
}
