namespace Waystone.Monads.Docs.Schemas.Sample;

using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Schemas;

/// <summary>packages/schemas/composition.md</summary>
internal static class CompositionPage
{
    #region schema-composition-check
    // A refinement. The value survives a failure, so every later rule on the
    // chain still runs and one parse reports all of them.
    public static readonly Schema<string, string> Title =
        Schema.Text.Trim()
              .NotEmpty()
              .Check(
                   title => !title.Contains("dragon", StringComparison.OrdinalIgnoreCase),
                   ViolationCode.NotAllowed,
                   "{Path} may not name a dragon, got {Received}.");
    #endregion

    #region schema-composition-transform
    // Changes the type the schema produces. From here on the chain is over
    // QuestTitle, not string.
    public static readonly Schema<string, QuestTitle> Titled =
        Schema.Text.Trim().NotEmpty().Transform(text => new QuestTitle(text));
    #endregion

    #region schema-composition-transform-result
    // A transform that can fail produces no value, so its own chain stops there.
    // Its siblings in a field set carry on and still report.
    public static readonly Schema<string, QuestRank> Rank =
        Schema.Text.Trim()
              .Transform(
                   text => Enum.TryParse(text, true, out QuestRank rank)
                       ? Result.Ok<QuestRank, Error>(rank)
                       : Result.Err<QuestRank, Error>(
                           ViolationCodeCatalog.Errors.Malformed(
                               $"'{text}' is not a rank.")));
    #endregion

    #region schema-composition-not
    // Reach for Not when the thing being rejected is already a schema worth
    // naming. For a one-off condition, Check with the negated predicate reads
    // better and costs less.
    public static readonly Schema<string, string> ReservedPrefixes =
        Schema.Text.StartsWith("guild:");

    // Negation has no message of its own to borrow, so one is required.
    public static readonly Schema<string, string> PublicTitle =
        Schema.Text.Trim()
              .NotEmpty()
              .Not(ReservedPrefixes, "{Path} may not use a reserved prefix.");
    #endregion

    #region schema-composition-when-unless
    // The rules run only when the predicate holds. Both take the whole value, so
    // they read as a condition on the subject rather than on one field.
    public static readonly Schema<string, string> SigilOfALongName =
        Schema.Text.MinLength(8).When(text => text.StartsWith("guild:"));

    public static readonly Schema<string, string> NoShoutingUnlessUrgent =
        Schema.Text.Matches("^[^A-Z]*$").Unless(text => text.EndsWith("!"));
    #endregion

    #region schema-composition-all
    // Every branch runs, and every failure is reported.
    public static readonly Schema<string, string> Passphrase = Schema.All(
        Schema.Text.MinLength(12),
        Schema.Text.Matches("[0-9]"),
        Schema.Text.Matches("[^a-zA-Z0-9]"));
    #endregion

    #region schema-composition-any
    // The first branch that accepts wins. When none does, one violation is
    // reported at the union's own path carrying the branch failures beneath it,
    // rather than a flat list a reader cannot attribute.
    public static readonly Schema<string, string> Contact = Schema.Any(
        Schema.Text.Matches(@"^[^@\s]+@[^@\s]+$"),
        Schema.Text.Matches(@"^\+?[0-9 ]{8,}$"));
    #endregion

    #region schema-composition-message
    // Replaces the message of every violation the chain produced, not only the
    // last one. Reach for it when the rules are an implementation detail and the
    // reader only needs to know what shape was expected.
    public static readonly Schema<string, string> Slug =
        Schema.Text.Trim()
              .NotEmpty()
              .MaxLength(40)
              .Matches("^[a-z-]+$")
              .WithMessage("{Path} has to be lower case words joined by hyphens.");
    #endregion

    #region schema-composition-code
    // A domain code, so a caller can branch on the failure without matching text.
    public static readonly Schema<string, string> ReservedTitle =
        Schema.Text.Not(ReservedPrefixes, "Reserved prefix.")
              .WithCode(new ErrorCode("quest.title_reserved"));
    #endregion

    internal static Field<string> Naming(QuestDto subject)
    {
        #region schema-composition-named
        // Set the path on the field, not on the schema. A schema is shared, so a
        // name baked into one renames every field of its shape and nothing
        // reports it; a field is built per parse and cannot leak.
        return Schema.Required(subject.PatronEmail, Schema.Text.Email())
                     .Named("patron");
        #endregion
    }

    #region schema-composition-named-on-a-schema
    // Schema.Named is the other half, for a schema that is not reached through a
    // field: a branch of Schema.Any, or one handed straight to Parse.
    public static readonly Schema<string, string> Contactable = Schema.Any(
        Schema.Text.Email().Named("email"),
        Schema.Text.Matches(@"^\+?[0-9 ]{8,}$").Named("phone"));
    #endregion

    #region schema-composition-sensitive
    // {Received} renders *** for this schema and everything beneath it, so the
    // rejected value stays out of logs and out of the response. Opt-in, because
    // seeing what was rejected is what makes most messages useful.
    public static readonly Schema<string, string> Secret =
        Schema.Text.NotEmpty().MinLength(12).Sensitive();
    #endregion
}

public sealed record QuestTitle(string Value);
