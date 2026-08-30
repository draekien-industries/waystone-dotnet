namespace Waystone.DocSnippets;

using Shouldly;
using System.IO;
using static Waystone.DocSnippets.Fixtures;
using Waystone.Monads.Options;
using Xunit;

public sealed class CliTests
{
    [Fact]
    public void FillsThePageAndSaysWhichOneItTouched()
    {
        using TemporaryDirectory root = Repository(EmptySlot);
        StringWriter output = new();

        Run(root, output, new StringWriter()).ShouldBe(0);

        output.ToString().ShouldContain("updated: waystone.monads/page.md");
        root.Read(Path.Combine("docs", "waystone.monads", "page.md")).ShouldContain("party.Rest();");
    }

    [Fact]
    public void ReportsAStalePageAndFailsUnderCheck()
    {
        using TemporaryDirectory root = Repository(EmptySlot);
        StringWriter output = new();
        StringWriter error = new();

        Run(root, output, error, "--check").ShouldBe(1);

        output.ToString().ShouldContain("stale: waystone.monads/page.md");
        error.ToString().ShouldContain("1 page(s) no longer match");
        root.Read(Path.Combine("docs", "waystone.monads", "page.md")).ShouldBe(EmptySlot);
    }

    [Fact]
    public void SaysSoWhenEveryPageIsCurrent()
    {
        using TemporaryDirectory root = Repository(EmptySlot);
        StringWriter output = new();

        Run(root, new StringWriter(), new StringWriter());
        Run(root, output, new StringWriter(), "--check").ShouldBe(0);

        output.ToString().ShouldContain("Every documentation snippet matches its source");
    }

    [Fact]
    public void NotesARegionNoPageUses()
    {
        using TemporaryDirectory root = Repository("No slots here.");
        StringWriter output = new();

        Run(root, output, new StringWriter()).ShouldBe(0);

        output.ToString().ShouldContain("note: snippet 'short-rest' is defined but no page uses it.");
    }

    [Fact]
    public void SeparatesAMissingCheckoutFromARealFailure()
    {
        using TemporaryDirectory root = new();
        Directory.CreateDirectory(Path.Combine(root.Path, ".git"));
        Directory.CreateDirectory(Path.Combine(root.Path, "sample", "Waystone.Monads.Docs"));
        StringWriter error = new();

        Cli.Run([], root.Path, _ => Option.None<string>(), new StringWriter(), error)
           .ShouldBe(Cli.NoDocumentationRepository);

        error.ToString().ShouldContain("Could not find the documentation repository");
    }

    [Fact]
    public void FailsWithOneWhenTheSourceIsMalformed()
    {
        using TemporaryDirectory root = Repository(EmptySlot);
        root.Write(
            Path.Combine("sample", "Waystone.Monads.Docs", "Broken.cs"),
            "#region long-rest");
        StringWriter error = new();

        Run(root, new StringWriter(), error).ShouldBe(1);

        error.ToString().ShouldContain("DocSnippetError.UnterminatedRegion");
    }

    [Fact]
    public void TakesTheDocumentationPathFromGitConfigWhenNothingElseNamesIt()
    {
        using TemporaryDirectory root = Repository(EmptySlot);
        string docs = Path.Combine(root.Path, "docs");
        StringWriter output = new();

        Cli.Run([], root.Path, _ => Option.Some(docs), output, new StringWriter()).ShouldBe(0);

        output.ToString().ShouldContain("updated: waystone.monads/page.md");
    }

    [Fact]
    public void PrefersTheExplicitArgumentOverEveryOtherCandidate()
    {
        using TemporaryDirectory root = Repository(EmptySlot);
        string docs = Path.Combine(root.Path, "docs");
        StringWriter output = new();

        Cli.Run(
               ["--docs", docs],
               root.Path,
               _ => Option.Some("nowhere"),
               output,
               new StringWriter())
           .ShouldBe(0);

        output.ToString().ShouldContain("updated: waystone.monads/page.md");
    }

    [Fact]
    public void TakesTheSampleRepositoryFromRepoWhenRunFromElsewhere()
    {
        using TemporaryDirectory root = Repository(EmptySlot);
        using TemporaryDirectory elsewhere = new();
        StringWriter output = new();

        Cli.Run(
               ["--repo", root.Path, "--docs", Path.Combine(root.Path, "docs")],
               elsewhere.Path,
               _ => Option.None<string>(),
               output,
               new StringWriter())
           .ShouldBe(0);

        output.ToString().ShouldContain("updated: waystone.monads/page.md");
    }

    private static int Run(
        TemporaryDirectory root,
        TextWriter output,
        TextWriter error,
        params string[] args) =>
        Cli.Run(
            args,
            root.Path,
            _ => Option.Some(Path.Combine(root.Path, "docs")),
            output,
            error);
}
