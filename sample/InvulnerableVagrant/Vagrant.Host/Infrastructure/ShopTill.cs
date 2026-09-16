namespace Vagrant.Host.Infrastructure;

using Vagrant.SharedKernel;

/// <summary>What the shop can pay out in coin.</summary>
/// <remarks>
/// <para>
/// A constant, not a ledger. The sample models what the shop sells and what it takes in,
/// and a till that goes up and down with every settlement is a fifth bounded context
/// teaching nothing the other four do not.
/// </para>
/// <para>
/// It is here rather than on the request because a patron cannot be asked how much money
/// the shop has. Pumat keeps two thousand in coin and the rest of the shop's worth on the
/// shelves, which is why something offered three thousand gold is a refusal a patron is
/// owed the reason for rather than a payment that silently falls short.
/// </para>
/// </remarks>
internal static class ShopTill
{
    /// <summary>What Pumat keeps behind the counter.</summary>
    public static Coin Holding => Coin.FromGold(2000);
}
