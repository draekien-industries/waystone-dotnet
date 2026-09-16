namespace Vagrant.Host.Contracts;

using Vagrant.SharedKernel;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

/// <summary>Turns counted-out coin into an amount, or says why it is not one.</summary>
/// <remarks>
/// <para>
/// Every denomination is optional and absent means none. A patron tendering only gold
/// sends only gold, rather than three zeroes to say what they are not holding.
/// </para>
/// <para>
/// One schema for every body that carries money, nested by
/// <see cref="MakeOfferSchema" />, <see cref="SettlePurchaseSchema" /> and
/// <see cref="OpenBuybackSchema" />. How coin is written on the wire is stated once, so
/// the three cannot drift apart.
/// </para>
/// </remarks>
internal sealed partial class CoinSchema : SchemaConfig<CoinRequest, Coin>
{
    /// <inheritdoc />
    protected override Result<Coin, SchemaViolation> Configure(CoinRequest subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        return Schema.Fields(
                          Denomination(subject.Platinum).Named("platinum"),
                          Denomination(subject.Gold).Named("gold"),
                          Denomination(subject.Silver).Named("silver"),
                          Denomination(subject.Copper).Named("copper"))
                     .Into(
                          (platinum, gold, silver, copper) => Coin.From(
                              Count(platinum),
                              Count(gold),
                              Count(silver),
                              Count(copper)));
    }

    /// <remarks>
    /// <c>Schema.Number</c> has no unsigned rule, so the bound is stated and the cast
    /// follows it. A negative count is refused here rather than wrapping into a fortune
    /// at the cast.
    /// </remarks>
    private static Field<Option<int>> Denomination(Option<int> value) =>
        Schema.Optional(value, Schema.Number.Int32.AtLeast(0));

    /// <remarks>
    /// <c>Match</c> rather than an unwrap with a fallback. Both of the unwraps that hand
    /// back a zero are reported here — one for hiding the fallback behind a default, the
    /// other for producing a zero indistinguishable from a real one — and both are right:
    /// a denomination nobody mentioned really is none of it, and saying so takes a branch
    /// that reads as the rule.
    /// </remarks>
    private static uint Count(Option<int> denomination) =>
        denomination.Match(static count => (uint)count, static () => 0u);
}
