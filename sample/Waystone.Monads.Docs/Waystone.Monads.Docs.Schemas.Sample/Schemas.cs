namespace Waystone.Monads.Docs.Schemas.Sample;

using Waystone.Monads.Results;
using Waystone.Monads.Schemas;

#region schemas-a-reusable-check
public static class Guild
{
    public static readonly Schema<string, string> Title =
        Schema.Text.Trim().LengthBetween(3, 80);

    public static readonly Schema<string, string> Email =
        Schema.Text.Trim().Email();

    public static readonly Schema<decimal, decimal> Reward =
        Schema.Number.Decimal.Between(1m, 10_000m);
}
#endregion

#region schemas-a-field-set
public partial class QuestSchema : SchemaConfig<QuestDto, Quest>
{
    protected override Result<Quest, SchemaViolation> Configure(QuestDto subject) =>
        Schema.Fields(
                   Schema.Required(subject.Title, Guild.Title),

                   // The path a caller is shown is "patron", not the property
                   // name the compiler read off the argument.
                   Schema.Required(subject.PatronEmail, Guild.Email)
                         .Named("patron"),
                   Schema.Required(subject.GoldReward, Guild.Reward),
                   Schema.Optional(subject.PartySize, Schema.Number.Int32.Positive()))
              .Into(
                   (title, patron, reward, party) =>
                       new Quest(title, patron, reward, party));
}
#endregion

/// <summary>packages/schemas.md</summary>
internal static class SchemasPage
{
    internal static Result<Quest, SchemaViolation> Parse(QuestDto posting)
    {
        #region schemas-parse
        Result<Quest, SchemaViolation> result =
            QuestSchema.Instance.Parse(posting);
        #endregion

        return result;
    }

    internal static string Describe(QuestDto posting)
    {
        #region schemas-read-the-failures
        return QuestSchema.Instance.Parse(posting)
                          .Match(
                               quest => $"Accepted {quest.Title}.",
                               violation => string.Join(
                                   "; ",
                                   violation.Violations.Select(
                                       failure =>
                                           $"{failure.Path}: {failure.Message}")));
        #endregion
    }

    internal static IDictionary<string, string[]> ForTheWire(QuestDto posting)
    {
        #region schemas-failures-as-a-dictionary
        return QuestSchema.Instance.Parse(posting)
                          .Match(
                               _ => new Dictionary<string, string[]>(),
                               violation => violation.ToDictionary());
        #endregion
    }

    internal static int CountTheFailures()
    {
        #region schemas-every-failure-at-once
        // An empty title, no patron, and a reward of zero.
        SchemaViolation violation =
            QuestSchema.Instance
                       .Parse(new QuestDto("", null, 0m, null, null, null))
                       .UnwrapErr();

        // Three, not one. A parse reports every field it could not accept.
        int failures = violation.Violations.Count;
        #endregion

        return failures;
    }
}
