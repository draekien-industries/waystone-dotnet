namespace Waystone.Monads.Docs.Schemas.Sample;

using System.Text.RegularExpressions;
using Waystone.Monads.Schemas;

/// <summary>packages/schemas/structures.md</summary>
internal static partial class StructuresPage
{
    [GeneratedRegex("^[a-z-]+$", RegexOptions.None, 1000)]
    private static partial Regex BountyName { get; }

    #region schema-structures-list
    // At least one objective, at most ten, and each one trimmed and bounded.
    public static readonly Schema<IReadOnlyList<string>, IReadOnlyList<string>>
        Objectives = Schema.List(Schema.Text.Trim().NotEmpty().MaxLength(120))
                           .MinCount(1)
                           .MaxCount(10);
    #endregion

    #region schema-structures-list-of-objects
    public static readonly Schema<IReadOnlyList<LeaderDto>, IReadOnlyList<Leader>>
        Leaders = Schema.List(LeaderSchema.Instance).MinCount(1);
    #endregion

    #region schema-structures-dictionary
    // A schema for the keys and a schema for the values.
    public static readonly
        Schema<IReadOnlyDictionary<string, int>, IReadOnlyDictionary<string, int>>
        Bounties = Schema.Dictionary(
                              Schema.Text.Trim().Matches(BountyName),
                              Schema.Number.Int32.Positive())
                         .MaxCount(50);
    #endregion

    internal static IReadOnlyList<string> IndexedPaths()
    {
        #region schema-structures-indexed-paths
        // A violation inside a structure carries where it was found, so the path
        // reads "[1]" for a list and "leader.email" through a nested schema. In a
        // field set both are prefixed by the field: "objectives[1]".
        SchemaViolation violation =
            Objectives.Parse(["Rescue the cleric", "  "]).UnwrapErr();

        IReadOnlyList<string> paths =
            violation.Violations.Select(failure => failure.Path.ToString())
                     .ToList();
        #endregion

        return paths;
    }

    internal static string PathSegments()
    {
        #region schema-structures-path-segments
        // The rendered path is written for a human. Branch on the segments
        // instead: a list position, a dictionary key and a failed Schema.Any
        // branch all render inside brackets, and only the segment says which
        // one you are looking at.
        SchemaViolation violation =
            Objectives.Parse(["Rescue the cleric", "  "]).UnwrapErr();

        PathSegment last = violation.Violations[0].Path.Segments[^1];

        string located = last.Kind switch
        {
            PathSegmentKind.Index => $"entry {last.Text}",
            PathSegmentKind.Key => $"key {last.Text}",
            PathSegmentKind.Branch => $"alternative {last.Text}",
            _ => last.Text,
        };
        #endregion

        return located;
    }

    internal static int BoundedReport()
    {
        #region schema-structures-too-many-problems
        // One list or dictionary reports at most 64 problems and then stops, so a
        // hostile payload cannot make the report as expensive as it likes. When it
        // stops, it says so with a truncated violation rather than trailing off.
        string[] blanks = Enumerable.Repeat("  ", 500).ToArray();

        SchemaViolation violation =
            Schema.List(Schema.Text.NotEmpty()).Parse(blanks).UnwrapErr();

        bool thereAreMore = violation.Violations.Any(
            failure =>
                failure.Code == ViolationCodeCatalog.Codes.Truncated);
        #endregion

        return thereAreMore ? violation.Violations.Count : 0;
    }
}
