using Waystone.Monads.Options;
using Waystone.Monads.Results;

namespace Waystone.Monads.Docs.Core.Sample.Guides;

/// <summary>
/// guides/exceptions.md. The page is almost all prose today and carries no
/// samples of its own, so these are the calls it describes rather than blocks
/// lifted from it. Every one of them throws on purpose.
/// </summary>
internal static class ExceptionsGuide
{
    internal static void UnwrapThrowsOnNone()
    {
        Option<string> none = Option.None<string>();
        none.Unwrap(); // throws UnwrapException
    }

    internal static void UnwrapThrowsOnErr()
    {
        Result<int, string> err = Result.Err<int, string>("the ritual fizzled");
        err.Unwrap(); // throws UnwrapException
    }

    internal static void UnwrapErrThrowsOnOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(20);
        ok.UnwrapErr(); // throws UnwrapException
    }

    internal static void ExpectThrowsOnNone()
    {
        Option<string> none = Option.None<string>();
        none.Expect("the familiar must be summoned"); // throws UnmetExpectationException
    }

    internal static void ExpectThrowsOnErr()
    {
        Result<int, string> err = Result.Err<int, string>("the ritual fizzled");
        err.Expect("the ritual must succeed"); // throws UnmetExpectationException
    }

    internal static void ExpectErrThrowsOnOk()
    {
        Result<int, string> ok = Result.Ok<int, string>(20);
        ok.ExpectErr("the ritual must fail"); // throws UnmetExpectationException
    }
}
