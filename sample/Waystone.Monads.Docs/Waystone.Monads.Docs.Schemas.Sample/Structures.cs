namespace Waystone.Monads.Docs.Schemas.Sample;

using Waystone.Monads.Schemas;

/// <summary>packages/schemas/structures.md</summary>
internal static class StructuresPage
{
    #region schema-structures-list
    // Every item is parsed, so a bad item at index 3 does not hide a bad one at
    // index 7. Both are reported.
    public static readonly Schema<IReadOnlyList<string>, IReadOnlyList<string>>
        Objectives = Schema.List(Schema.Text.Trim().NotEmpty().MaxLength(120))
                           .MinCount(1)
                           .MaxCount(10);
    #endregion

    #region schema-structures-list-of-objects
    // The item schema can be a schema you wrote. Nothing about List cares which.
    public static readonly Schema<IReadOnlyList<LeaderDto>, IReadOnlyList<Leader>>
        Leaders = Schema.List(LeaderSchema.Instance).MinCount(1);
    #endregion

    #region schema-structures-dictionary
    // Keys are parsed too, so a malformed key is a violation rather than a silent
    // entry nobody looks up.
    public static readonly
        Schema<IReadOnlyDictionary<string, int>, IReadOnlyDictionary<string, int>>
        Bounties = Schema.Dictionary(
                              Schema.Text.Trim().Matches("^[a-z-]+$"),
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
}
