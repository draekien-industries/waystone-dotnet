using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Reference;

/// <summary>
/// reference/option/nesting.md and reference/result/nesting.md — Flatten,
/// Transpose, and the four conversions between the two types.
/// </summary>
internal static class Nesting
{
    internal sealed class TollCalculator
    {
        internal Option<decimal> GetToll(decimal amount) => Option.Some(amount * 0.1m);
    }

    internal static class Realm
    {
        internal const string TalDorei = "Tal'Dorei";
    }

    internal static void FlattenAnOption()
    {
        Option<Option<string>> some = Option.Some(Option.Some("Chetney"));
        Option<string> result = some.Flatten();

        _ = result;
    }

    internal static void FlattenAResult()
    {
        Result<string, string> start = Result.Ok<string, string>("Storm Weaver");
        Result<Result<int, string>, string> output = start.Map(x => CountRunes(x));
        Result<int, string> flattened = output.Flatten();

        _ = flattened;
    }

    internal static void TransposeAnOptionOfResult()
    {
        Option<int> maybeNumber = Option.Try(() => RollD20());

        Option<Result<int, string>> maybeResult = maybeNumber
            .Map(number => Divide(number, 2));

        Result<Option<int>, string> result = maybeResult.Transpose();

        _ = result;
    }

    internal static void TransposeAResultOfOption()
    {
        Result<Option<decimal>, string> calculationResult =
            CreateCalculator(Realm.TalDorei)
                .Map(calculator => calculator.GetToll(100.00m));

        Option<Result<decimal, string>> maybeToll = calculationResult.Transpose();

        _ = maybeToll;
    }

    internal static void OkOr()
    {
        Option<int> some = Option.Some(1);
        Option<int> none = Option.None<int>();
        Error error = new("ER1", "Missing number.");

        Result<int, Error> ok = some.OkOr(error);
        //                 ^? Ok(1)

        Result<int, Error> err = none.OkOr(error);
        //                 ^? Err(error)

        _ = (ok, err);
    }

    internal static void OkOrElse()
    {
        Option<int> some = Option.Some(1);
        Option<int> none = Option.None<int>();

        Result<int, string> ok = some.OkOrElse(() => "Missing number");
        //                  ^? Ok(1)

        Result<int, string> err = none.OkOrElse(() => "Missing number");
        //                  ^? Err("Missing number")

        _ = (ok, err);
    }

    internal static void GetOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<int, string> err = Result.Err<int, string>("Error");

        Option<int> some = ok.GetOk();
        //          ^? Some(1)

        Option<int> none = err.GetOk();
        //          ^? None()

        _ = (some, none);
    }

    internal static void GetErr()
    {
        Result<int, string> ok = Result.Ok<int, string>(1);
        Result<int, string> err = Result.Err<int, string>("Error");

        Option<string> none = ok.GetErr();
        //             ^? None()

        Option<string> some = err.GetErr();
        //             ^? Some("Error")

        _ = (none, some);
    }

    private static Result<int, string> CountRunes(string source) =>
        Result.Ok<int, string>(source.Length);

    private static Result<int, string> Divide(int a, int b) =>
        b == 0
            ? Result.Err<int, string>("divide by zero")
            : Result.Ok<int, string>(a / b);

    private static int RollD20() => Random.Shared.Next(1, 21);

    private static Result<TollCalculator, string> CreateCalculator(string realm) =>
        Result.Ok<TollCalculator, string>(new TollCalculator());
}
