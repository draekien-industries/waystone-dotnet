using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Reference;

/// <summary>
/// reference/state-overloads.md. Both spellings of the same idea live here on
/// purpose: the page documents the <c>TState</c> overloads and the chainable
/// <c>With</c> form side by side, because the overloads stay supported and are
/// cheaper for a single call.
/// </summary>
internal static class StateOverloads
{
    internal sealed record Quest(string Name, int GoldReward);

    internal static void OnAResult(Result<int, string> result, int fallback)
    {
        #region state-overloads-the-state-overload-on-a-result
        result.UnwrapOrElse(fallback, static (error, state) => state);
        #endregion
    }

    internal static void OnAnOption(Option<string> option, string fallback)
    {
        #region state-overloads-the-state-overload-on-an-option
        option.UnwrapOrElse(fallback, static state => state);
        #endregion
    }

    internal static void MapWithState(Option<int> reward, int partySize)
    {
        #region state-overloads-the-state-overload
        // the compiler rejects any capture in here
        reward.Map(partySize, static (gold, party) => gold / party);
        #endregion
    }

    internal static void MapOrElseThreadsStateThroughBothDelegates(
        Option<int> option,
        int fallback)
    {
        #region state-overloads-map-or-else
        option.MapOrElse(
            fallback,
            static state => state,
            static (value, state) => value + state);
        #endregion
    }

    internal static void BindTheStateInstead(Option<int> reward, int partySize)
    {
        #region state-overloads-with-map
        Option<int> share = reward
            .With(partySize)
            .Map(static (gold, party) => gold / party);
        #endregion

        _ = share;
    }

    internal static void ABranchWithNoValueTakesTheStateAlone(
        Option<int> reward,
        int fallback)
    {
        #region state-overloads-with-no-value-branch
        // None has no value to hand over, so the delegate takes the state alone
        int gold = reward.With(fallback).UnwrapOrElse(static state => state);
        #endregion

        _ = gold;
    }

    internal static void PackMoreThanOneValueIntoATuple(
        Option<Quest> quest,
        int partySize,
        string fallback)
    {
        #region state-overloads-with-a-tuple
        // C# names the members after the variables, so nothing is invented
        string summary = quest
            .With((partySize, fallback))
            .Match(
                static (found, state) =>
                    $"{found.Name} splits {found.GoldReward / state.partySize}",
                static state => state.fallback);
        #endregion

        _ = summary;
    }

    internal static void TheStateDoesNotStick(
        Option<int> reward,
        int partySize,
        int bonus)
    {
        #region state-overloads-with-does-not-stick
        Option<int> total = reward
            .With(partySize)
            .Map(static (gold, party) => gold / party) // the state is spent here
            .Filter(static share => share > 0)         // so this is an ordinary call
            .With(bonus)                               // bind again to spend more
            .Map(static (share, extra) => share + extra);
        #endregion

        _ = total;
    }

    internal static async Task<Option<int>> AnAsyncDelegateTakesStateToo(
        Option<Quest> quest,
        int partySize)
    {
        #region state-overloads-with-async
        Option<int> share = await quest
            .With(partySize)
            .MapAsync(static async (found, party) => await ShareOf(found, party));
        #endregion

        return share;
    }

    internal static void TheStaticFactoriesBindToo(string entry)
    {
        #region state-overloads-with-a-factory
        Option<int> gold = Option.With(entry).Try(static text => int.Parse(text));
        #endregion

        _ = gold;
    }

    internal static void OnAResultTheErrorSideBindsAsWell(
        Result<Quest, string> attempt,
        string realm)
    {
        #region state-overloads-with-on-a-result
        Result<Quest, string> tagged = attempt
            .With(realm)
            .MapErr(static (error, where) => $"{where}: {error}");
        #endregion

        _ = tagged;
    }

    private static Task<int> ShareOf(Quest quest, int partySize) =>
        Task.FromResult(quest.GoldReward / partySize);
}
