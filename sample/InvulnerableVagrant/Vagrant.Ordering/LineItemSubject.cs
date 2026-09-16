namespace Vagrant.Ordering;

/// <summary>Identifies what a line of a purchase is for.</summary>
/// <remarks>
/// Ordering does not interpret the value. Naming Catalog's <c>StockedItemId</c> would
/// mean referencing <c>Vagrant.Catalog</c> from here, and a bare <c>Guid</c> would let
/// any identifier in the sample be put on a purchase as any other.
///
/// <c>Vagrant.Host</c> translates, the same way it will translate for Staffing's
/// <c>ErrandSubject</c>.
/// </remarks>
/// <param name="Value">The underlying identifier.</param>
public readonly record struct LineItemSubject(Guid Value)
{
    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
