namespace Waystone.Monads.Shouldly.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class UseMonadAssertionCodeFixTests
{
    [Theory]
    [InlineData("option.IsSome.ShouldBeTrue()", "option.ShouldBeSome()", "IsSome", "Option<int>", "ShouldBeSome")]
    [InlineData("option.IsNone.ShouldBeFalse()", "option.ShouldBeSome()", "IsNone", "Option<int>", "ShouldBeSome")]
    [InlineData("option.IsNone.ShouldBeTrue()", "option.ShouldBeNone()", "IsNone", "Option<int>", "ShouldBeNone")]
    [InlineData("option.IsSome.ShouldBeFalse()", "option.ShouldBeNone()", "IsSome", "Option<int>", "ShouldBeNone")]
    [InlineData("result.IsOk.ShouldBeTrue()", "result.ShouldBeOk()", "IsOk", "Result<int, string>", "ShouldBeOk")]
    [InlineData("result.IsErr.ShouldBeFalse()", "result.ShouldBeOk()", "IsErr", "Result<int, string>", "ShouldBeOk")]
    [InlineData("result.IsErr.ShouldBeTrue()", "result.ShouldBeErr()", "IsErr", "Result<int, string>", "ShouldBeErr")]
    [InlineData("result.IsOk.ShouldBeFalse()", "result.ShouldBeErr()", "IsOk", "Result<int, string>", "ShouldBeErr")]
    public Task GivenAStateAssertedThroughABool_ThenRewriteIt(
        string before,
        string after,
        string member,
        string monad,
        string replacement) =>
        Fix(before, after, member, monad, replacement);

    [Theory]
    [InlineData("option.Unwrap().ShouldBe(3)", "option.ShouldBeSomeValue(3)", "Unwrap", "Option<int>", "ShouldBeSomeValue")]
    [InlineData("result.Unwrap().ShouldBe(3)", "result.ShouldBeOkValue(3)", "Unwrap", "Result<int, string>", "ShouldBeOkValue")]
    [InlineData("result.UnwrapErr().ShouldBe(\"failed\")", "result.ShouldBeErrValue(\"failed\")", "UnwrapErr", "Result<int, string>", "ShouldBeErrValue")]
    public Task GivenAValueAssertedThroughAnUnwrap_ThenRewriteIt(
        string before,
        string after,
        string member,
        string monad,
        string replacement) =>
        Fix(before, after, member, monad, replacement);

    /// <summary>
    /// The argument list transfers unchanged, so a custom message survives the rewrite.
    /// </summary>
    /// <remarks>
    /// The assertions take the expected value first and the custom message second, in
    /// the order Shouldly's own <c>ShouldBe</c> does. That is what lets the fix move the
    /// list across rather than rebuild it, and it is the reason the analyzer refuses any
    /// overload whose second parameter is something else.
    /// </remarks>
    [Theory]
    [InlineData(
        "option.IsSome.ShouldBeTrue(\"while loading\")",
        "option.ShouldBeSome(\"while loading\")",
        "IsSome",
        "ShouldBeSome")]
    [InlineData(
        "option.Unwrap().ShouldBe(3, \"while loading\")",
        "option.ShouldBeSomeValue(3, \"while loading\")",
        "Unwrap",
        "ShouldBeSomeValue")]
    public Task GivenACustomMessage_ThenCarryItOver(
        string before,
        string after,
        string member,
        string replacement) =>
        Fix(before, after, member, "Option<int>", replacement);

    /// <summary>
    /// The rewrite leaves a parenthesised await behind for WMS2002 to take.
    /// </summary>
    /// <remarks>
    /// The two rules compose rather than overlap: this one replaces the assertion and
    /// keeps the receiver exactly as written, which turns a raw assertion on an awaited
    /// task into the shape WMS2002 recognises. Neither rule needs to know about the
    /// other, and a suite carrying both shapes at once converges after two passes.
    /// </remarks>
    [Fact]
    public Task GivenAnAwaitedReceiver_ThenKeepItAndRewriteTheAssertion() =>
        Verify.CodeFixAsync<RawAssertionAnalyzer, UseMonadAssertionCodeFix>(
            """
            private async Task Check(Task<Option<int>> task)
            {
                {|#0:(await task).IsSome.ShouldBeTrue()|};
            }
            """,
            """
            private async Task Check(Task<Option<int>> task)
            {
                (await task).ShouldBeSome();
            }
            """,
            Verify.Diagnostic(Rules.RawAssertion)
               .WithLocation(0)
               .WithArguments("IsSome", "Option<int>", "ShouldBeSome"));

    /// <summary>
    /// Every shape in one file is rewritten in a single batch.
    /// </summary>
    /// <remarks>
    /// Batch fixing is how a migration across a suite actually runs, and it is a
    /// different code path from fixing one diagnostic at a time: the fixes are computed
    /// against the original tree and applied together, so a fix that depended on an
    /// earlier one having already run would pass the single-site tests and corrupt the
    /// sweep.
    /// </remarks>
    [Fact]
    public Task GivenSeveralShapesInOneFile_ThenFixThemTogether() =>
        Verify.FixAllAsync<RawAssertionAnalyzer, UseMonadAssertionCodeFix>(
            """
            private void Check(Option<int> option, Result<int, string> result)
            {
                {|#0:option.IsSome.ShouldBeTrue()|};
                {|#1:option.Unwrap().ShouldBe(3)|};
                {|#2:result.IsErr.ShouldBeTrue()|};
                {|#3:result.UnwrapErr().ShouldBe("failed")|};
                option.ShouldBeOfType<Some<int>>();
            }
            """,
            """
            private void Check(Option<int> option, Result<int, string> result)
            {
                option.ShouldBeSome();
                option.ShouldBeSomeValue(3);
                result.ShouldBeErr();
                result.ShouldBeErrValue("failed");
                option.ShouldBeOfType<Some<int>>();
            }
            """,
            Verify.Diagnostic(Rules.RawAssertion)
               .WithLocation(0)
               .WithArguments("IsSome", "Option<int>", "ShouldBeSome"),
            Verify.Diagnostic(Rules.RawAssertion)
               .WithLocation(1)
               .WithArguments("Unwrap", "Option<int>", "ShouldBeSomeValue"),
            Verify.Diagnostic(Rules.RawAssertion)
               .WithLocation(2)
               .WithArguments("IsErr", "Result<int, string>", "ShouldBeErr"),
            Verify.Diagnostic(Rules.RawAssertion)
               .WithLocation(3)
               .WithArguments(
                    "UnwrapErr",
                    "Result<int, string>",
                    "ShouldBeErrValue"));

    private static Task Fix(
        string before,
        string after,
        string member,
        string monad,
        string replacement) =>
        Verify.CodeFixAsync<RawAssertionAnalyzer, UseMonadAssertionCodeFix>(
            Subject("{|#0:" + before + "|}"),
            Subject(after),
            Verify.Diagnostic(Rules.RawAssertion)
               .WithLocation(0)
               .WithArguments(member, monad, replacement));

    private static string Subject(string expression) =>
        $$"""
          private void Check(Option<int> option, Result<int, string> result)
          {
              {{expression}};
          }
          """;
}
