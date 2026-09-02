namespace Waystone.Monads.Docs.Schemas.Sample;

using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

public interface IQuestBoard
{
    ValueTask<bool> TitleIsFree(string title, CancellationToken cancellationToken);
}

/// <summary>packages/schemas/asynchrony.md</summary>
internal static class AsynchronyPage
{
    #region schema-async-check
    // The rule is handed the value and the parse's own cancellation token. It runs
    // only when everything before it accepted, so it never sees a value the chain
    // could not produce.
    public static Schema<string, string> UniqueTitle(IQuestBoard board) =>
        Schema.Text.Trim()
              .NotEmpty()
              .MaxLength(80)
              .CheckAsync(
                   board.TitleIsFree,
                   ViolationCode.Duplicate,
                   "{Path} is already on the board, got {Received}.");
    #endregion

    internal static async Task<string> Post(
        IQuestBoard board,
        string title,
        CancellationToken cancellationToken)
    {
        #region schema-async-parse
        Result<string, SchemaViolation> result =
            await UniqueTitle(board).ParseAsync(title, cancellationToken);
        #endregion

        return result.UnwrapOr("untitled");
    }

    #region schema-async-outside-a-field-set
    // SchemaConfig.Configure returns a value rather than a task, so a field set
    // only ever runs the synchronous path. An asynchronous rule reached from there
    // throws, and WMSC0006 reports it at build time rather than in production.
    //
    // Compose the rule around the generated schema instead. The field set stays
    // synchronous and the round trip happens once, after every cheap rule has
    // already had its say.
    public static ValueTask<Result<Quest, SchemaViolation>> ParseAgainstTheBoard(
        QuestDto posting,
        IQuestBoard board,
        CancellationToken cancellationToken) =>
        QuestSchema.Instance
                   .CheckAsync(
                        (quest, token) => board.TitleIsFree(quest.Title, token),
                        ViolationCode.Duplicate,
                        "That quest is already on the board.")
                   .ParseAsync(posting, cancellationToken);
    #endregion
}
