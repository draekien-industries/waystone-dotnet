using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Core.Sample.Reference;

/// <summary>
/// reference/option/creation.md and reference/result/creation.md. The source
/// page covers both types under one heading, so this file does too. Split it
/// when the two reference pages are written.
/// </summary>
internal static class Creation
{
    internal sealed record Adventurer(string Name, Uri Portrait);

    internal static void OptionFactories()
    {
        Option<string> some = Option.Some("Hello Bees!");
        Option<string> none = Option.None<string>();

        _ = (some, none);
    }

    internal static void ResultFactories()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<int, string> err = Result.Err<int, string>("Something went wrong...");

        _ = (ok, err);
    }

    internal static void ResultFactoriesDefaultingToError()
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

    internal static void OptionTry()
    {
        Option<Adventurer> maybeAdventurer = Option.Try(() => GetCurrentAdventurer());

        _ = maybeAdventurer;
    }

    internal static void ResultTryWithYourOwnErrorType()
    {
        Result<Adventurer, string> result = Result.Try(
            factory: () => GetCurrentAdventurer(),
            onError: ex => ex.Message);

        _ = result;
    }

    internal static void ResultTryDefaultingToError()
    {
        Result<Adventurer, Error> result = Result.Try<Adventurer>(
            () => GetCurrentAdventurer());

        _ = result;
    }

    internal static void PassingStateToTheFactory(string text)
    {
        Option<int> parsed = Option.Try(text, static value => int.Parse(value));

        Result<int, Error> result = Result.Try(text, static value => int.Parse(value));

        _ = (parsed, result);
    }

    private static Adventurer GetCurrentAdventurer() =>
        new("Chetney", new Uri("https://example.test/chetney.png"));
}

/// <summary>reference/*/creation.md — the catalog the creation page marks up</summary>
[ErrorCodeCatalog]
public enum PartyErrors
{
    NotFound,
}
