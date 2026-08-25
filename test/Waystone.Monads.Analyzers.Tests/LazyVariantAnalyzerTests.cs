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
               .WithArguments("Or", "Option<int>", "OrElse",
                    "and computing it may be expensive"));

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
               .WithArguments("And", "Option<int>", "AndThen",
                    "and computing it may be expensive"));

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
               .WithArguments("UnwrapOr", "Option<int>", "UnwrapOrElse",
                    "and computing it may be expensive"));

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
               .WithArguments("MapOr", "Option<int>", "MapOrElse",
                    "and computing it may be expensive"));

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
               .WithArguments("OkOr", "Option<int>", "OkOrElse",
                    "and computing it may be expensive"));

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
               .WithArguments("UnwrapOr", "Result<int, string>", "UnwrapOrElse",
                    "and computing it may be expensive"));

    [Fact]
    public Task FlagsAnObjectCreation() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal Option<string> Pick(Option<string> option) =>
                option.{|#0:Or|}(Option.Some(new string('x', 3)));
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("Or", "Option<string>", "OrElse",
                    "and computing it may be expensive"));

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
                    "UnwrapOrElseAsync",
                    "and computing it may be expensive"));

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
    public Task IgnoresArithmeticOverFreeOperands() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal int Read(Option<int> option, int fallback) =>
                option.UnwrapOr(fallback + 1);
            """);

    [Fact]
    public Task IgnoresAnArrayElement() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            private readonly int[] _defaults = new int[4];

            internal int Read(Option<int> option) =>
                option.UnwrapOr(_defaults[0]);
            """);

    [Fact]
    public Task IgnoresATernaryOverFreeOperands() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal int Read(Option<int> option, bool flag, int a, int b) =>
                option.UnwrapOr(flag ? a : b);
            """);

    [Fact]
    public Task FlagsACallInsideAnOtherwiseFreeExpression() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal int Expensive() => 42;

            internal int Read(Option<int> option, int fallback) =>
                option.{|#0:UnwrapOr|}(fallback + Expensive());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<int>", "UnwrapOrElse",
                    "and computing it may be expensive"));

    [Fact]
    public Task FlagsAUserDefinedOperator() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal readonly struct Money
            {
                public static Money operator +(Money a, Money b) => default;
            }

            internal class Subject
            {
                internal Money Read(Option<Money> option, Money a, Money b) =>
                    option.{|#0:UnwrapOr|}(a + b);
            }
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<Money>", "UnwrapOrElse",
                    "and computing it may be expensive"));

    [Fact]
    public Task FlagsAUserDefinedImplicitConversion() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal readonly struct Money
            {
                public static implicit operator Money(int value) => default;
            }

            internal class Subject
            {
                internal Money Read(Option<Money> option, int fallback) =>
                    option.{|#0:UnwrapOr|}(fallback);
            }
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<Money>", "UnwrapOrElse",
                    "and computing it may be expensive"));

    [Fact]
    public Task FlagsAnIncrement() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            private int _next;

            internal int Read(Option<int> option) =>
                option.{|#0:UnwrapOr|}(_next++);
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<int>", "UnwrapOrElse",
                    "and evaluating it changes state"));

    /// <remarks>
    /// A struct default has no constant value, unlike <c>default(int)</c> and
    /// <c>default(string)</c>, so this is the case that reaches the
    /// <c>IDefaultValueOperation</c> arm rather than the constant check above
    /// it.
    /// </remarks>
    [Fact]
    public Task IgnoresAStructDefault() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal System.Guid Read(Option<System.Guid> option) =>
                option.UnwrapOr(default(System.Guid));
            """);

    [Fact]
    public Task IgnoresThis() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal Subject Read(Option<Subject> option) =>
                option.UnwrapOr(this);
            """);

    [Fact]
    public Task IgnoresANegation() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal int Read(Option<int> option, int fallback) =>
                option.UnwrapOr(-fallback);
            """);

    [Fact]
    public Task IgnoresACoalesceOverFreeOperands() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal string Read(Option<string> option, string? a, string b) =>
                option.UnwrapOr(a ?? b);
            """);

    [Fact]
    public Task IgnoresATupleOfFreeOperands() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal (int, int) Read(Option<(int, int)> option, int a, int b) =>
                option.UnwrapOr((a, b));
            """);

    [Fact]
    public Task IgnoresRedundantParentheses() =>
        Verify.NoDiagnosticAsync<LazyVariantAnalyzer>(
            """
            internal int Read(Option<int> option, int fallback) =>
                option.UnwrapOr((fallback + 1));
            """);

    [Fact]
    public Task FlagsACallInsideATuple() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal int Expensive() => 42;

            internal (int, int) Read(Option<(int, int)> option, int a) =>
                option.{|#0:UnwrapOr|}((Expensive(), a));
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<(int, int)>", "UnwrapOrElse",
                    "and computing it may be expensive"));

    [Fact]
    public Task FlagsACallInsideAnArrayIndex() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            private readonly int[] _defaults = new int[4];

            internal int Which() => 2;

            internal int Read(Option<int> option) =>
                option.{|#0:UnwrapOr|}(_defaults[Which()]);
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<int>", "UnwrapOrElse",
                    "and computing it may be expensive"));

    [Fact]
    public Task FlagsACallInsideATernary() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal int Expensive() => 42;

            internal int Read(Option<int> option, bool flag, int a) =>
                option.{|#0:UnwrapOr|}(flag ? a : Expensive());
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<int>", "UnwrapOrElse",
                    "and computing it may be expensive"));

    [Fact]
    public Task FlagsAUserDefinedUnaryOperator() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            internal readonly struct Money
            {
                public static Money operator -(Money value) => default;
            }

            internal class Subject
            {
                internal Money Read(Option<Money> option, Money amount) =>
                    option.{|#0:UnwrapOr|}(-amount);
            }
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<Money>", "UnwrapOrElse",
                    "and computing it may be expensive"));

    [Fact]
    public Task FlagsAnAssignment() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            private int _cached;

            internal int Read(Option<int> option, int fallback) =>
                option.{|#0:UnwrapOr|}(_cached = fallback);
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<int>", "UnwrapOrElse",
                    "and evaluating it changes state"));

    [Fact]
    public Task FlagsACompoundAssignment() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            private int _total;

            internal int Read(Option<int> option, int fallback) =>
                option.{|#0:UnwrapOr|}(_total += fallback);
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<int>", "UnwrapOrElse",
                    "and evaluating it changes state"));

    [Fact]
    public Task FlagsACoalesceAssignment() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            private string? _cached;

            internal string Read(Option<string> option, string fallback) =>
                option.{|#0:UnwrapOr|}(_cached ??= fallback);
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<string>", "UnwrapOrElse",
                    "and evaluating it changes state"));

    [Fact]
    public Task FlagsAMutationInsideATernary() =>
        Verify.AnalyzerAsync<LazyVariantAnalyzer>(
            """
            private int _n;

            internal int Read(Option<int> option, bool flag, int a) =>
                option.{|#0:UnwrapOr|}(flag ? a : _n++);
            """,
            Verify.Diagnostic(Rules.EagerArgumentNotFree)
               .WithLocation(0)
               .WithArguments("UnwrapOr", "Option<int>", "UnwrapOrElse",
                    "and evaluating it changes state"));

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
