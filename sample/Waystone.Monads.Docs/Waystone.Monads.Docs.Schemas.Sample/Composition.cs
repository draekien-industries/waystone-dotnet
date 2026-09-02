namespace Waystone.Monads.Docs.Schemas.Sample;

using System.Text.RegularExpressions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Schemas;

/// <summary>packages/schemas/composition.md</summary>
internal static partial class CompositionPage
{
    [GeneratedRegex("^[^A-Z]*$", RegexOptions.None, 1000)]
    private static partial Regex NoCapitals { get; }

    [GeneratedRegex("[0-9]", RegexOptions.None, 1000)]
    private static partial Regex HasADigit { get; }

    [GeneratedRegex("[^a-zA-Z0-9]", RegexOptions.None, 1000)]
    private static partial Regex HasASymbol { get; }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+$", RegexOptions.None, 1000)]
    private static partial Regex LooksLikeAnEmail { get; }

    [GeneratedRegex(@"^\+?[0-9 ]{8,}$", RegexOptions.None, 1000)]
    private static partial Regex LooksLikeAPhone { get; }

    [GeneratedRegex("^[a-z-]+$", RegexOptions.None, 1000)]
    private static partial Regex LowerCaseAndHyphens { get; }

    #region schema-composition-check
    public static readonly Schema<string, string> Title =
        Schema.Text.Trim()
              .NotEmpty()
              .Check(
                   title => !title.Contains("dragon", StringComparison.OrdinalIgnoreCase),
                   ViolationCode.NotAllowed,
                   "{Path} may not name a dragon, got {Received}.");
    #endregion

    #region schema-composition-transform
    public static readonly Schema<string, QuestTitle> Titled =
        Schema.Text.Trim().NotEmpty().Transform(text => new QuestTitle(text));
    #endregion

    #region schema-composition-transform-result
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
    // A schema worth naming, so Not has something to invert.
    public static readonly Schema<string, string> ReservedPrefixes =
        Schema.Text.StartsWith("guild:");

    public static readonly Schema<string, string> PublicTitle =
        Schema.Text.Trim()
              .NotEmpty()
              .Not(ReservedPrefixes, "{Path} may not use a reserved prefix.");
    #endregion

    #region schema-composition-when-unless
    public static readonly Schema<string, string> SigilOfALongName =
        Schema.Text.MinLength(8).When(text => text.StartsWith("guild:"));

    public static readonly Schema<string, string> NoShoutingUnlessUrgent =
        Schema.Text.Matches(NoCapitals).Unless(text => text.EndsWith("!"));
    #endregion

    #region schema-composition-all
    public static readonly Schema<string, string> Passphrase = Schema.All(
        Schema.Text.MinLength(12),
        Schema.Text.Matches(HasADigit),
        Schema.Text.Matches(HasASymbol));
    #endregion

    #region schema-composition-any
    // An email address or a phone number, either being fine.
    public static readonly Schema<string, string> Contact = Schema.Any(
        Schema.Text.Matches(LooksLikeAnEmail),
        Schema.Text.Matches(LooksLikeAPhone));
    #endregion

    #region schema-composition-message
    // Four rules, one message.
    public static readonly Schema<string, string> Slug =
        Schema.Text.Trim()
              .NotEmpty()
              .MaxLength(40)
              .Matches(LowerCaseAndHyphens)
              .WithMessage("{Path} has to be lower case words joined by hyphens.");
    #endregion

    #region schema-composition-code
    public static readonly Schema<string, string> ReservedTitle =
        Schema.Text.Not(ReservedPrefixes, "Reserved prefix.")
              .WithCode(new ErrorCode("quest.title_reserved"));
    #endregion

    internal static Field<string> Naming(QuestDto subject)
    {
        #region schema-composition-named
        // A violation reports "patron", not "patronEmail".
        return Schema.Required(subject.PatronEmail, Schema.Text.Email())
                     .Named("patron");
        #endregion
    }

    #region schema-composition-named-on-a-schema
    // Naming the branches, so a failure says which one was tried.
    public static readonly Schema<string, string> Contactable = Schema.Any(
        Schema.Text.Email().Named("email"),
        Schema.Text.Matches(LooksLikeAPhone).Named("phone"));
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
