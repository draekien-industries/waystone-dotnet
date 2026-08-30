using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Core.Sample.Reference.ResultApi;

/// <summary>reference/result/side-effects.md</summary>
internal static class ResultSideEffects
{
    internal sealed record Quest(int Id, string Name);

    internal sealed record Character(string Username);

    private interface ILogger
    {
        void LogInformation(string message, params object?[] args);

        void LogWarning(string message, params object?[] args);
    }

    private static readonly ILogger logger = null!;

    internal static void Inspect()
    {
        Result<string, string> nameResult = Result.Ok<string, string>("Percival");
        nameResult.Inspect(name => Console.WriteLine(name.Length));
    }

    internal static Result<string, string> InspectErr() =>
        FindCharacter("Percy")
            .InspectErr(err => Console.WriteLine($"Find character failed: {err.Message}"))
            .Map(character => character.Username)
            .MapErr(err => err.Message);

    internal static Result<Quest, Error> BothTogether(int id) =>
        LoadQuest(id)
            .Inspect(q => logger.LogInformation("Loaded quest {Id}", q.Id))
            .InspectErr(e => logger.LogWarning("Load failed: {Code} {Message}", e.Code, e.Message));

    internal static void ToStringShowsOnlyTheState(Error e)
    {
        _ = Result.Ok<int, Error>(1).ToString();  // "Ok { IsOk = True, IsErr = False }"
        _ = Result.Err<int, Error>(e).ToString(); // "Err { IsOk = False, IsErr = True }"
    }

    private static Result<Quest, Error> LoadQuest(int id) =>
        Result.Ok<Quest, Error>(new Quest(id, "Slay the ancient white dragon"));

    private static Result<Character, Error> FindCharacter(string name) =>
        Result.Ok<Character, Error>(new Character(name));
}
