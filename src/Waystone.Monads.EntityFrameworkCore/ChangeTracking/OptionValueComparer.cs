namespace Microsoft.EntityFrameworkCore.ChangeTracking;

using Waystone.Monads.Options;

/// <summary>
/// Compares two <see cref="Option{T}" /> values by case and held value, so
/// change tracking notices a property moving between some and none.
/// </summary>
/// <remarks>
/// Register this alongside one of the option converters. Without it, Entity
/// Framework Core builds a comparer of its own for the converted property, and
/// that comparer is not guaranteed to reach the record equality
/// <see cref="Option{T}" /> already has — a property reassigned from
/// <c>Some(1)</c> to <c>Some(2)</c> can then go unnoticed and never reach the
/// database.
/// <para>
/// One class covers both reference and value types, unlike the converters. A
/// comparer names only the model type, so nothing here depends on how <c>T?</c>
/// resolves.
/// </para>
/// <para>
/// The snapshot is the option itself rather than a copy. <see cref="Option{T}" />
/// is an immutable record, so nothing can mutate the snapshot out from under the
/// change tracker.
/// </para>
/// </remarks>
/// <typeparam name="T">The type held by a some option.</typeparam>
public sealed class OptionValueComparer<T> : ValueComparer<Option<T>>
    where T : notnull
{
    /// <summary>
    /// Creates a comparer that defers to the record equality of
    /// <see cref="Option{T}" /> and snapshots by reference.
    /// </summary>
    public OptionValueComparer()
        : base(
            (left, right) => object.Equals(left, right),
            option => option == null ? 0 : option.GetHashCode(),
            option => option)
    {
    }
}
