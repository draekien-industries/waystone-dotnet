namespace Waystone.DocSnippets;

using Shouldly;
using System.IO;
using Waystone.Monads.Options;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class LocatorTests
{
    [Fact]
    public void FindsTheRepositoryRootFromADirectoryInsideIt()
    {
        using TemporaryDirectory root = new();
        Directory.CreateDirectory(Path.Combine(root.Path, ".git"));
        string deep = Directory.CreateDirectory(Path.Combine(root.Path, "src", "thing")).FullName;

        Locator.RepositoryRoot(deep).ShouldBeOkValue(root.Path);
    }

    [Fact]
    public void TreatsAGitFileAsARootSoAWorktreeResolves()
    {
        using TemporaryDirectory root = new();
        File.WriteAllText(Path.Combine(root.Path, ".git"), "gitdir: elsewhere");

        Locator.RepositoryRoot(root.Path).ShouldBeOkValue(root.Path);
    }

    [Fact]
    public void RefusesADirectoryOutsideAnyRepository()
    {
        using TemporaryDirectory root = new();

        Error error = Locator.RepositoryRoot(root.Path).ShouldBeErr();

        error.Code.ShouldBe(DocSnippetError.NotInARepository.ToErrorCode());
        error.Message.ShouldContain("not inside a git repository");
    }

    [Fact]
    public void TakesTheFirstCandidateThatHoldsTheSpace()
    {
        using TemporaryDirectory root = new();
        string docs = Directory.CreateDirectory(Path.Combine(root.Path, "docs")).FullName;
        Directory.CreateDirectory(Path.Combine(docs, "waystone.monads"));

        Locator
           .Resolve(
                [
                    new Candidate("--docs", Option.None<string>()),
                    new Candidate("$WAYSTONE_DOCS_PATH", Option.Some("")),
                    new Candidate("a sibling", Option.Some(root.Path)),
                    new Candidate("last", Option.Some(docs)),
                ])
           .ShouldBeOkValue(docs);
    }

    [Fact]
    public void NamesEveryCandidateWhenNoneResolves()
    {
        using TemporaryDirectory root = new();

        Error error = Locator
                     .Resolve(
                          [
                              new Candidate("--docs", Option.None<string>()),
                              new Candidate("a sibling", Option.Some(root.Path)),
                          ])
                     .ShouldBeErr();

        error.Code.ShouldBe(DocSnippetError.DocumentationRepositoryNotFound.ToErrorCode());
        error.Message.ShouldContain("--docs: not set");
        error.Message.ShouldContain("no waystone.monads directory there");
        error.Message.ShouldContain(Locator.EnvironmentVariable);
        error.Message.ShouldContain(Locator.GitConfigKey);
    }

    [Fact]
    public void RecognisesTheDocumentationRepositoryByItsSpace()
    {
        using TemporaryDirectory root = new();

        Locator.IsDocumentationRepository(root.Path).ShouldBeFalse();
        Directory.CreateDirectory(Path.Combine(root.Path, "waystone.monads"));
        Locator.IsDocumentationRepository(root.Path).ShouldBeTrue();
    }
}
