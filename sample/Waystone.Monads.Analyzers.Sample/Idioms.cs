#nullable enable

namespace Waystone.Monads.Analyzers.Sample;

using System;
using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;

internal class Idioms
{
    internal Option<string> MaybeNullPassedToSome(string? value) =>
        Option.Some(value);

    internal Option<string> ProjectionMayReturnNull(Option<int> option) =>
        option.Map(Describe);

    internal Option<string> ProjectionMayReturnNullFromALambda(
        Option<int> option) =>
        option.Map(value => Describe(value));

    private static string? Describe(int value) =>
        value > 0 ? value.ToString() : null;

    internal int Panics(Option<int> option) => option.Unwrap();

    internal int Expects(Option<int> option) => option.Expect("checked");

    internal Result<int, string> ThrowsFromAResultMember(int value)
    {
        if (value > 0)
        {
            return Result.Ok<int, string>(value);
        }

        throw new InvalidOperationException("not positive");
    }

    internal int GuardsThenUnwraps(Option<int> option)
    {
        if (option.IsSome)
        {
            return option.Unwrap();
        }

        return 0;
    }

    internal bool ChecksThenUnwraps(Option<int> option) =>
        option.IsSome && option.Unwrap() > 2;

    internal Option<int> MapsThenFlattens(Option<int> option) =>
        option.Map(value => Option.Some(value * 2)).Flatten();

    internal int UnwrapsOrADefault(Option<int> option) => option.UnwrapOr(0);

    internal bool ComparesToNull(Option<int> option) => option == null;

    internal Option<Option<int>> Nested() => Option.None<Option<int>>();

    internal Result<string, string> IdenticalTypeArguments() =>
        Result.Ok<string, string>("value");

    internal bool DeclaresACase(Some<int> some) => some.IsSome;

    internal string? NullableAlongsideOption(int id) => null;

    internal int UnwrapsOrDefaultOnAStruct(Option<int> option) =>
        option.UnwrapOrDefault();

    // The pair below is WM2017 before and after, and both halves belong in this
    // project rather than one of them in Waystone.Monads.Docs. The rule is on
    // here, so the second method asserts something the documentation samples
    // cannot: that the rewrite the message names actually silences the rule.
    // Watch the build output — one WM2017 for the whole pair, on the first.
    // Change one half and change the other, or the page shows a before and an
    // after that are not the same call.

    internal Option<int> CapturesInsteadOfBindingState(
        Option<int> reward,
        int partySize)
    {
        #region idioms-wm2017-capture
        Option<int> share = reward.Map(gold => gold / partySize);
        #endregion

        return share;
    }

    internal Option<int> BindsTheStateInstead(
        Option<int> reward,
        int partySize)
    {
        #region idioms-wm2017-bound
        Option<int> share = reward
            .With(partySize)
            .Map(static (gold, party) => gold / party);
        #endregion

        return share;
    }

    // One WM2023, on the first of the pair below. Unlike the WM2017 pair these
    // deliberately do not agree: the bound version treats an absent bonus as
    // zero, which is the mistake the rule reports. Keep them different, and
    // keep the page saying why.
    //
    // The bound version also carries a WM2007 and a WM2025, both on the
    // 'UnwrapOr' the delegate has to call by hand. That is the rule stack this
    // spelling earns rather than a defect in the sample: unwrapping inside a
    // delegate is a nested chain whatever it unwraps to.

    internal Option<int> BindsAnOptionAsState(
        Option<int> reward,
        Option<int> bonus)
    {
        #region idioms-wm2023-bound
        Option<int> haul = reward
            .With(bonus)
            .Map(static (gold, extra) => gold + extra.UnwrapOr(0));
        #endregion

        return haul;
    }

    internal Option<int> ZipsTheTwoInstead(
        Option<int> reward,
        Option<int> bonus)
    {
        #region idioms-wm2023-zipped
        Option<int> haul = reward.ZipWith(
            bonus,
            static (gold, extra) => gold + extra);
        #endregion

        return haul;
    }

    // One WM2024, on the first of the pair below. These two do produce the same
    // value — the difference is that the first allocates a delegate to defer a
    // literal that was already built. The fallback is 100 rather than 0 so the
    // second does not trip WM2007 as well.

    internal int DefersAValueAlreadyBuilt(Option<int> reward)
    {
        #region idioms-wm2024-deferred
        int gold = reward.UnwrapOrElse(() => 100);
        #endregion

        return gold;
    }

    internal int TakesTheValueDirectly(Option<int> reward)
    {
        #region idioms-wm2024-direct
        int gold = reward.UnwrapOr(100);
        #endregion

        return gold;
    }

    // One WM2025, on the 'Map' inside the delegate below. The pair produces the
    // same option; the difference is that the second gives the nested step a
    // name, a signature and somewhere to be called from.

    internal Option<string> NestsAChainInsideADelegate(Option<int> reward)
    {
        #region idioms-wm2025-nested
        Option<string> label = reward.AndThen(
            static gold => TierOf(gold).Map(tier => tier.ToUpperInvariant()));
        #endregion

        return label;
    }

    internal Option<string> ExtractsTheNestedChain(Option<int> reward)
    {
        #region idioms-wm2025-extracted
        Option<string> label = reward.AndThen(TierLabel);
        #endregion

        return label;
    }

    private static Option<string> TierLabel(int gold) =>
        TierOf(gold).Map(tier => tier.ToUpperInvariant());

    private static Option<string> TierOf(int gold) =>
        gold > 100 ? Option.Some("gold") : Option.None<string>();

    // One WM2026, on the first of the pair below. Both produce the same option.
    // The difference is what the signature claims: the first says the step can
    // be absent and then never is.

    internal Option<int> LiftsInsideAndThen(Option<int> reward)
    {
        #region idioms-wm2026-lifted
        Option<int> doubled = reward.AndThen(
            static gold => Option.Some(gold * 2));
        #endregion

        return doubled;
    }

    internal Option<int> ProjectsInstead(Option<int> reward)
    {
        #region idioms-wm2026-projected
        Option<int> doubled = reward.Map(static gold => gold * 2);
        #endregion

        return doubled;
    }
}
