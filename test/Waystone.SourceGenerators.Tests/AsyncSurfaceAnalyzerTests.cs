namespace Waystone.SourceGenerators;

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

/// <remarks>
/// WSG0004 is the mechanism behind DRA-115's audit rather than a convenience: the
/// invariant it enforces — that this library's own return type is assignable to
/// its own step parameter type — was established by a hand count, and a hand count
/// is not repeatable. These cases pin the two edges that decide whether it stays
/// enforceable: what counts as publicly visible, and what counts as a monad.
/// </remarks>
public sealed class AsyncSurfaceAnalyzerTests
{
    [Fact]
    public async Task
        GivenAStepDelegateReturningTask_WhenAnalysed_ThenReportWsg0003()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static ValueTask<Option<int>> AndThenAsync(
                    Func<int, Task<Option<int>>> map) => default;
            }
            """);

        diagnostics.Select(diagnostic => diagnostic.Id)
                   .ShouldBe(["WSG0003"]);
    }

    [Fact]
    public async Task
        GivenAStepDelegateReturningTaskOfResult_WhenAnalysed_ThenReportWsg0003()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static ValueTask<Result<int, string>> AndThenAsync(
                    Func<int, Task<Result<int, string>>> factory) => default;
            }
            """);

        diagnostics.Select(diagnostic => diagnostic.Id)
                   .ShouldBe(["WSG0003"]);
    }

    [Fact]
    public async Task
        GivenAStepDelegate_WhenAnalysed_ThenNameTheParameterAndTheFix()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static ValueTask<Option<int>> AndThenAsync(
                    Func<int, Task<Option<int>>> map) => default;
            }
            """);

        string message = diagnostics.Single().GetMessage();

        message.ShouldContain("'map'");
        message.ShouldContain("'Subject.AndThenAsync'");
        message.ShouldContain("'Task<Option<int>>'");
        message.ShouldContain("'ValueTask<Option<int>>'");
        message.ShouldNotContain("{0}");
    }

    /// <summary>
    /// The rule this analyzer enforces is typed by provenance: a delegate returning
    /// one of our monads takes <c>ValueTask</c>, because only our chains produce
    /// that shape, and a delegate returning an arbitrary type keeps <c>Task</c>,
    /// because foreign code produces that. Reporting this case would force
    /// <c>MapAsync(async u =&gt; await client.GetStringAsync(u))</c> at every
    /// boundary in every consumer.
    /// </summary>
    [Fact]
    public async Task
        GivenABoundaryDelegateReturningTask_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static ValueTask<Option<string>> MapAsync(
                    Func<int, Task<string>> map) => default;

                public static ValueTask<Option<int>> FilterAsync(
                    Func<int, Task<bool>> predicate) => default;
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task
        GivenAStepDelegateReturningValueTask_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static ValueTask<Option<int>> AndThenAsync(
                    Func<int, ValueTask<Option<int>>> map) => default;
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    /// <summary>
    /// A delegate returning the non-generic <c>Task</c> wraps nothing, so there is
    /// no type argument to inspect.
    /// </summary>
    [Fact]
    public async Task
        GivenADelegateReturningNonGenericTask_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static ValueTask InspectAsync(Func<int, Task> inspect) =>
                    default;
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task GivenANonDelegateParameter_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static ValueTask<Option<int>> OrAsync(Option<int> other) =>
                    default;
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    /// <summary>
    /// The check is on the delegate's invoke signature rather than on <c>Func</c>
    /// by name, so a named delegate type is caught the same way.
    /// </summary>
    [Fact]
    public async Task GivenANamedDelegateType_WhenAnalysed_ThenReportWsg0003()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public delegate Task<Option<int>> Step(int value);

            public static class Subject
            {
                public static ValueTask<Option<int>> AndThenAsync(Step map) =>
                    default;
            }
            """);

        diagnostics.Select(diagnostic => diagnostic.Id)
                   .ShouldBe(["WSG0003"]);
    }

    [Fact]
    public async Task GivenTaskOfOption_WhenAnalysed_ThenReportWsg0004()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static Task<Option<int>> FetchAsync() => null!;
            }
            """);

        diagnostics.Select(diagnostic => diagnostic.Id)
                   .ShouldBe(["WSG0004"]);
    }

    [Fact]
    public async Task GivenTaskOfResult_WhenAnalysed_ThenReportWsg0004()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static Task<Result<int, string>> FetchAsync() => null!;
            }
            """);

        diagnostics.Select(diagnostic => diagnostic.Id)
                   .ShouldBe(["WSG0004"]);
    }

    /// <summary>
    /// The message carries three substituted arguments, and a count mismatch renders
    /// as a literal placeholder that no other assertion here would catch.
    /// </summary>
    [Fact]
    public async Task GivenTaskOfOption_WhenAnalysed_ThenNameTheMemberAndTheFix()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static Task<Option<int>> FetchAsync() => null!;
            }
            """);

        string message = diagnostics.Single().GetMessage();

        message.ShouldContain("'Subject.FetchAsync'");
        message.ShouldContain("'Task<Option<int>>'");
        message.ShouldContain("'ValueTask<Option<int>>'");
        message.ShouldNotContain("{0}");
    }

    [Fact]
    public async Task GivenValueTaskOfOption_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static ValueTask<Option<int>> FetchAsync() => default;
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task GivenTaskOfAForeignType_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static Task<string> FetchAsync() => null!;
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    /// <summary>
    /// A boundary-shaped return whose type argument is not a named type at all,
    /// which is the branch a monad check reached through a cast would miss.
    /// </summary>
    [Fact]
    public async Task GivenTaskOfAnArray_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static Task<Option<int>[]> FetchAsync() => null!;
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task GivenAnArrayReturn_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                public static Option<int>[] Fetch() => null!;
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    /// <summary>
    /// The rule governs the published surface, so an internal member is out of
    /// scope however it is declared.
    /// </summary>
    [Fact]
    public async Task GivenAnInternalMember_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            public static class Subject
            {
                internal static Task<Option<int>> FetchAsync() => null!;
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    /// <summary>
    /// A public member of an internal type is not publicly visible, which the walk
    /// up the containing types is what establishes.
    /// </summary>
    [Fact]
    public async Task GivenAPublicMemberOfAnInternalType_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            internal static class Subject
            {
                public static Task<Option<int>> FetchAsync() => null!;
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task
        GivenAPublicMemberOfAPublicTypeNestedInAnInternalType_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            internal static class Outer
            {
                public static class Subject
                {
                    public static Task<Option<int>> FetchAsync() => null!;
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    /// <summary>
    /// A compilation that does not reference this library has no monads to compare
    /// against, so the rule loads nothing and walks no symbols.
    /// </summary>
    /// <remarks>
    /// The subject declares its own <c>Option</c> in the real namespace and no
    /// <c>Result</c> at all, so the return type below is one the rule would report
    /// if it loaded a partial set. Asserting against a foreign return type instead
    /// would pass whether the guard existed or not.
    /// </remarks>
    [Fact]
    public async Task GivenNoReferenceToTheLibrary_WhenAnalysed_ThenReportNothing()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAnalyzer.Run(
            """
            namespace Waystone.Monads.Options
            {
                public sealed class Option<T> { }
            }

            public static class Subject
            {
                public static Task<Options.Option<int>> FetchAsync() => null!;
            }
            """,
            false);

        diagnostics.ShouldBeEmpty();
    }
}
