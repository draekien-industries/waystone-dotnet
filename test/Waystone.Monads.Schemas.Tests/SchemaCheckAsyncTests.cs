namespace Waystone.Monads.Schemas;

using System;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class SchemaCheckAsyncTests
{
    private static readonly ParseContext At = ParseContext.Root.At("email");

    [Fact]
    public async Task GivenAPassingRule_WhenCheckingAsync_ThenReportNothing()
    {
        Outcome<string> outcome = await Free()
                                       .EvaluateAsync(
                                            "abc",
                                            At,
                                            CancellationToken.None);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("abc");
    }

    /// <summary>
    /// A refinement, like <c>Check</c>: the value survives so every later rule on
    /// the chain still runs and one parse reports every problem at once.
    /// </summary>
    [Fact]
    public async Task
        GivenAFailingRule_WhenCheckingAsync_ThenKeepTheValueAndReportIt()
    {
        Outcome<string> outcome = await Free()
                                       .EvaluateAsync(
                                            "taken",
                                            At,
                                            CancellationToken.None);

        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe("taken");

        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Message.ShouldBe("email is already taken, got taken.");
        violation.Code.ShouldBe(
            ViolationCodeCatalog.ToErrorCode(ViolationCode.Mismatched));
    }

    [Fact]
    public async Task GivenAFailedField_WhenCheckingAsync_ThenNeverRunTheRule()
    {
        Outcome<string> outcome = await new Rejects<string>()
                                       .CheckAsync(
                                            static (_, _) =>
                                                throw new InvalidOperationException(
                                                    "The rule ran over a value the schema never produced."),
                                            ViolationCode.Mismatched,
                                            "Unreachable.")
                                       .EvaluateAsync(
                                            "abc",
                                            At,
                                            CancellationToken.None);

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task GivenADomainCode_WhenCheckingAsync_ThenReportThatCode()
    {
        var code = new ErrorCode("user.handle_reserved");

        Outcome<string> outcome = await new PassThrough<string>()
                                       .CheckAsync(
                                            static (_, _) =>
                                                new ValueTask<bool>(false),
                                            code,
                                            "Reserved.")
                                       .EvaluateAsync(
                                            "root",
                                            At,
                                            CancellationToken.None);

        outcome.Violations.ShouldHaveSingleItem().Code.ShouldBe(code);
    }

    [Fact]
    public async Task GivenACancelledParse_WhenCheckingAsync_ThenPassTheToken()
    {
        using var source = new CancellationTokenSource();

        source.Cancel();

        Schema<string, string> schema = new PassThrough<string>().CheckAsync(
            static (_, token) =>
                new ValueTask<bool>(!token.IsCancellationRequested),
            ViolationCode.Mismatched,
            "Cancelled.");

        Outcome<string> outcome =
            await schema.EvaluateAsync("abc", At, source.Token);

        outcome.Violations.ShouldHaveSingleItem();
    }

    /// <summary>
    /// Blocking here would deadlock a caller on a synchronisation context and hide
    /// the mistake everywhere it did not. The throw is what <c>WMSC0006</c> reports
    /// ahead of, for the schemas a generator can see.
    /// </summary>
    [Fact]
    public void GivenAnAsyncRule_WhenParsing_ThenThrow() =>
        Should.Throw<InvalidOperationException>(() => Free().Parse("abc"))
              .Message.ShouldBe(
                   AsyncCheckSchema<string, string>.SynchronousParseMessage);

    [Fact]
    public async Task GivenAnAsyncRule_WhenParsingAsync_ThenParse()
    {
        Result<string, SchemaViolation> result = await Free()
           .ParseAsync("abc", TestContext.Current.CancellationToken);

        result.Unwrap().ShouldBe("abc");
    }

    /// <summary>
    /// A rule the parse never reaches never throws, which is why the exception is
    /// documented as depending on the input rather than on the schema alone.
    /// </summary>
    [Fact]
    public void GivenAnUnreachedAsyncRule_WhenParsing_ThenDoNotThrow() =>
        Schema.Optional(Option.None<string>(), Free())
              .Evaluate(ParseContext.Root)
              .ShouldBeEmpty();

    [Fact]
    public void GivenNoPredicate_WhenCheckingAsync_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => new PassThrough<string>().CheckAsync(
                null!,
                ViolationCode.Mismatched,
                "Unreachable."));

    [Fact]
    public void GivenNoCode_WhenCheckingAsync_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => new PassThrough<string>().CheckAsync(
                static (_, _) => new ValueTask<bool>(true),
                null!,
                "Unreachable."));

    [Fact]
    public void GivenNoMessage_WhenCheckingAsync_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => new PassThrough<string>().CheckAsync(
                static (_, _) => new ValueTask<bool>(true),
                ViolationCode.Mismatched,
                null!));

    private static Schema<string, string> Free() =>
        new PassThrough<string>().CheckAsync(
            static (value, _) => new ValueTask<bool>(value != "taken"),
            ViolationCode.Mismatched,
            "{Path} is already taken, got {Received}.");
}
