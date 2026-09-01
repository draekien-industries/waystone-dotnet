namespace Waystone.Monads.Schemas;

using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

public sealed class SchemaNotTests
{
    private static readonly ParseContext At = ParseContext.Root.At("nickname");

    [Fact]
    public void GivenTheRejectedSchemaPasses_WhenNegating_ThenReportIt()
    {
        Outcome<string> outcome =
            new PassThrough<string>()
               .Not(
                    new PassThrough<string>(),
                    "Expected {Path} not to be reserved, got {Received}.")
               .Evaluate("root", At);

        outcome.HasValue.ShouldBeTrue();
        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.NotAllowed);

        violation.Message.ShouldBe(
            "Expected nickname not to be reserved, got root.");
    }

    [Fact]
    public void GivenTheRejectedSchemaFails_WhenNegating_ThenReportNothing()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .Not(new Rejects<string>(), "Not allowed.")
                                 .Evaluate("alice", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("alice");
    }

    [Fact]
    public void GivenTheRejectedSchemaOnlyRefines_WhenNegating_ThenReportNothing()
    {
        Outcome<string> outcome =
            new PassThrough<string>()
               .Not(new RefinesAndKeeps<string>(), "Not allowed.")
               .Evaluate("alice", At);

        outcome.Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenNoValue_WhenNegating_ThenDoNotRunTheRejectedSchema()
    {
        var rejected = new Counting<string>(new PassThrough<string>());

        Outcome<string> outcome = new Rejects<string>()
                                 .Not(rejected, "Not allowed.")
                                 .Evaluate("alice", At);

        rejected.Evaluations.ShouldBe(0);
        outcome.HasValue.ShouldBeFalse();
    }

    [Fact]
    public async Task
        GivenTheRejectedSchemaPasses_WhenNegatingAsynchronously_ThenReportIt()
    {
        Outcome<string> outcome =
            await new AsyncPassThrough<string>()
                 .Not(new AsyncPassThrough<string>(), "Not allowed: {Received}.")
                 .EvaluateAsync(
                      "root",
                      At,
                      TestContext.Current.CancellationToken);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Not allowed: root.");
    }

    [Fact]
    public async Task
        GivenTheRejectedSchemaFailsAsynchronously_WhenNegating_ThenReportNothing()
    {
        Outcome<string> outcome =
            await new AsyncPassThrough<string>()
                 .Not(new AsyncRejects<string>(), "Not allowed.")
                 .EvaluateAsync(
                      "alice",
                      At,
                      TestContext.Current.CancellationToken);

        outcome.Violations.ShouldBeEmpty();
    }

    [Fact]
    public async Task
        GivenNoValue_WhenNegatingAsynchronously_ThenDoNotRunTheRejectedSchema()
    {
        var rejected = new Counting<string>(new AsyncPassThrough<string>());

        Outcome<string> outcome = await new AsyncRejects<string>()
                                       .Not(rejected, "Not allowed.")
                                       .EvaluateAsync(
                                            "alice",
                                            At,
                                            TestContext.Current
                                               .CancellationToken);

        rejected.Evaluations.ShouldBe(0);
        outcome.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void GivenANullArgument_WhenNegating_ThenThrow()
    {
        var inner = new PassThrough<string>();

        Should.Throw<ArgumentNullException>(() => inner.Not(null!, "m"))
              .ParamName.ShouldBe("rejected");

        Should.Throw<ArgumentNullException>(
                   () => inner.Not(new PassThrough<string>(), null!))
              .ParamName.ShouldBe("message");

        Should.Throw<ArgumentNullException>(
                   () => new NotSchema<string, string>(
                       null!,
                       new PassThrough<string>(),
                       "m"))
              .ParamName.ShouldBe("inner");
    }
}
