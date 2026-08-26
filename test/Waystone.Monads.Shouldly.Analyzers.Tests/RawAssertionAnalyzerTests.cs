namespace Waystone.Monads.Shouldly.Analyzers;

using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// The message arguments are asserted on every reported case, not just the location.
/// The third argument is the assertion the fix will write, and it reaches the fix
/// through the diagnostic's own properties — so a case whose message names the wrong
/// replacement is a case whose fix writes the wrong call.
/// </remarks>
public class RawAssertionAnalyzerTests
{
    [Theory]
    [InlineData("option.IsSome.ShouldBeTrue()", "IsSome", "Option<int>", "ShouldBeSome")]
    [InlineData("option.IsNone.ShouldBeFalse()", "IsNone", "Option<int>", "ShouldBeSome")]
    [InlineData("option.IsNone.ShouldBeTrue()", "IsNone", "Option<int>", "ShouldBeNone")]
    [InlineData("option.IsSome.ShouldBeFalse()", "IsSome", "Option<int>", "ShouldBeNone")]
    [InlineData("result.IsOk.ShouldBeTrue()", "IsOk", "Result<int, string>", "ShouldBeOk")]
    [InlineData("result.IsErr.ShouldBeFalse()", "IsErr", "Result<int, string>", "ShouldBeOk")]
    [InlineData("result.IsErr.ShouldBeTrue()", "IsErr", "Result<int, string>", "ShouldBeErr")]
    [InlineData("result.IsOk.ShouldBeFalse()", "IsOk", "Result<int, string>", "ShouldBeErr")]
    public Task GivenAStateAssertedThroughABool_ThenNameTheReplacement(
        string expression,
        string member,
        string monad,
        string replacement) =>
        Verify.AnalyzerAsync<RawAssertionAnalyzer>(
            Subject(expression),
            Verify.Diagnostic(Rules.RawAssertion)
               .WithLocation(0)
               .WithArguments(member, monad, replacement));

    [Theory]
    [InlineData("option.Unwrap().ShouldBe(3)", "Unwrap", "Option<int>", "ShouldBeSomeValue")]
    [InlineData("result.Unwrap().ShouldBe(3)", "Unwrap", "Result<int, string>", "ShouldBeOkValue")]
    [InlineData("result.UnwrapErr().ShouldBe(\"failed\")", "UnwrapErr", "Result<int, string>", "ShouldBeErrValue")]
    public Task GivenAValueAssertedThroughAnUnwrap_ThenNameTheReplacement(
        string expression,
        string member,
        string monad,
        string replacement) =>
        Verify.AnalyzerAsync<RawAssertionAnalyzer>(
            Subject(expression),
            Verify.Diagnostic(Rules.RawAssertion)
               .WithLocation(0)
               .WithArguments(member, monad, replacement));

    /// <summary>
    /// A custom message is carried onto the replacement, so its presence does not stop
    /// the rule.
    /// </summary>
    [Theory]
    [InlineData("option.IsSome.ShouldBeTrue(\"while loading\")", "IsSome", "ShouldBeSome")]
    [InlineData("option.Unwrap().ShouldBe(3, \"while loading\")", "Unwrap", "ShouldBeSomeValue")]
    public Task GivenACustomMessage_ThenStillReport(
        string expression,
        string member,
        string replacement) =>
        Verify.AnalyzerAsync<RawAssertionAnalyzer>(
            Subject(expression),
            Verify.Diagnostic(Rules.RawAssertion)
               .WithLocation(0)
               .WithArguments(member, "Option<int>", replacement));

    /// <summary>
    /// The concrete-type assertion is deliberately not a trigger.
    /// </summary>
    /// <remarks>
    /// A test that asserts <c>Some&lt;int&gt;</c> is usually testing the closed
    /// hierarchy itself, and nothing in the syntax distinguishes that from an
    /// incidental type check. Rewriting it to <c>ShouldBeSome</c> would delete the only
    /// coverage of the hierarchy, so the shape is excluded rather than handled.
    /// </remarks>
    [Theory]
    [InlineData("option.ShouldBeOfType<Some<int>>()")]
    [InlineData("result.ShouldBeOfType<Ok<int, string>>()")]
    public Task GivenAConcreteTypeAssertion_ThenReportNothing(string expression) =>
        Verify.NoDiagnosticAsync<RawAssertionAnalyzer>(
            Unmarked(expression));

    [Fact]
    public Task GivenAStateReadThatIsNotAsserted_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<RawAssertionAnalyzer>(
            """
            private bool Check(Option<int> option) => option.IsSome;
            """);

    /// <summary>
    /// The receiver's type is what the rule keys on, so a foreign type that happens to
    /// expose the same members is not reported.
    /// </summary>
    [Fact]
    public Task GivenAMonadShapedTypeThatIsNotAMonad_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<RawAssertionAnalyzer>(
            """
            internal sealed class Lookalike
            {
                public bool IsSome => true;

                public int Unwrap() => 3;
            }

            internal class Subject
            {
                private void Check(Lookalike fake)
                {
                    fake.IsSome.ShouldBeTrue();
                    fake.Unwrap().ShouldBe(3);
                }
            }
            """);

    /// <summary>
    /// A bool read off a monad that names no state is not reported.
    /// </summary>
    /// <remarks>
    /// Reaches the state lookup and misses, which is the arm that keeps the rule off
    /// every other boolean member a monad might grow. The lookup runs before the
    /// receiver's type is checked, so this case and the lookalike above cover the two
    /// sides of the same guard.
    /// </remarks>
    [Fact]
    public Task GivenABoolThatNamesNoState_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<RawAssertionAnalyzer>(
            """
            internal sealed class Holder
            {
                public bool Flag => true;
            }

            internal class Subject
            {
                private void Check(Holder holder)
                {
                    holder.Flag.ShouldBeTrue();
                }
            }
            """);

    /// <remarks>
    /// Reaches the rule's name lookup with a member that names no state and no unwrap,
    /// which is the arm that keeps it off every other boolean and every other zero
    /// argument call in a test suite.
    /// </remarks>
    [Fact]
    public Task GivenAnUnrelatedMember_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<RawAssertionAnalyzer>(
            """
            private void Check(Option<int> option, bool flag)
            {
                flag.ShouldBeTrue();
                option.ToString().ShouldBe("Some");
            }
            """);

    /// <remarks>
    /// The state read has to be a member access. A combinator returns the same bool
    /// through an invocation, and there is no receiver to hang the replacement on.
    /// </remarks>
    [Fact]
    public Task GivenAStateCombinator_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<RawAssertionAnalyzer>(
            """
            private void Check(Option<int> option)
            {
                option.IsSomeAnd(value => value > 0).ShouldBeTrue();
            }
            """);

    /// <remarks>
    /// The unwrap has to be an invocation with no arguments, so a comparison on a plain
    /// local is not mistaken for one.
    /// </remarks>
    [Fact]
    public Task GivenAComparisonOnAPlainValue_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<RawAssertionAnalyzer>(
            """
            private void Check(string text, Option<int> option)
            {
                text.ShouldBe("failed");
                option.UnwrapOr(0).ShouldBe(3);
            }
            """);

    /// <summary>
    /// An overload whose extra argument is not a custom message is not reported.
    /// </summary>
    /// <remarks>
    /// The tolerance, comparer and ignore-order overloads all describe how to compare a
    /// bare value, and the assertions that take the monad have no counterpart for them.
    /// Forwarding the argument list positionally onto <c>ShouldBeSomeValue</c> would
    /// either drop the tolerance or fail to compile, so the rule stays silent.
    /// </remarks>
    [Fact]
    public Task GivenAComparisonOverloadWithItsOwnOptions_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<RawAssertionAnalyzer>(
            """
            private void Check(Option<double> option)
            {
                option.Unwrap().ShouldBe(3.0, 0.1);
            }
            """);

    /// <remarks>
    /// Keyed on the <c>Shouldly</c> namespace, so a project carrying its own
    /// <c>ShouldBe</c> is left alone.
    /// </remarks>
    [Fact]
    public Task GivenAnAssertionThatIsNotFromShouldly_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<RawAssertionAnalyzer>(
            """
            internal static class Home
            {
                public static void ShouldBeTrue(this bool actual) { }

                public static void ShouldBe<T>(this T actual, T expected) { }
            }

            internal class Subject
            {
                private void Check(Option<int> option)
                {
                    Home.ShouldBeTrue(option.IsSome);
                    Home.ShouldBe(option.Unwrap(), 3);
                }
            }
            """);

    [Fact]
    public Task GivenANegatedAssertion_ThenReportNothing() =>
        Verify.NoDiagnosticAsync<RawAssertionAnalyzer>(
            """
            private void Check(Option<int> option)
            {
                option.Unwrap().ShouldNotBe(4);
            }
            """);

    /// <summary>
    /// A project without the assertions package gets nothing at all.
    /// </summary>
    /// <remarks>
    /// This is the case that decides where the rule ships. Every consumer of the core
    /// library writes assertions like these and most will never reference this package;
    /// a diagnostic there would name a method they cannot call.
    /// </remarks>
    [Fact]
    public Task GivenTheAssertionsPackageIsAbsent_ThenReportNothing() =>
        Verify.WithoutAssertionsAsync<RawAssertionAnalyzer>(
            """
            private void Check(Option<int> option, Result<int, string> result)
            {
                option.IsSome.ShouldBeTrue();
                option.Unwrap().ShouldBe(3);
                result.IsOk.ShouldBeTrue();
            }
            """);

    private static string Subject(string expression) =>
        $$"""
          private void Check(Option<int> option, Result<int, string> result)
          {
              {|#0:{{expression}}|};
          }
          """;

    private static string Unmarked(string expression) =>
        $$"""
          private void Check(Option<int> option, Result<int, string> result)
          {
              {{expression}};
          }
          """;
}
