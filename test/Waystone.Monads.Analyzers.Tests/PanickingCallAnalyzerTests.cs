namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class PanickingCallAnalyzerTests
{
    [Fact]
    public Task FlagsUnwrapOnAnOption() =>
        Verify.AnalyzerAsync<PanickingCallAnalyzer>(
            "internal int Value(Option<int> option) => option.{|#0:Unwrap|}();",
            Verify.Diagnostic(Rules.UnwrapUsed)
               .WithLocation(0)
               .WithArguments("Unwrap"));

    [Fact]
    public Task FlagsExpectSeparatelyFromUnwrap() =>
        Verify.AnalyzerAsync<PanickingCallAnalyzer>(
            "internal int Value(Option<int> option) => option.{|#0:Expect|}(\"missing\");",
            Verify.Diagnostic(Rules.ExpectUsed)
               .WithLocation(0)
               .WithArguments("Expect"));

    [Fact]
    public Task FlagsUnwrapErrOnAResult() =>
        Verify.AnalyzerAsync<PanickingCallAnalyzer>(
            "internal string Reason(Result<int, string> result) => result.{|#0:UnwrapErr|}();",
            Verify.Diagnostic(Rules.UnwrapUsed)
               .WithLocation(0)
               .WithArguments("UnwrapErr"));

    [Fact]
    public Task FlagsTheAsyncExtensionOverload() =>
        Verify.AnalyzerAsync<PanickingCallAnalyzer>(
            "internal ValueTask<int> Value(Task<Option<int>> option) => option.{|#0:UnwrapAsync|}();",
            Verify.Diagnostic(Rules.UnwrapUsed)
               .WithLocation(0)
               .WithArguments("UnwrapAsync"));

    [Fact]
    public Task IgnoresTheSafeAlternatives() =>
        Verify.NoDiagnosticAsync<PanickingCallAnalyzer>(
            """
            internal int Fallback(Option<int> option) => option.UnwrapOr(0);
            internal int Computed(Option<int> option) => option.UnwrapOrElse(() => 0);
            internal int Defaulted(Option<int> option) => option.UnwrapOrDefault();
            """);

    [Fact]
    public Task IgnoresAnUnrelatedTypeWithTheSameMethodName() =>
        Verify.NoDiagnosticAsync<PanickingCallAnalyzer>(
            """
            internal class Box
            {
                internal int Unwrap() => 1;
                internal int Expect(string message) => 1;
            }

            internal class Subject
            {
                internal int Value(Box box) => box.Unwrap() + box.Expect("no");
            }
            """);
}
