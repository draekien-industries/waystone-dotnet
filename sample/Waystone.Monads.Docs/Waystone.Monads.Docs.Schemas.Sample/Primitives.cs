namespace Waystone.Monads.Docs.Schemas.Sample;

using System.Text.RegularExpressions;
using Waystone.Monads.Schemas;

/// <summary>packages/schemas/primitives.md</summary>
internal static partial class PrimitivesPage
{
    #region schema-primitives-text
    public static readonly Schema<string, string> Sigil =
        Schema.Text.Trim().LengthBetween(3, 24);

    // A shape with a fixed width says so, rather than bounding both ends at the
    // same number.
    public static readonly Schema<string, string> CountryCode =
        Schema.Text.Trim().Length(2);

    // A closed set of spellings. Schema.Enum is the better home when the domain
    // already models the set as an enumeration.
    public static readonly Schema<string, string> Difficulty =
        Schema.Text.Trim()
              .OneOf(
                   global::System.StringComparison.OrdinalIgnoreCase,
                   "easy",
                   "standard",
                   "deadly");
    #endregion

    #region schema-primitives-text-pattern
    // Matches takes a Regex rather than a pattern string, which is what puts the
    // choice of a match timeout in front of you. [GeneratedRegex] compiles the
    // expression at build time instead of at start-up.
    [GeneratedRegex("^[a-z-]+$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex RunePattern { get; }

    public static readonly Schema<string, string> Rune =
        Schema.Text.Matches(RunePattern);

    // Build it by hand where the expression is not known at compile time. Give it
    // a timeout: the pattern is yours, the value is not, and an expression with no
    // ceiling runs against a crafted input for as long as that input takes.
    public static readonly Schema<string, string> Incantation =
        Schema.Text.Matches(
            new Regex(
                @"^\p{L}[\p{L}\s]*$",
                RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1)));
    #endregion

    #region schema-primitives-text-shapes
    // Checked by a scan rather than an expression, so there is no pattern to get
    // subtly wrong and no matching timeout to trip.
    public static readonly Schema<string, string> PatronEmail =
        Schema.Text.Trim().Email();

    // Restrict the scheme whenever the value will be followed or rendered. An
    // absolute URL also includes javascript: and data:.
    public static readonly Schema<string, string> Portrait =
        Schema.Text.Trim().Url("https");

    // Literals, not expressions. A dot or a bracket here means itself.
    public static readonly Schema<string, string> Tagged =
        Schema.Text.StartsWith("quest:").EndsWith(".md");
    #endregion

    #region schema-primitives-numbers
    // One rule, so a party of twelve is one failure rather than two.
    public static readonly Schema<int, int> PartySize =
        Schema.Number.Int32.Between(1, 6);

    public static readonly Schema<long, long> ExperienceAwarded =
        Schema.Number.Int64.Positive();

    // Exclusive at both ends, which is what GreaterThan and LessThan mean.
    public static readonly Schema<decimal, decimal> GoldReward =
        Schema.Number.Decimal.GreaterThan(0m).LessThan(10_000m);

    public static readonly Schema<double, double> SpellRangeMetres =
        Schema.Number.Double.Positive();
    #endregion

    #region schema-primitives-identifiers
    public static readonly Schema<Guid, Guid> QuestId = Schema.Id.NotEmpty();
    #endregion

    #region schema-primitives-temporal
    // A moment, so a time zone is part of the value.
    public static readonly Schema<DateTimeOffset, DateTimeOffset> Deadline =
        Schema.Timestamp.After(DateTimeOffset.UnixEpoch);

    // A day, so a time of day would be noise. Not available on netstandard2.0.
    public static readonly Schema<DateOnly, DateOnly> Founded =
        Schema.Date.OnOrAfter(new DateOnly(1066, 10, 14));

    // Inclusivity is in the name. Before and After exclude the bound; OnOrBefore
    // and OnOrAfter include it, which is what a closing date means.
    public static readonly Schema<DateOnly, DateOnly> ClosesOn =
        Schema.Date.OnOrBefore(new DateOnly(2026, 12, 31));
    #endregion

    #region schema-primitives-booleans
    public static readonly Schema<bool, bool> AcceptedTerms =
        Schema.Bool.IsTrue();

    // The rarer half, and worth a second look. A flag that has to be clear often
    // reads better as the opposite flag that has to be set.
    public static readonly Schema<bool, bool> NotSuspended =
        Schema.Bool.IsFalse();
    #endregion

    #region schema-primitives-enums
    // Rejects a value outside the declared members, which a cast can produce.
    public static readonly Schema<QuestRank, QuestRank> Rank =
        Schema.Enum<QuestRank>();
    #endregion

    #region schema-primitives-any-type
    // For<T> is the identity schema: it accepts anything of that type and gives
    // Check and Transform somewhere to hang off. Every primitive above is one.
    public static readonly Schema<TimeSpan, TimeSpan> Duration =
        Schema.For<TimeSpan>()
              .Check(
                   span => span > TimeSpan.Zero,
                   ViolationCode.OutOfRange,
                   "{Path} has to be longer than nothing, got {Received}.");
    #endregion
}
