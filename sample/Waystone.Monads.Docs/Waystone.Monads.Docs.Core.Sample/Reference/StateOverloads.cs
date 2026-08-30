using Waystone.Monads.Options;
using Waystone.Monads.Results;

namespace Waystone.Monads.Docs.Core.Sample.Reference;

/// <summary>reference/state-overloads.md</summary>
internal static class StateOverloads
{
    internal static void OnAResult(Result<int, string> result, int fallback)
    {
        result.UnwrapOrElse(fallback, static (error, state) => state);
    }

    internal static void OnAnOption(Option<string> option, string fallback)
    {
        option.UnwrapOrElse(fallback, static state => state);
    }

    internal static void MapWithState(Option<int> option, int offset)
    {
        // the compiler rejects any capture in here
        option.Map(offset, static (value, state) => value + state);
    }

    internal static void MapOrElseThreadsStateThroughBothDelegates(
        Option<int> option,
        int fallback)
    {
        option.MapOrElse(
            fallback,
            static state => state,
            static (value, state) => value + state);
    }
}
