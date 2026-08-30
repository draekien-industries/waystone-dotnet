using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Core.Sample.Reference.OptionApi;

/// <summary>reference/option/nesting.md</summary>
internal static class OptionNesting
{
    internal static void Flatten()
    {
        Option<Option<string>> some = Option.Some(Option.Some("Chetney"));
        Option<string> result = some.Flatten();

        _ = result;
    }

    internal static void Transpose()
    {
        Option<int> maybeNumber = Option.Try(() => RollD20());

        Option<Result<int, string>> maybeResult = maybeNumber
            .Map(number => Divide(number, 2));

        Result<Option<int>, string> result = maybeResult.Transpose();

        _ = result;
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

    private static Result<int, string> Divide(int a, int b) =>
        b == 0
            ? Result.Err<int, string>("divide by zero")
            : Result.Ok<int, string>(a / b);

    private static int RollD20() => Random.Shared.Next(1, 21);
}
