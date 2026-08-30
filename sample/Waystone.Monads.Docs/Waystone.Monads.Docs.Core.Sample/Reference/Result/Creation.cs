using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Core.Sample.Reference.ResultApi;

/// <summary>reference/result/creation.md</summary>
internal static class ResultCreation
{
    internal sealed record Adventurer(string Name, Uri Portrait);

    internal static void Factories()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<int, string> err = Result.Err<int, string>("Something went wrong...");

        _ = (ok, err);
    }

    internal static void FactoriesDefaultingToError()
    {
        Result<int, Error> ok = Result.Ok<int>(1);
        Result<int, Error> err = Result.Err<int>(
            new Error("MyCode", "Something went wrong..."));

        _ = (ok, err);
    }

    internal static Result<Adventurer, Error> FromAGeneratedCatalog() =>
        Result.Err<Adventurer>(
            PartyErrorsCatalog.Errors.NotFound("The adventurer was not found"));

    internal static void OkRejectsNull()
    {
        Result.Ok<string, Error>(null!); // throws
    }

    internal static void ADefaultValueIsFine()
    {
        Result<int, string> zero = Result.Ok<int, string>(0);

        _ = zero;
    }

    internal static void TryWithYourOwnErrorType()
    {
        Result<Adventurer, string> result = Result.Try(
            factory: () => GetCurrentAdventurer(),
            onError: ex => ex.Message);

        _ = result;
    }

    internal static void TryDefaultingToError()
    {
        Result<Adventurer, Error> result = Result.Try<Adventurer>(
            () => GetCurrentAdventurer());

        _ = result;
    }

    internal static async Task TryAsync()
    {
        Result<Adventurer, string> result = await Result.TryAsync(
            asyncFactory: () => GetCurrentAdventurerAsync(),
            onError: ex => ex.Message);

        _ = result;
    }

    internal static void PassingStateToTheFactory(string text)
    {
        Result<int, Error> result = Result.Try(text, static value => int.Parse(value));

        _ = result;
    }

    private static Adventurer GetCurrentAdventurer() =>
        new("Chetney", new Uri("https://example.test/chetney.png"));

    private static Task<Adventurer> GetCurrentAdventurerAsync() =>
        Task.FromResult(GetCurrentAdventurer());
}

/// <summary>reference/result/creation.md — the catalog the page marks up</summary>
[ErrorCodeCatalog]
public enum PartyErrors
{
    NotFound,
}
