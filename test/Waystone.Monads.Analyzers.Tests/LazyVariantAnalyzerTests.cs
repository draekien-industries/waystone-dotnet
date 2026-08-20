namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class LazyVariantAnalyzerTests
{
    [Fact]
    public Task FlagsOrWithACall() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal Option<int> Fallback() => Option.Some(0);

            internal Option<int> Pick(Option<int> option) =>
                option.{|#0:Or|}(Fallback());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("Or", "Option<int>", "OrElse"));

    [Fact]
    public Task FlagsAndWithACall() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal Option<string> Next() => Option.Some("x");

            internal Option<string> Pick(Option<int> option) =>
                option.{|#0:And|}(Next());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("And", "Option<int>", "AndThen"));

    [Fact]
    public Task FlagsUnwrapOrWithACall() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal int Expensive() => 42;

            internal int Read(Option<int> option) =>
                option.{|#0:UnwrapOr|}(Expensive());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<int>", "UnwrapOrElse"));

    [Fact]
    public Task FlagsMapOrOnItsFirstArgument() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal int Expensive() => 42;

            internal int Read(Option<int> option) =>
                option.{|#0:MapOr|}(Expensive(), value => value + 1);
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("MapOr", "Option<int>", "MapOrElse"));

    [Fact]
    public Task FlagsOkOrWithACall() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal string Reason() => "missing";

            internal Result<int, string> Convert(Option<int> option) =>
                option.{|#0:OkOr|}(Reason());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("OkOr", "Option<int>", "OkOrElse"));

    [Fact]
    public Task FlagsAResultReceiver() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal int Expensive() => 42;

            internal int Read(Result<int, string> result) =>
                result.{|#0:UnwrapOr|}(Expensive());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Result<int, string>", "UnwrapOrElse"));

    [Fact]
    public Task FlagsAnObjectCreation() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal Option<string> Pick(Option<string> option) =>
                option.{|#0:Or|}(Option.Some(new string('x', 3)));
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("Or", "Option<string>", "OrElse"));

    [Fact]
    public Task FlagsAnAsyncWrapper() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal int Expensive() => 42;

            internal ValueTask<int> Read(Task<Option<int>> option) =>
                option.{|#0:UnwrapOrAsync|}(Expensive());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments(
                    "UnwrapOrAsync",
                    "Task<Option<int>>",
                    "UnwrapOrElseAsync"));

    [Fact]
    public Task IgnoresALiteral() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal int Read(Option<int> option) => option.UnwrapOr(0);
            """);

    [Fact]
    public Task IgnoresALocal() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal int Read(Option<int> option)
            {
                int fallback = 42;

                return option.UnwrapOr(fallback);
            }
            """);

    [Fact]
    public Task IgnoresAParameter() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal int Read(Option<int> option, int fallback) =>
                option.UnwrapOr(fallback);
            """);

    [Fact]
    public Task IgnoresAField() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            private readonly int _fallback = 42;

            internal int Read(Option<int> option) =>
                option.UnwrapOr(_fallback);
            """);

    [Fact]
    public Task IgnoresAPropertyRead() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            private int Fallback => 42;

            internal int Read(Option<int> option) =>
                option.UnwrapOr(Fallback);
            """);

    [Fact]
    public Task IgnoresADefault() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal Option<int> Pick(Option<int> option) =>
                option.Or(default(Option<int>));
            """);

    [Fact]
    public Task IgnoresTheLazySiblings() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal Option<int> Fallback() => Option.Some(0);

            internal Option<int> Pick(Option<int> option) =>
                option.OrElse(() => Fallback());
            """);

    [Fact]
    public Task IgnoresAnUnrelatedOr() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal static class Choice
            {
                internal static int Or(this int value, int other) => other;
            }

            internal class Subject
            {
                internal int Expensive() => 42;

                internal int Read(int value) => value.Or(Expensive());
            }
            """);
}
