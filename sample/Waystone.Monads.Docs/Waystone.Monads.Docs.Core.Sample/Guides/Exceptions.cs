using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Core.Sample.Guides;

/// <summary>
/// guides/exceptions.md. Every call in the first six methods throws on purpose
/// — that is what the page is about.
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

    internal static void TrySwallowsTheThrow(string text)
    {
        Option<int> parsed = Option.Try(() => int.Parse(text));
        // None if the parse threw

        _ = parsed;
    }

    internal static void TheConstructorsRejectNull()
    {
        Result.Ok<string, Error>(null!); // throws ArgumentNullException
    }
}
