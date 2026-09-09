namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class BoundStateAnalyzerTests
{
    [Fact]
    public Task FlagsAnOptionBoundAsState() =>
        Verify.AnalyzerAsync<BoundStateAnalyzer>(
            """
            internal Option<int> Combine(Option<int> option, Option<int> other) =>
                option.{|#0:With|}(other)
                      .Map(static (value, second) => value + second.UnwrapOr(0));
            """,
            Verify.Diagnostic(Rules.OptionBoundAsState)
               .WithLocation(0)
               .WithArguments("Option<int>"));

    [Fact]
    public Task NamesTheBoundTypeInTheMessage() =>
        Verify.AnalyzerAsync<BoundStateAnalyzer>(
            """
            internal Option<string> Combine(
                Option<string> option,
                Option<string> other) =>
                option.{|#0:With|}(other)
                      .Map(static (value, second) => value + second.UnwrapOr(""));
            """,
            Verify.Diagnostic(Rules.OptionBoundAsState)
               .WithLocation(0)
               .WithArguments("Option<string>"));

    /// <remarks>
    /// A <c>Some</c> or <c>None</c> held in its own type is the same mistake, and
    /// <c>IsOption</c> alone does not see one — it asks what the type was
    /// constructed from, and these were constructed from the case rather than from
    /// <c>Option</c>. The message names the type as the author wrote it.
    /// </remarks>
    [Fact]
    public Task FlagsADerivedCaseBoundAsState() =>
        Verify.AnalyzerAsync<BoundStateAnalyzer>(
            """
            internal Option<int> Combine(Option<int> option, Some<int> other) =>
                option.{|#0:With|}(other)
                      .Map(static (value, second) => value + second.UnwrapOr(0));
            """,
            Verify.Diagnostic(Rules.OptionBoundAsState)
               .WithLocation(0)
               .WithArguments("Some<int>"));

    [Fact]
    public Task IgnoresAPlainValueBoundAsState() =>
        Verify.NoDiagnosticAsync<BoundStateAnalyzer>(
            """
            internal Option<int> Shift(Option<int> option, int offset) =>
                option.With(offset).Map(static (value, state) => value + state);
            """);

    /// <remarks>
    /// The deliberate exclusion. Result has no Zip or ZipWith, so binding one is
    /// the capture-free spelling rather than a mistake, and reporting it would
    /// contradict <c>WM2017</c>. See the remarks on the descriptor.
    /// </remarks>
    [Fact]
    public Task IgnoresAResultBoundAsStateOnAResult() =>
        Verify.NoDiagnosticAsync<BoundStateAnalyzer>(
            """
            internal void Combine(
                Result<int, string> result,
                Result<int, string> other)
            {
                _ = result.With(other);
            }
            """);

    [Fact]
    public Task IgnoresAResultBoundAsStateOnAnOption() =>
        Verify.NoDiagnosticAsync<BoundStateAnalyzer>(
            """
            internal void Combine(Option<int> option, Result<int, string> other)
            {
                _ = option.With(other);
            }
            """);

    /// <remarks>
    /// The factory binder is nested in the static <c>Option</c> class rather than
    /// in <c>Option&lt;T&gt;</c>, and there is no receiving option to zip against.
    /// </remarks>
    [Fact]
    public Task IgnoresTheFactoryBinder() =>
        Verify.NoDiagnosticAsync<BoundStateAnalyzer>(
            """
            internal void Bind(Option<int> other)
            {
                _ = Option.With(other);
            }
            """);

    [Fact]
    public Task IgnoresAWithThatIsNotOurs() =>
        Verify.NoDiagnosticAsync<BoundStateAnalyzer>(
            """
            internal sealed class Holder
            {
                internal Holder With(Option<int> option) => this;

                internal Holder Bind(Option<int> option) => With(option);
            }
            """);
}
