namespace Vagrant.Catalog;

using Waystone.Monads.Results.Errors;

/// <summary>Every way a request to the shop's stock can be refused.</summary>
/// <remarks>
/// <para>
/// The generated codes read <c>vagrant.catalog.floor_above_asking</c>,
/// <c>vagrant.catalog.not_stocked</c> and <c>vagrant.catalog.not_enough_on_hand</c>. A
/// code is derived from the member's own name, so renaming a member here changes what a
/// patron's client sees.
/// </para>
/// <para>
/// Internal, and reachable from <c>Vagrant.Host</c> only through
/// <c>InternalsVisibleTo</c>. The host reads
/// <c>CatalogErrorCatalog.Codes.NotEnoughOnHand</c> to choose a status code; nothing
/// branches on the message.
/// </para>
/// <para>
/// Named <c>CatalogError</c> rather than <c>Catalog</c> because a type sharing its name
/// with its namespace loses the lookup to the namespace.
/// </para>
/// </remarks>
[ErrorCodeCatalog(Format = "vagrant.catalog.{member:snake}")]
internal enum CatalogError
{
    /// <summary>The floor price named was above the asking price.</summary>
    FloorAboveAsking,

    /// <summary>The shop holds no line of stock under that identifier.</summary>
    NotStocked,

    /// <summary>Fewer were on hand than the withdrawal asked for.</summary>
    NotEnoughOnHand,
}
