using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Reference.ResultApi;

/// <summary>reference/result/nesting.md</summary>
internal static class ResultNesting
{
    internal sealed class TollCalculator
    {
        internal Option<decimal> GetToll(decimal amount) => Option.Some(amount * 0.1m);
    }

    internal static class Realm
    {
        internal const string TalDorei = "Tal'Dorei";
    }

    internal static void Flatten()
    {
        Result<string, string> start = Result.Ok<string, string>("Storm Weaver");
        Result<Result<int, string>, string> output = start.Map(x => CountRunes(x));
        Result<int, string> flattened = output.Flatten();

        _ = flattened;
    }

    internal static void Transpose()
    {
        Result<Option<decimal>, string> calculationResult =
            CreateCalculator(Realm.TalDorei)
                .Map(calculator => calculator.GetToll(100.00m));

        Option<Result<decimal, string>> maybeToll = calculationResult.Transpose();

        _ = maybeToll;
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

    private static Result<TollCalculator, string> CreateCalculator(string realm) =>
        Result.Ok<TollCalculator, string>(new TollCalculator());
}
