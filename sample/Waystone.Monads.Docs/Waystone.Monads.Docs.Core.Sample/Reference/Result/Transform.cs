using System.Diagnostics;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Core.Sample.Reference.ResultApi;

/// <summary>reference/result/transform.md</summary>
internal static class ResultTransform
{
    internal static void Map()
    {
        Result<string, string> nameResult = Result.Ok<string, string>("Consent");
        Result<int, string> lengthResult = nameResult.Map(name => name.Length);

        _ = lengthResult;
    }

    internal static Result<int, Error> MapErr() =>
        RollForName()                                             // Result<string, string>
            .MapErr(message => new Error("name.failed", message)) // Result<string, Error>
            .AndThen(name => CountRunes(name));                   // Result<int, Error>

    internal static void AndThen()
    {
        Result<int, string> two = Result.Ok<int, string>(2);
        Debug.Assert(two.AndThen(SquareThenToString) == Result.Ok<string, string>("4"));

        Result<int, string> big = Result.Ok<int, string>(int.MaxValue);
        Debug.Assert(big.AndThen(SquareThenToString) == Result.Err<string, string>("overflow"));

        Result<int, string> nan = Result.Err<int, string>("NaN");
        Debug.Assert(nan.AndThen(SquareThenToString) == Result.Err<string, string>("NaN"));
    }

    internal static void And()
    {
        _ = Result.Ok<int, string>(1).And(Result.Err<int, string>("late error"));
        //  ^? Err("late error")

        _ = Result.Err<int, string>("early error").And(Result.Ok<int, string>(1));
        //  ^? Err("early error")

        _ = Result.Err<int, string>("early error").And(Result.Err<int, string>("late error"));
        //  ^? Err("early error")

        _ = Result.Ok<int, string>(1).And(Result.Ok<int, string>(2));
        //  ^? Ok(2)
    }

    internal static void Or()
    {
        _ = Result.Ok<int, string>(1).Or(Result.Ok<int, string>(2));
        //  ^? Ok(1)

        _ = Result.Ok<int, string>(1).Or(Result.Err<int, string>("error"));
        //  ^? Ok(1)

        _ = Result.Err<int, string>("error").Or(Result.Ok<int, string>(1));
        //  ^? Ok(1)

        _ = Result.Err<int, string>("error 1").Or(Result.Err<int, string>("error 2"));
        //  ^? Err("error 2")
    }

    internal static void OrElse()
    {
        _ = Result.Ok<int, string>(2).OrElse(Recover);           // Ok(2), untouched
        _ = Result.Err<int, string>("NaN").OrElse(Recover);      // Ok(0), recovered
        _ = Result.Err<int, string>("overflow").OrElse(Recover); // Err("overflow")
    }

    // OrElse runs on the Err half, so its factory takes the error — not the ok value.
    private static Result<int, string> Recover(string error) =>
        error == "NaN"
            ? Result.Ok<int, string>(0)
            : Result.Err<int, string>(error);

    private static Result<string, string> SquareThenToString(int value) =>
        Result.Try<int, string>(() => checked(value * value), _ => "overflow")
              .Map(x => x.ToString());

    private static Result<string, string> RollForName() =>
        Result.Ok<string, string>("Vex'ahlia");

    private static Result<int, Error> CountRunes(string value) =>
        Result.Ok<int, Error>(value.Length);
}
