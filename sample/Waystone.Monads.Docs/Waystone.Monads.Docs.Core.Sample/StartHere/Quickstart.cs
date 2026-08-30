using Waystone.Monads.Options;
using Waystone.Monads.Results;

namespace Waystone.Monads.Docs.Core.Sample.StartHere;

/// <summary>start-here/quickstart.md</summary>
internal static class Quickstart
{
    internal static string UsingOption()
    {
        Option<string> name = Option.Some("Liam O'Brian");
        Option<string> missing = Option.None<string>();

        string greeting = name.Match(
            some => $"Hello, {some}!",
            () => "Hello, stranger!");

        return greeting + missing.UnwrapOr(string.Empty);
    }

    internal static int UsingResult()
    {
        var result = ParseInt("42");

        int value = result.Match(
            ok => ok,
            err => -1);

        return value;
    }

    private static Result<int, string> ParseInt(string input) =>
        int.TryParse(input, out var value)
            ? Result.Ok<int, string>(value)
            : Result.Err<int, string>($"Input '{input}' is not a valid number");
}
