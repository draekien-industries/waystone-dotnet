namespace Waystone.DocSnippets;

using System.IO;

/// <summary>
/// One sample file and one page, laid out the way the tool expects to find them.
/// Shared because <see cref="RunnerTests" /> and <see cref="CliTests" /> drive the
/// same tree from different heights.
/// </summary>
internal static class Fixtures
{
    internal const string Source =
        """
        class Party
        {
            #region short-rest
            party.Rest();
            #endregion
        }
        """;

    internal const string EmptySlot =
        """
        <!-- snippet: short-rest -->
        <!-- endSnippet -->
        """;

    internal const string FilledSlot =
        """
        <!-- snippet: short-rest -->
        <!-- source: sample/Waystone.Monads.Docs/Party.cs -->
        ```csharp
        party.Rest();
        ```
        <!-- endSnippet -->
        """;

    internal const string Page = "waystone.monads/page.md";

    internal static TemporaryDirectory Repository(string page)
    {
        TemporaryDirectory root = new();
        Directory.CreateDirectory(Path.Combine(root.Path, ".git"));
        root.Write(Path.Combine("sample", "Waystone.Monads.Docs", "Party.cs"), Source);
        root.Write(Path.Combine("docs", "waystone.monads", "page.md"), page);

        return root;
    }

    internal static Options Options(TemporaryDirectory root, bool check) =>
        new(
            root.Path,
            Path.Combine(root.Path, "sample", "Waystone.Monads.Docs"),
            Path.Combine(root.Path, "docs"),
            check);
}
