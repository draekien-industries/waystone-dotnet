namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vagrant.Ordering;
using Vagrant.SharedKernel;

/// <summary>Stores an <see cref="AgreedPrice" /> as the coin it wraps.</summary>
/// <remarks>
/// A converter rather than a complex property over its <c>Coin</c>. The agreed price has
/// one component, so a nested mapping would buy a second layer of configuration for a
/// single column that <see cref="CoinConverter" /> already knows how to write.
/// </remarks>
internal sealed class AgreedPriceConverter : ValueConverter<AgreedPrice, long>
{
    /// <summary>Creates the converter.</summary>
    public AgreedPriceConverter()
        : base(
            price => checked((long)price.Settled.InCopper()),
            copper => new AgreedPrice(Coin.FromCopper(checked((ulong)copper))))
    {
    }
}
