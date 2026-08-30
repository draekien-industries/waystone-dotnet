using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Guides;

/// <summary>guides/async.md</summary>
internal static class AsyncGuide
{
    internal sealed record Character(string Name);

    internal static async Task<Character> OneAwaitAtTheEnd(string id) =>
        // no intermediate awaits, one await at the end
        await SummonCharacterAsync(id)
            .MapAsync(c => EnrichAsync(c))
            .UnwrapOrAsync(Commoner);

    internal static async Task<Character> TheSameChainStepByStep(string id)
    {
        Option<Character> fetched = await SummonCharacterAsync(id);
        Option<Character> enriched = await fetched.MapAsync(c => EnrichAsync(c));

        return enriched.UnwrapOr(Commoner);
    }

    internal static ValueTask<string> MatchAsyncOnAResult(
        Result<Character, Error> result) =>
        result.MatchAsync(
            async x => await RenderAsync(x),
            async e => await DescribeAsync(e));

    internal static async Task RunTwoInParallel(
        Option<string> a,
        Option<string> b)
    {
        await Task.WhenAll(
            a.MapAsync(FetchAsync).AsTask(),
            b.MapAsync(FetchAsync).AsTask());
    }

    internal static async Task<Option<Character>> OptionTryAsync(string id) =>
        await Option.TryAsync(() => SummonCharacterOrThrowAsync(id));

    internal static async Task ResultTryAsync(string id)
    {
        // supply your own error type
        Result<Character, string> result = await Result.TryAsync(
            asyncFactory: () => SummonCharacterOrThrowAsync(id),
            onError: ex => ex.Message);

        // or let the error type default to Error
        Result<Character, Error> builtIn =
            await Result.TryAsync<Character>(() => SummonCharacterOrThrowAsync(id));

        _ = (result, builtIn);
    }

    internal static async Task MixingSyncAndAsyncBranches(Option<int> option)
    {
        // both branches async
        string text = await option.MatchAsync(
            async x => await RenderNumberAsync(x),
            async () => await LoadDefaultAsync());

        // only the Some branch is async
        string fromSome = await option.MatchAsync(
            async x => await RenderNumberAsync(x),
            () => "none");

        // only the None branch is async
        string fromNone = await option.MatchAsync(
            x => x.ToString(),
            async () => await LoadDefaultAsync());

        _ = (text, fromSome, fromNone);
    }

    // The page also shows a MatchAsync on a Result whose Err branch is
    // synchronous, to say it does not compile. There is nothing to pin here:
    // a sample that fails to build is the claim itself, and this project would
    // stop building if it were added.

    internal static async Task ConsumingAnOptionAsync(string id)
    {
        Character character = await SummonCharacterAsync(id).UnwrapAsync();
        Character orCommoner = await SummonCharacterAsync(id).UnwrapOrAsync(Commoner);
        Character? orDefault = await SummonCharacterAsync(id).UnwrapOrDefaultAsync();
        Character expected = await SummonCharacterAsync(id).ExpectAsync("the character must exist");

        _ = (character, orCommoner, orDefault, expected);
    }

    internal static async Task ConsumingAResultAsync(string id)
    {
        Character character = await LoadCharacterAsync(id).UnwrapAsync();
        Character orCommoner = await LoadCharacterAsync(id).UnwrapOrAsync(Commoner);
        Character? orDefault = await LoadCharacterAsync(id).UnwrapOrDefaultAsync();
        Character expected = await LoadCharacterAsync(id).ExpectAsync("the character must exist");

        Error error = await LoadCharacterAsync(id).UnwrapErrAsync();
        Error expectedErr = await LoadCharacterAsync(id).ExpectErrAsync("the load must fail");

        _ = (character, orCommoner, orDefault, expected, error, expectedErr);
    }

    private static ValueTask<Option<Character>> SummonCharacterAsync(string id) =>
        new(Option.Some(new Character("Fjord")));

    private static ValueTask<Result<Character, Error>> LoadCharacterAsync(string id) =>
        new(Result.Ok<Character, Error>(new Character("Fjord")));

    private static Task<Character> SummonCharacterOrThrowAsync(string id) =>
        Task.FromResult(new Character("Fjord"));

    private static Task<Character> EnrichAsync(Character character) =>
        Task.FromResult(character);

    private static ValueTask<string> RenderAsync(Character character) =>
        new(character.Name);

    private static ValueTask<string> RenderNumberAsync(int value) =>
        new(value.ToString());

    private static ValueTask<string> DescribeAsync(Error error) =>
        new(error.Message);

    private static ValueTask<string> LoadDefaultAsync() => new("default");

    private static Task<string> FetchAsync(string value) => Task.FromResult(value);

    // UnwrapOrAsync takes a value, not a factory. The page writes
    // UnwrapOrAsync(Guest), so Guest has to be a value there too.
    private static readonly Character Commoner = new("Commoner");
}
