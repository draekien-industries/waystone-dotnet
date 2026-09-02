namespace Waystone.Monads.Docs.Schemas.Sample;

using Waystone.Monads.Options;

// The cast the pages in this project share. Scaffolding rather than published
// code: a page quotes the schema, not the shapes it happens to run over.

public enum QuestRank
{
    Copper,
    Silver,
    Gold,
}

public sealed record QuestDto(
    string? Title,
    string? PatronEmail,
    decimal? GoldReward,
    int? PartySize,
    QuestRank? Rank,
    string? Nickname);

/// <summary>
/// The thing a parse produces. Its constructor is not public, so the only way to
/// hold one is to have passed the schema.
/// </summary>
public sealed class Quest
{
    internal Quest(
        string title,
        string patronEmail,
        decimal goldReward,
        Option<int> partySize)
    {
        Title = title;
        PatronEmail = patronEmail;
        GoldReward = goldReward;
        PartySize = partySize;
    }

    public string Title { get; }

    public string PatronEmail { get; }

    public decimal GoldReward { get; }

    public Option<int> PartySize { get; }
}
