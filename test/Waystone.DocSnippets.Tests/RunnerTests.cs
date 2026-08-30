namespace Waystone.DocSnippets;

using Shouldly;
using System.IO;
using static Waystone.DocSnippets.Fixtures;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class RunnerTests
{
    [Fact]
    public void FillsAStalePageAndNamesIt()
    {
        using TemporaryDirectory root = Repository(EmptySlot);

        RunResult result = Runner.Run(Options(root, check: false)).ShouldBeOk();

        result.StalePages.ShouldBe(["waystone.monads/page.md"]);
        root.Read(Path.Combine("docs", "waystone.monads", "page.md")).ShouldBe(FilledSlot);
    }

    [Fact]
    public void SaysNothingIsStaleWhenThePageIsAlreadyCurrent()
    {
        using TemporaryDirectory root = Repository(FilledSlot);

        Runner.Run(Options(root, check: false)).ShouldBeOk().StalePages.ShouldBeEmpty();
    }

    [Fact]
    public void LeavesTheFileUntouchedInCheckMode()
    {
        using TemporaryDirectory root = Repository(EmptySlot);

        RunResult result = Runner.Run(Options(root, check: true)).ShouldBeOk();

        result.StalePages.ShouldBe(["waystone.monads/page.md"]);
        root.Read(Path.Combine("docs", "waystone.monads", "page.md")).ShouldBe(EmptySlot);
    }

    [Fact]
    public void ReportsARegionNoPageUses()
    {
        using TemporaryDirectory root = Repository("No slots here.");

        Runner.Run(Options(root, check: true)).ShouldBeOk().UnusedKeys.ShouldBe(["short-rest"]);
    }

    [Fact]
    public void ReportsNothingUnusedOnceAPageClaimsTheRegion()
    {
        using TemporaryDirectory root = Repository(EmptySlot);

        Runner.Run(Options(root, check: true)).ShouldBeOk().UnusedKeys.ShouldBeEmpty();
    }

    [Fact]
    public void IgnoresRegionsLeftInBuildOutput()
    {
        using TemporaryDirectory root = Repository(EmptySlot);
        root.Write(
            Path.Combine("sample", "Waystone.Monads.Docs", "obj", "Debug", "Party.cs"),
            Source);

        Runner.Run(Options(root, check: true)).ShouldBeOk().StalePages.ShouldBe(["waystone.monads/page.md"]);
    }

    [Fact]
    public void IgnoresMarkdownInsideDotDirectories()
    {
        using TemporaryDirectory root = Repository("No slots here.");
        root.Write(Path.Combine("docs", ".github", "page.md"), "<!-- snippet: unknown-key -->");

        Runner.Run(Options(root, check: true)).ShouldBeOk().StalePages.ShouldBeEmpty();
    }

    [Fact]
    public void RefusesTwoRegionsWithTheSameName()
    {
        using TemporaryDirectory root = Repository(EmptySlot);
        root.Write(Path.Combine("sample", "Waystone.Monads.Docs", "Other.cs"), Source);

        Error error = Runner.Run(Options(root, check: true)).ShouldBeErr();

        error.Code.ShouldBe(DocSnippetError.DuplicateKey.ToErrorCode());
        error.Message.ShouldContain("'short-rest' is defined in both");
    }

    [Fact]
    public void RefusesAPageWhoseSlotIsNeverClosed()
    {
        using TemporaryDirectory root = Repository("<!-- snippet: short-rest -->");

        Error error = Runner.Run(Options(root, check: true)).ShouldBeErr();

        error.Code.ShouldBe(DocSnippetError.UnterminatedSlot.ToErrorCode());
        error.Message.ShouldContain("waystone.monads/page.md");
    }
}
