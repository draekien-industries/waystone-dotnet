using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Reference.ResultApi;

/// <summary>reference/result/consume.md</summary>
internal static class ResultConsume
{
    internal sealed record Adventurer(Uri Portrait);

    internal sealed record Loadout(string Weapon);

    private static class ErrorCodes
    {
        internal const string MissingName = "name.missing";

        internal const string MalformedDateTime = "date.malformed";
    }

    internal static void StateChecks()
    {
        Result<DateTime, Error> safeParseResult = SafeParse("2025-01-01");

        _ = safeParseResult.IsOk;  // true
        _ = safeParseResult.IsErr; // false
    }

    internal static void IsOkAnd()
    {
        Result<DateTime, Error> safeParseResult = SafeParse("2025-01-01");

        _ = safeParseResult.IsOkAnd(dateTime => dateTime > new DateTime(2024, 1, 1)); // true
    }

    internal static void IsErrAnd()
    {
        Result<DateTime, Error> failed = SafeParse("2025");
        //                      ^? Err(new Error(ErrorCodes.MalformedDateTime, "not a date"))

        _ = failed.IsErrAnd(error => error.Code == ErrorCodes.MalformedDateTime); // true
    }

    internal static void Match()
    {
        Result<string, string> nameResult = Result.Ok<string, string>("Sam");

        int length = nameResult.Match(
            name => name.Length,
            _ => 0);

        _ = length;
    }

    internal static void Deconstructing(Result<string, string> nameResult)
    {
        if (nameResult is Ok<string, string>(var name)) { _ = name; }
        if (nameResult is Err<string, string>(var error)) { _ = error; }
    }

    internal static void Unwrap()
    {
        Result<string, string> nameResult = Result.Ok<string, string>("Danny");
        string name = nameResult.Unwrap();

        _ = name;
    }

    internal static void UnwrapErr()
    {
        Result<int, string> ok = Result.Ok<int, string>(10);
        ok.UnwrapErr(); // throws UnwrapException

        Result<int, string> err = Result.Err<int, string>("Error");
        err.UnwrapErr(); // returns "Error"
    }

    internal static void UnwrapOr()
    {
        Result<string, Error> nameResult =
            Result.Err<string, Error>(
                new Error(ErrorCodes.MissingName, "no name was supplied"));

        string name = nameResult.UnwrapOr("Unknown");
        //     ^? "Unknown"

        _ = name;
    }

    internal static void UnwrapOrElse()
    {
        Result<Loadout, Error> getLoadoutResult = GetLoadout("Ashton");
        //                     ^? Err<Loadout, Error>

        Loadout loadout = getLoadoutResult.UnwrapOrElse(error => GenerateDefaultLoadout());
        //      ^? generated loadout

        _ = loadout;
    }

    internal static void UnwrapOrDefault()
    {
        Result<int, string> numberResult = Result.Err<int, string>("Error");
        int number = numberResult.UnwrapOrDefault();
        //  ^? 0

        _ = number;
    }

    internal static void UnwrapOrNull()
    {
        Result<int, string> countResult = Result.Err<int, string>("Error");
        int? count = countResult.UnwrapOrNull();
        //   ^? null

        _ = count;
    }

    internal static void Expect()
    {
        Result<string, string> nameResult = Result.Ok<string, string>("Pelor");
        string name = nameResult.Expect("Expected a name, but got an error");

        _ = name;
    }

    internal static void ExpectErr()
    {
        Result.Ok<int, string>(10).ExpectErr("Must be error");
        // throws UnmetExpectationException with message "Must be error"
    }

    internal static void MapOr()
    {
        Result<string, string> nameResult = Result.Err<string, string>("Error");

        int length = nameResult.MapOr(0, name => name.Length);
        //  ^? 0

        _ = length;
    }

    internal static void MapOrElse()
    {
        Result<Adventurer, Error> getAdventurerResult = GetAdventurer("Changebringer");

        Uri portrait = getAdventurerResult.MapOrElse(
            error => GeneratePortrait(),
            adventurer => adventurer.Portrait);

        _ = portrait;
    }

    internal static void MapOrDefault()
    {
        Result<string, string> nameResult = Result.Err<string, string>("Error");
        int length = nameResult.MapOrDefault(name => name.Length);
        //  ^? 0

        _ = length;
    }

    internal static void MapOrNull()
    {
        Result<string, string> nameResult = Result.Err<string, string>("Error");
        int? length = nameResult.MapOrNull(name => name.Length);
        //   ^? null, where MapOrDefault would have given you 0

        _ = length;
    }

    private static Result<DateTime, Error> SafeParse(string input) =>
        DateTime.TryParse(input, out DateTime parsed)
            ? Result.Ok<DateTime, Error>(parsed)
            : Result.Err<DateTime, Error>(
                new Error(ErrorCodes.MalformedDateTime, "not a date"));

    private static Result<Loadout, Error> GetLoadout(string name) =>
        Result.Err<Loadout, Error>(new Error("loadout.missing", "no loadout"));

    private static Result<Adventurer, Error> GetAdventurer(string name) =>
        Result.Ok<Adventurer, Error>(
            new Adventurer(new Uri("https://example.test/portrait.png")));

    private static Uri GeneratePortrait() =>
        new("https://example.test/generated.png");

    private static Loadout GenerateDefaultLoadout() => new("Rusty dagger");
}
