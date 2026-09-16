namespace Vagrant.Host.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vagrant.SharedKernel;

/// <summary>Stores a <see cref="Coin" /> as a single count of copper pieces.</summary>
/// <remarks>
/// One column rather than four. Storing each denomination separately would let a row
/// hold twelve silver and one gold — two ways to write the same price, which every
/// query comparing prices would then have to reconcile.
/// </remarks>
internal sealed class CoinConverter : ValueConverter<Coin, long>
{
    public CoinConverter()
        : base(coin => coin.InCopper(), copper => Coin.FromCopper(copper))
    {
    }
}
