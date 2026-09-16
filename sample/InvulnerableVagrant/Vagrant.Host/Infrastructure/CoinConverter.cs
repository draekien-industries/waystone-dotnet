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
    /// <summary>Creates the converter.</summary>
    /// <remarks>
    /// <see cref="Coin" /> counts copper in a <see cref="ulong" /> and SQLite's INTEGER
    /// is signed, so the column is <see cref="long" />. The conversion is checked: a
    /// price past <see cref="long.MaxValue" /> copper would otherwise be stored as a
    /// negative one, which is the state the type exists to make impossible.
    /// </remarks>
    public CoinConverter()
        : base(
            coin => checked((long)coin.InCopper()),
            copper => Coin.FromCopper(checked((ulong)copper)))
    {
    }
}
