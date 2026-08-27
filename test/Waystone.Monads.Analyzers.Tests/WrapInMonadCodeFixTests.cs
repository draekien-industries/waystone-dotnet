namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

public class WrapInMonadCodeFixTests
{
    [Fact]
    public Task WrapsAValueBoundForAnOption() =>
        Fixed(
            """
            internal Option<int> Widen() => {|#0:5|};
            """,
            """
            internal Option<int> Widen() => Option.Some<int>(5);
            """);

    [Fact]
    public Task WrapsAValueBoundForTheOkSideOfAResult() =>
        Fixed(
            """
            internal Result<int, Error> Widen() => {|#0:5|};
            """,
            """
            internal Result<int, Error> Widen() => Result.Ok<int, Error>(5);
            """);

    /// <remarks>
    /// The error side takes a different member for the same shape of assignment, and
    /// which one applies is readable only from the target's type arguments.
    /// </remarks>
    [Fact]
    public Task WrapsAValueBoundForTheErrorSideOfAResult() =>
        Fixed(
            """
            internal Result<int, Error> Widen() =>
                {|#0:new Error("order.refused", "refused")|};
            """,
            """
            internal Result<int, Error> Widen() =>
                Result.Err<int, Error>(new Error("order.refused", "refused"));
            """);

    /// <remarks>
    /// A conversion in argument position reports CS1503 rather than CS0029 and reads
    /// its target from the parameter rather than from the expression, so it is the
    /// second of the two shapes the removed conversion left behind.
    /// </remarks>
    [Fact]
    public Task WrapsAValuePassedToAParameter() =>
        Verify.CompilerCodeFixAsync<WrapInMonadCodeFix>(
            """
            internal Option<int> Passed() => Widen({|#0:7|});

            private static Option<int> Widen(Option<int> option) => option;
            """,
            """
            internal Option<int> Passed() => Widen(Option.Some<int>(7));

            private static Option<int> Widen(Option<int> option) => option;
            """,
            new[] { Mismatch("CS1503", 0) },
            Array.Empty<DiagnosticResult>());

    /// <remarks>
    /// A delegate invocation reaches overload resolution with a candidate that is not
    /// a method at all, so the target has to come from the converted type rather than
    /// from a parameter.
    /// </remarks>
    [Fact]
    public Task WrapsAValuePassedToADelegate() =>
        Verify.CompilerCodeFixAsync<WrapInMonadCodeFix>(
            """
            internal Option<int> Passed(Func<Option<int>, Option<int>> widen) =>
                widen({|#0:7|});
            """,
            """
            internal Option<int> Passed(Func<Option<int>, Option<int>> widen) =>
                widen(Option.Some<int>(7));
            """,
            new[] { Mismatch("CS1503", 0) },
            Array.Empty<DiagnosticResult>());

    /// <remarks>
    /// The fully qualified factory is what compiles when the namespace is not
    /// imported, and a fix that emitted the short name would trade one error for
    /// another.
    /// </remarks>
    [Fact]
    public Task QualifiesTheFactoryWhenItIsNotInScope() =>
        Verify.RawCodeFixAsync<EmptyDiagnosticAnalyzer, WrapInMonadCodeFix>(
            """
            internal class Subject
            {
                internal Waystone.Monads.Options.Option<int> Widen() => {|#0:5|};
            }
            """,
            """
            internal class Subject
            {
                internal Waystone.Monads.Options.Option<int> Widen() => Waystone.Monads.Options.Option.Some<int>(5);
            }
            """,
            Mismatch("CS0029", 0));

    /// <remarks>
    /// Some's factory returns the base Option, so wrapping would leave the same
    /// conversion error one type along. The derived cases get no fix at all.
    /// </remarks>
    [Fact]
    public Task LeavesADerivedCaseTargetAlone() =>
        Unfixed(
            """
            internal Some<int> Widen() => {|#0:5|};
            """);

    /// <remarks>
    /// The value has to match one of the target's type arguments. A source that
    /// matches neither is an ordinary mistake rather than a removed conversion.
    /// </remarks>
    [Fact]
    public Task LeavesAValueThatMatchesNoTypeArgumentAlone() =>
        Unfixed(
            """
            internal Option<string> Widen() => {|#0:5|};
            """);

    /// <remarks>
    /// CS0029 is the most common error in C#, so the fix has to stay quiet on every
    /// target that is not one of this library's monads.
    /// </remarks>
    [Fact]
    public Task LeavesAnUnrelatedConversionAlone() =>
        Unfixed(
            """
            internal int Widen() => {|#0:"five"|};
            """);

    /// <remarks>
    /// An array of a monad is not a monad, and reading its type arguments would find
    /// none — it is not a named type at all.
    /// </remarks>
    [Fact]
    public Task LeavesAnArrayOfAMonadAlone() =>
        Unfixed(
            """
            internal Option<int>[] Widen() => {|#0:5|};
            """);

    /// <remarks>
    /// A method group has no type for the semantic model to match against a type
    /// argument, so the fix declines before it reads the target. In argument
    /// position it arrives as the same CS1503 the fix does handle, rather than as
    /// the CS0428 a return position reports.
    /// </remarks>
    [Fact]
    public Task LeavesAMethodGroupArgumentAlone() =>
        Verify.DeclinedCompilerCodeFixAsync<WrapInMonadCodeFix>(
            """
            internal Option<int> Passed() => Widen({|#0:Five|});

            private static Option<int> Widen(Option<int> option) => option;

            private static int Five() => 5;
            """,
            Mismatch("CS1503", 0));

    private static DiagnosticResult Mismatch(string id, int location) =>
        DiagnosticResult.CompilerError(id).WithLocation(location);

    private static Task Fixed(string source, string fixedSource) =>
        Verify.CompilerCodeFixAsync<WrapInMonadCodeFix>(
            source,
            fixedSource,
            new[] { Mismatch("CS0029", 0) },
            Array.Empty<DiagnosticResult>());

    private static Task Unfixed(string source) =>
        Verify.DeclinedCompilerCodeFixAsync<WrapInMonadCodeFix>(
            source,
            Mismatch("CS0029", 0));
}
