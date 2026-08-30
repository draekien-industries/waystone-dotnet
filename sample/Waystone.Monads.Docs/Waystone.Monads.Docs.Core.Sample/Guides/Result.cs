using System.Diagnostics;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Guides;

/// <summary>guides/result.md</summary>
internal static class ResultGuide
{
    internal sealed record Quest(int Id, string Name);

    internal sealed record Character(string Username);

    private static class ErrorCodes
    {
        internal const string MalformedDateTime = "date.malformed";
    }

    private interface ILogger
    {
        void LogInformation(string message, params object?[] args);

        void LogWarning(string message, params object?[] args);
    }

    private static readonly ILogger logger = null!;

    internal sealed record Reward(string Item);

    internal static void Creating()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<int, string> err = Result.Err<int, string>("Something went wrong");

        // TErr defaults to Error when you leave it off
        Result<int, Error> okWithDefaultError = Result.Ok<int>(1);
        Result<int, Error> errWithDefaultError =
            Result.Err<int>(new Error("quest.failed", "Something went wrong"));

        _ = (ok, err, okWithDefaultError, errWithDefaultError);
    }

    internal static void NeitherSideCanHoldNull()
    {
        Result.Ok<string, Error>(null!); // throws ArgumentNullException
    }

    internal static void ADefaultValueIsFine()
    {
        Result<int, string> zero = Result.Ok<int, string>(0);
        Result<Guid, string> empty = Result.Ok<Guid, string>(Guid.Empty);

        _ = (zero, empty);
    }

    internal static Result<Reward, Error> ChainingFallibleSteps(string name) =>
        FindCharacter(name)
            .AndThen(GetQuest)
            .AndThen(ClaimReward);

    internal static string MatchingOut(Result<Reward, Error> result) =>
        result.Match(
            reward => reward.Item,
            error => error.Message);

    internal static void UnwrappingOut(Result<Reward, Error> result)
    {
        Reward orFallback = result.UnwrapOr(new Reward("A handful of copper"));
        Reward orComputed = result.UnwrapOrElse(error => new Reward(error.Code));

        _ = (orFallback, orComputed);
    }

    internal static string PatternMatchingWithSwitch(Result<Reward, Error> result) =>
        result switch
        {
            Ok<Reward, Error>(var reward) => reward.Item,
            Err<Reward, Error>(var error) => error.Message,
            _ => throw new UnreachableException(),
        };

    internal static void PatternMatchingWithIf(Result<Reward, Error> result)
    {
        if (result is Err<Reward, Error>(var error))
        {
            logger.LogWarning("No reward: {Message}", error.Message);
        }
    }

    internal static void PrintingAndLogging(Error e)
    {
        _ = Result.Ok<int, Error>(1).ToString();  // "Ok { IsOk = True, IsErr = False }"
        _ = Result.Err<int, Error>(e).ToString(); // "Err { IsOk = False, IsErr = True }"
    }

    internal static Result<Quest, Error> Inspecting(int id) =>
        LoadQuest(id)
            .Inspect(q => logger.LogInformation("Loaded quest {Id}", q.Id))
            .InspectErr(e => logger.LogWarning("Load failed: {Code} {Message}", e.Code, e.Message));

    internal static void IsOkAnd()
    {
        Result<DateTime, Error> safeParseResult = SafeParse("2025-01-01");

        _ = safeParseResult.IsOkAnd(dateTime => dateTime > new DateTime(2024, 1, 1)); // true
    }

    internal static void IsErrAnd()
    {
        Result<DateTime, Error> safeParseResult = SafeParse("2025");
        //                      ^? Err<DateTime, Error>(
        //                             new Error(ErrorCodes.MalformedDateTime, "not a date"))

        _ = safeParseResult.IsErrAnd(error => error.Code == ErrorCodes.MalformedDateTime); // true
    }

    internal static Result<int, Error> MapErrThenFlatten() =>
        RollForName()                                             // Result<string, string>
            .MapErr(message => new Error("name.failed", message)) // Result<string, Error>
            .Map(name => CountRunes(name))                        // Result<Result<int, Error>, Error>
            .Flatten();                                           // Result<int, Error>

    internal static Result<int, Error> TheSameChainWithAndThen() =>
        RollForName()                                             // Result<string, string>
            .MapErr(message => new Error("name.failed", message)) // Result<string, Error>
            .AndThen(name => CountRunes(name));                   // Result<int, Error>

    internal static void ExpectErr()
    {
        Result<int, string> result = Result.Ok<int, string>(10);
        result.ExpectErr("Must be error"); // throws UnmetExpectationException with message "Must be error"
    }

    internal static void UnwrapErr()
    {
        Result<int, string> ok = Result.Ok<int, string>(10);
        ok.UnwrapErr(); // throws UnwrapException

        Result<int, string> err = Result.Err<int, string>("Error");
        err.UnwrapErr(); // returns "Error"
    }

    internal static Result<string, string> ChainingBothHalves() =>
        FindCharacter("Percy")
            .InspectErr(err => Console.WriteLine($"Find character failed: {err.Message}"))
            .Map(character => character.Username)
            .MapErr(err => err.Message);

    internal static void And()
    {
        {
            var x = Result.Ok<int, string>(1);
            var y = Result.Err<int, string>("late error");
            Debug.Assert(x.And(y) == Result.Err<int, string>("late error"));
        }

        {
            var x = Result.Err<int, string>("early error");
            var y = Result.Ok<int, string>(1);
            Debug.Assert(x.And(y) == Result.Err<int, string>("early error"));
        }

        {
            var x = Result.Err<int, string>("early error");
            var y = Result.Err<int, string>("late error");
            Debug.Assert(x.And(y) == Result.Err<int, string>("early error"));
        }

        {
            var x = Result.Ok<int, string>(1);
            var y = Result.Ok<int, string>(2);
            Debug.Assert(x.And(y) == Result.Ok<int, string>(2));
        }
    }

    internal static void AndThen()
    {
        Result<int, string> two = Result.Ok<int, string>(2);
        Debug.Assert(two.AndThen(SquareThenToString) == Result.Ok<string, string>("4"));

        Result<int, string> big = Result.Ok<int, string>(int.MaxValue);
        Debug.Assert(big.AndThen(SquareThenToString) == Result.Err<string, string>("overflow"));

        Result<int, string> nan = Result.Err<int, string>("NaN");
        Debug.Assert(nan.AndThen(SquareThenToString) == Result.Err<string, string>("NaN"));
    }

    internal static void Or()
    {
        {
            var x = Result.Ok<int, string>(1);
            var y = Result.Ok<int, string>(2);
            Debug.Assert(x.Or(y) == Result.Ok<int, string>(1));
        }

        {
            var x = Result.Ok<int, string>(1);
            var y = Result.Err<int, string>("error");
            Debug.Assert(x.Or(y) == Result.Ok<int, string>(1));
        }

        {
            var x = Result.Err<int, string>("error");
            var y = Result.Ok<int, string>(1);
            Debug.Assert(x.Or(y) == Result.Ok<int, string>(1));
        }

        {
            var x = Result.Err<int, string>("error 1");
            var y = Result.Err<int, string>("error 2");
            Debug.Assert(x.Or(y) == Result.Err<int, string>("error 2"));
        }
    }

    internal static void OrElse()
    {
        Result<int, string> two = Result.Ok<int, string>(2);
        Debug.Assert(two.OrElse(Recover) == two);

        Result<int, string> nan = Result.Err<int, string>("NaN");
        Debug.Assert(nan.OrElse(Recover) == Result.Ok<int, string>(0));

        Result<int, string> overflow = Result.Err<int, string>("overflow");
        Debug.Assert(overflow.OrElse(Recover) == overflow);
    }

    // OrElse runs on the Err half, so its factory takes the error — not the ok value.
    private static Result<int, string> Recover(string error) =>
        error == "NaN"
            ? Result.Ok<int, string>(0)
            : Result.Err<int, string>(error);

    private static Result<string, string> SquareThenToString(int value) =>
        Result.Try<int, string>(() => checked(value * value), _ => "overflow")
              .Map(x => x.ToString());

    private static Result<Quest, Error> LoadQuest(int id) =>
        Result.Ok<Quest, Error>(new Quest(id, "Slay the ancient white dragon"));

    private static Result<DateTime, Error> SafeParse(string input) =>
        DateTime.TryParse(input, out DateTime parsed)
            ? Result.Ok<DateTime, Error>(parsed)
            : Result.Err<DateTime, Error>(
                new Error(ErrorCodes.MalformedDateTime, "not a date"));

    private static Result<string, string> RollForName() =>
        Result.Ok<string, string>("Vex'ahlia");

    private static Result<int, Error> CountRunes(string value) =>
        Result.Ok<int, Error>(value.Length);

    private static Result<Quest, Error> GetQuest(Character character) =>
        Result.Ok<Quest, Error>(new Quest(1, "Slay the ancient white dragon"));

    private static Result<Reward, Error> ClaimReward(Quest quest) =>
        Result.Ok<Reward, Error>(new Reward("Fenthras, Wrath of the Fey Wilds"));

    private static Result<Character, Error> FindCharacter(string name) =>
        Result.Ok<Character, Error>(new Character(name));
}
