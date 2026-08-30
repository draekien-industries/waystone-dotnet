namespace Waystone.Monads.PreviousMajor.Sample;

using System.Collections.Generic;
using System.Threading.Tasks;
using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;

/// <summary>
/// The two members that returned <c>Task</c> up to 6.x — <c>TryAsync</c> and
/// <c>CollectAsync</c> — held at the shapes a v6 consumer could write. <c>DRA-115</c>
/// moves both to <c>ValueTask</c> so that the first link in a chain is composable,
/// and claims the break is <c>CS0029</c> for a caller who names the type.
/// </summary>
/// <remarks>
/// Every member here names <c>Task</c> explicitly, because that is the only way to
/// break. A caller who awaits the call directly, or holds it in a <c>var</c>, is
/// unaffected — which is most callers, and is why this break is narrower than the
/// row count suggests.
/// </remarks>
internal static class AsyncFactories
{
    internal static async Task<int> IntoALocal()
    {
        Task<Option<int>> pending = Option.TryAsync(() => Task.FromResult(42));

        Option<int> option = await pending;

        return option.UnwrapOr(0);
    }

    internal static Task<Result<int, string>> FromAMethodReturn() =>
        Result.TryAsync(() => Task.FromResult(42), exception => exception.Message);

    internal static async Task<int> Gathered(IAsyncEnumerable<Option<int>> options)
    {
        Task<Option<IReadOnlyList<int>>> pending = options.CollectAsync();

        Option<IReadOnlyList<int>> collected = await pending;

        return collected.MapOr(0, values => values.Count);
    }
}
