using Waystone.Monads.Options;
using Waystone.Monads.Results;

namespace Waystone.Monads.Docs.Core.Sample.StartHere;

/// <summary>start-here/quickstart.md</summary>
internal static class Quickstart
{
    internal static (string Vow, string Silence) UsingOption()
    {
        Option<string> patron = Option.Some("The Raven Queen");
        Option<string> noPatron = Option.None<string>();

        string vow = patron.Match(
            some => $"You are sworn to {some}.",
            () => "You are sworn to no one.");
        // "You are sworn to The Raven Queen."

        string silence = noPatron.Match(
            some => $"You are sworn to {some}.",
            () => "You are sworn to no one.");
        // "You are sworn to no one."

        return (vow, silence);
    }

    internal static int UsingResult()
    {
        Result<int, string> roll = RollDie("20");

        int value = roll.Match(
            ok => ok,
            err => 0);
        // somewhere between 1 and 20

        return value;
    }

    private static Result<int, string> RollDie(string sides) =>
        int.TryParse(sides, out int faces) && faces > 0
            ? Result.Ok<int, string>(Random.Shared.Next(1, faces + 1))
            : Result.Err<int, string>($"'{sides}' is not a number of sides.");
}
