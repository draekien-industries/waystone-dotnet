namespace Vagrant.Ordering;

using Waystone.Monads.Results.Errors;

/// <summary>Every way coin can refuse to move.</summary>
/// <remarks>
/// <para>
/// The generated codes read <c>vagrant.ordering.no_line_items</c>,
/// <c>vagrant.ordering.offer_below_floor</c> and so on. A code is derived from the
/// member's own name, so renaming a member here changes what a patron's client sees.
/// </para>
/// <para>
/// Internal, and reachable from <c>Vagrant.Host</c> only through
/// <c>InternalsVisibleTo</c>. The host reads
/// <c>OrderingErrorCatalog.Codes.InsufficientCoin</c> to choose a status code; nothing
/// branches on the message.
/// </para>
/// <para>
/// Named <c>OrderingError</c> rather than <c>Ordering</c> because a type sharing its name
/// with its namespace loses the lookup to the namespace.
/// </para>
/// </remarks>
[ErrorCodeCatalog(Format = "vagrant.ordering.{member:snake}")]
internal enum OrderingError
{
    /// <summary>A purchase was presented with nothing on it.</summary>
    NoLineItems,

    /// <summary>The same thing was presented twice on one purchase.</summary>
    DuplicateLine,

    /// <summary>The offer named was below the least the shop would take.</summary>
    OfferBelowFloor,

    /// <summary>The book holds no purchase under that identifier.</summary>
    NoSuchPurchase,

    /// <summary>The book holds no buyback under that identifier.</summary>
    NoSuchBuyback,

    /// <summary>A line was haggled over that the purchase does not carry.</summary>
    NotOnThisPurchase,

    /// <summary>The coin tendered fell short of the total.</summary>
    InsufficientCoin,

    /// <summary>The purchase has already been settled.</summary>
    PurchaseAlreadySettled,

    /// <summary>The buyback has already been settled.</summary>
    BuybackAlreadySettled,

    /// <summary>The till holds less than the shop offered.</summary>
    TillCannotCover,
}
