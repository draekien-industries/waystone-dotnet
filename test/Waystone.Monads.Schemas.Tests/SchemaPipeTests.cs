namespace Waystone.Monads.Schemas;

using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

public sealed class SchemaPipeTests
{
    private static readonly ParseContext At = ParseContext.Root.At("orderId");

    [Fact]
    public void GivenBothSchemasPass_WhenPiping_ThenProduceTheSecondSchemasValue()
    {
        Outcome<int> outcome = new PassThrough<string>()
                              .Pipe(new Lengths())
                              .Evaluate("abcd", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(4);
    }

    [Fact]
    public void GivenTheSecondSchemaRefines_WhenPiping_ThenKeepItsValueAndViolation()
    {
        Outcome<int> outcome = new Lengths()
                              .Pipe(new RefinesAndKeeps<int>())
                              .Evaluate("abcd", At);

        outcome.Value.ShouldBe(4);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Refined orderId, kept 4.");
    }

    [Fact]
    public void GivenTheSecondSchemaFails_WhenPiping_ThenProduceNoValue()
    {
        Outcome<int> outcome = new PassThrough<string>()
                              .Pipe(new RejectsText())
                              .Evaluate("abcd", At);

        outcome.HasValue.ShouldBeFalse();

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Rejected orderId: got abcd.");
    }

    [Fact]
    public void GivenTheFirstSchemaRefines_WhenPiping_ThenStillRunTheSecond()
    {
        Outcome<int> outcome = new RefinesAndKeeps<string>()
                              .Pipe(new Lengths())
                              .Evaluate("abcd", At);

        outcome.Value.ShouldBe(4);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Refined orderId, kept abcd.");
    }

    [Fact]
    public void GivenBothSchemasReport_WhenPiping_ThenReportTheFirstSchemasViolationsFirst()
    {
        Outcome<int> outcome = new RefinesAndKeeps<string>()
                              .Pipe(new RejectsText())
                              .Evaluate("abcd", At);

        outcome.HasValue.ShouldBeFalse();

        outcome.Violations.Select(static violation => violation.Message)
               .ShouldBe(
                    new[]
                    {
                        "Refined orderId, kept abcd.",
                        "Rejected orderId: got abcd.",
                    });
    }

    [Fact]
    public void GivenTheFirstSchemaFails_WhenPiping_ThenDoNotRunTheSecond()
    {
        var next = new Counting<string>(new PassThrough<string>());

        Outcome<string> outcome = new Rejects<string>()
                                 .Pipe(next)
                                 .Evaluate("abcd", At);

        next.Evaluations.ShouldBe(0);
        outcome.HasValue.ShouldBeFalse();

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Rejected orderId: got abcd.");
    }

    [Fact]
    public async Task GivenAnAsynchronousSecondSchema_WhenPipingAsynchronously_ThenAwaitIt()
    {
        Outcome<string> outcome =
            await new AsyncPassThrough<string>()
                 .Pipe(new AsyncRejects<string>())
                 .EvaluateAsync(
                      "abcd",
                      At,
                      TestContext.Current.CancellationToken);

        outcome.HasValue.ShouldBeFalse();

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Rejected orderId asynchronously.");
    }

    [Fact]
    public async Task GivenTheFirstSchemaRefines_WhenPipingAsynchronously_ThenReportBoth()
    {
        Outcome<string> outcome =
            await new RefinesAndKeeps<string>()
                 .Pipe(new AsyncRejects<string>())
                 .EvaluateAsync(
                      "abcd",
                      At,
                      TestContext.Current.CancellationToken);

        outcome.Violations.Select(static violation => violation.Message)
               .ShouldBe(
                    new[]
                    {
                        "Refined orderId, kept abcd.",
                        "Rejected orderId asynchronously.",
                    });
    }

    [Fact]
    public async Task GivenTheFirstSchemaFailsAsynchronously_WhenPiping_ThenDoNotRunTheSecond()
    {
        var next = new Counting<string>(new AsyncPassThrough<string>());

        Outcome<string> outcome =
            await new AsyncRejects<string>()
                 .Pipe(next)
                 .EvaluateAsync(
                      "abcd",
                      At,
                      TestContext.Current.CancellationToken);

        next.Evaluations.ShouldBe(0);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Rejected orderId asynchronously.");
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", true)]
    [InlineData("0f8fad5b-d9cb-469f-a165-70867728950e", false)]
    public void GivenADeclaredUuidSchema_WhenPipingParsedText_ThenApplyItsRules(
        string text,
        bool rejected)
    {
        Schema<string, Guid> schema = Schema.Text
                                            .Transform(static value => Guid.Parse(value))
                                            .Pipe(Schema.Uuid.NotEmpty());

        schema.Parse(text).IsErr.ShouldBe(rejected);
    }

    [Fact]
    public void GivenANullArgument_WhenPiping_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => new PassThrough<string>().Pipe<int>(null!))
              .ParamName.ShouldBe("next");

        Should.Throw<ArgumentNullException>(
                   () => new PipeSchema<string, string, int>(
                       null!,
                       new Lengths()))
              .ParamName.ShouldBe("inner");
    }
}
