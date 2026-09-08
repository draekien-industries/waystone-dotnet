namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class FreeDelegateAnalyzerTests
{
    [Fact]
    public Task FlagsUnwrapOrElseGivenALiteral() =>
        Verify.AnalyzerAsync<FreeDelegateAnalyzer>(
            """
            internal int Read(Option<int> option) =>
                option.{|#0:UnwrapOrElse|}(() => 0);
            """,
            Verify.Diagnostic(Rules.FreeDelegatePassedToLazyMember)
               .WithLocation(0)
               .WithArguments("UnwrapOrElse", "UnwrapOr"));

    [Fact]
    public Task FlagsOrElseGivenALocal() =>
        Verify.AnalyzerAsync<FreeDelegateAnalyzer>(
            """
            internal Option<int> Pick(Option<int> option)
            {
                var fallback = Option.Some(0);

                return option.{|#0:OrElse|}(() => fallback);
            }
            """,
            Verify.Diagnostic(Rules.FreeDelegatePassedToLazyMember)
               .WithLocation(0)
               .WithArguments("OrElse", "Or"));

    [Fact]
    public Task FlagsAndThenGivenAParameter() =>
        Verify.AnalyzerAsync<FreeDelegateAnalyzer>(
            """
            internal Option<int> Chain(Option<int> option, Option<int> next) =>
                option.{|#0:AndThen|}(value => next);
            """,
            Verify.Diagnostic(Rules.FreeDelegatePassedToLazyMember)
               .WithLocation(0)
               .WithArguments("AndThen", "And"));

    [Fact]
    public Task FlagsOkOrElseGivenAConstant() =>
        Verify.AnalyzerAsync<FreeDelegateAnalyzer>(
            """
            private const string Missing = "missing";

            internal Result<int, string> Convert(Option<int> option) =>
                option.{|#0:OkOrElse|}(() => Missing);
            """,
            Verify.Diagnostic(Rules.FreeDelegatePassedToLazyMember)
               .WithLocation(0)
               .WithArguments("OkOrElse", "OkOr"));

    [Fact]
    public Task FlagsMapOrElseGivenADefaultExpression() =>
        Verify.AnalyzerAsync<FreeDelegateAnalyzer>(
            """
            internal int Measure(Option<string> option) =>
                option.{|#0:MapOrElse|}(() => default(int), text => text.Length);
            """,
            Verify.Diagnostic(Rules.FreeDelegatePassedToLazyMember)
               .WithLocation(0)
               .WithArguments("MapOrElse", "MapOr"));

    /// <remarks>
    /// The delegate's own parameter is ignored, so the rule keys on the body
    /// rather than on the arity. <c>UnwrapOr</c> takes the same value.
    /// </remarks>
    [Fact]
    public Task FlagsADelegateThatIgnoresItsArgument() =>
        Verify.AnalyzerAsync<FreeDelegateAnalyzer>(
            """
            internal int Read(Result<int, string> result) =>
                result.{|#0:UnwrapOrElse|}(error => 0);
            """,
            Verify.Diagnostic(Rules.FreeDelegatePassedToLazyMember)
               .WithLocation(0)
               .WithArguments("UnwrapOrElse", "UnwrapOr"));

    /// <remarks>
    /// The second delegate of <c>MapOrElse</c> is the map, and it has no eager
    /// sibling to move to. Only the first is the deferred value.
    /// </remarks>
    [Fact]
    public Task IgnoresAFreeMapDelegate() =>
        Verify.NoDiagnosticAsync<FreeDelegateAnalyzer>(
            """
            internal int Measure(Option<string> option, int width) =>
                option.MapOrElse(() => Compute(), text => width);

            private static int Compute() => 1;
            """);

    [Fact]
    public Task IgnoresADelegateThatCallsAMethod() =>
        Verify.NoDiagnosticAsync<FreeDelegateAnalyzer>(
            """
            internal int Read(Option<int> option) =>
                option.UnwrapOrElse(() => Compute());

            private static int Compute() => 1;
            """);

    /// <remarks>
    /// The boundary against <c>WM2016</c>'s notion of free. A getter may compute,
    /// and whether it does is only decidable for a symbol in this compilation, so
    /// making the call eager could run arbitrary work unconditionally.
    /// </remarks>
    [Fact]
    public Task IgnoresADelegateThatReadsAProperty() =>
        Verify.NoDiagnosticAsync<FreeDelegateAnalyzer>(
            """
            private int Fallback => 1;

            internal int Read(Option<int> option) =>
                option.UnwrapOrElse(() => Fallback);
            """);

    [Fact]
    public Task IgnoresADelegateThatBuildsAComposite() =>
        Verify.NoDiagnosticAsync<FreeDelegateAnalyzer>(
            """
            internal int Read(Option<int> option, int offset) =>
                option.UnwrapOrElse(() => offset + 1);
            """);

    [Fact]
    public Task IgnoresADelegateWithStatementsBesideTheReturn() =>
        Verify.NoDiagnosticAsync<FreeDelegateAnalyzer>(
            """
            internal int Read(Option<int> option) =>
                option.UnwrapOrElse(
                    () =>
                    {
                        System.Console.WriteLine("falling back");

                        return 0;
                    });
            """);

    [Fact]
    public Task IgnoresAnEagerMemberWhichIsWM2016sToReport() =>
        Verify.NoDiagnosticAsync<FreeDelegateAnalyzer>(
            """
            internal int Read(Option<int> option) => option.UnwrapOr(0);
            """);

    [Fact]
    public Task IgnoresALazyMemberThatIsNotOurs() =>
        Verify.NoDiagnosticAsync<FreeDelegateAnalyzer>(
            """
            internal sealed class Holder
            {
                internal int UnwrapOrElse(System.Func<int> factory) => factory();

                internal int Read() => UnwrapOrElse(() => 0);
            }
            """);
}
