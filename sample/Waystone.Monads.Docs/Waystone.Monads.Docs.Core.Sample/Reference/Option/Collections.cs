using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Reference.OptionApi;

/// <summary>reference/option/nesting.md — the collection extensions</summary>
internal static class OptionCollections
{
    internal static void Filter()
    {
        List<Option<string>> collection =
        [
            Option.Some("Vex'ahlia"),
            Option.Some("Grog"),
            Option.None<string>(),
        ];

        IEnumerable<Option<string>> filtered = collection.Filter(x => x.StartsWith('V'));
        //                          ^? [Some("Vex'ahlia"), None, None]

        _ = filtered;
    }

    internal static void Map()
    {
        List<Option<string>> collection =
        [
            Option.Some("Vex'ahlia"),
            Option.Some("Grog"),
            Option.None<string>(),
        ];

        IEnumerable<Option<string>> mapped = collection.Map(x => $"{x} the Brave");
        //                          ^? [Some("Vex'ahlia the Brave"), Some("Grog the Brave"), None]

        _ = mapped;
    }

    internal static void Flatten()
    {
        List<Option<string>> collection =
        [
            Option.Some("Vex'ahlia"),
            Option.None<string>(),
            Option.Some("Grog"),
        ];

        IEnumerable<string> values = collection.Flatten();
        //                  ^? ["Vex'ahlia", "Grog"]

        _ = values;
    }

    internal static void Collect()
    {
        List<Option<string>> collection =
        [
            Option.Some("Vex'ahlia"),
            Option.Some("Grog"),
        ];

        Option<IReadOnlyList<string>> all = collection.Collect();
        //                            ^? Some(["Vex'ahlia", "Grog"])

        _ = all;
    }

    internal static void CollectStopsAtTheFirstGap()
    {
        List<Option<string>> withAGap =
        [
            Option.Some("Vex'ahlia"),
            Option.None<string>(),
            Option.Some("Grog"),
        ];

        Option<IReadOnlyList<string>> all = withAGap.Collect();
        //                            ^? None

        _ = all;
    }

    internal static async Task CollectAsync(
        IAsyncEnumerable<Option<string>> stream,
        CancellationToken cancellationToken)
    {
        Option<IReadOnlyList<string>> all =
            await stream.CollectAsync(cancellationToken);

        _ = all;
    }

    internal static void FirstOrNone()
    {
        List<Option<string>> collection =
        [
            Option.Some("Vex'ahlia"),
            Option.Some("Grog"),
        ];

        Option<string> first = collection.FirstOrNone(x => x.StartsWith('V'));
        //             ^? Some("Vex'ahlia")

        _ = first;
    }

    internal static void FirstOr()
    {
        List<Option<string>> collection =
        [
            Option.Some("Vex'ahlia"),
            Option.Some("Grog"),
        ];

        string first = collection.FirstOr(x => x.StartsWith('Z'), "Trinket");
        //     ^? "Trinket"

        _ = first;
    }

    internal static void FirstOrElse()
    {
        List<Option<string>> collection =
        [
            Option.Some("Vex'ahlia"),
            Option.Some("Grog"),
        ];

        string first = collection.FirstOrElse(x => x.StartsWith('Z'), () => "Trinket");
        //     ^? "Trinket"

        _ = first;
    }
}
