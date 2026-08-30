using Waystone.Monads.Options;

namespace Waystone.Monads.Docs.Core.Sample.Reference.OptionApi;

/// <summary>reference/option/creation.md</summary>
internal static class OptionCreation
{
    internal sealed record Adventurer(string Name, Uri Portrait);

    internal static void Factories()
    {
        Option<string> some = Option.Some("Hello Bees!");
        Option<string> none = Option.None<string>();

        _ = (some, none);
    }

    internal static void FromNullable(string? sigil)
    {
        Option<string> maybeSigil = Option.FromNullable(sigil);

        _ = maybeSigil;
    }

    internal static void Try()
    {
        Option<Adventurer> maybeAdventurer = Option.Try(() => GetCurrentAdventurer());

        _ = maybeAdventurer;
    }

    internal static void ADefaultValueIsFine()
    {
        Option<int> zero = Option.Try(() => 0);
        //          ^? Some(0)

        _ = zero;
    }

    internal static async Task TryAsync()
    {
        Option<Adventurer> maybeAdventurer =
            await Option.TryAsync(() => GetCurrentAdventurerAsync());

        _ = maybeAdventurer;
    }

    internal static void PassingStateToTheFactory(string text)
    {
        Option<int> parsed = Option.Try(text, static value => int.Parse(value));

        _ = parsed;
    }

    private static Adventurer GetCurrentAdventurer() =>
        new("Chetney", new Uri("https://example.test/chetney.png"));

    private static Task<Adventurer> GetCurrentAdventurerAsync() =>
        Task.FromResult(GetCurrentAdventurer());
}
