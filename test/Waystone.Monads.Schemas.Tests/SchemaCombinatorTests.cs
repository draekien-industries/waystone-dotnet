namespace Waystone.Monads.Schemas;

using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

public sealed class SchemaCombinatorTests
{
    private static readonly ParseContext At = ParseContext.Root.At("contact");

    [Fact]
    public void GivenEveryBranchPasses_WhenRequiringAll_ThenReportNothing()
    {
        Outcome<string> outcome =
            Schema.All(new PassThrough<string>(), new PassThrough<string>())
                  .Evaluate("alice", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("alice");
    }

    [Fact]
    public void GivenSeveralBranchesFail_WhenRequiringAll_ThenGatherThemAll()
    {
        Outcome<string> outcome = Schema.All(
                                            new RefinesAndKeeps<string>(),
                                            new RefinesAndKeeps<string>(),
                                            new PassThrough<string>())
                                       .Evaluate("alice", At);

        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe("alice");
        outcome.Violations.Count.ShouldBe(2);
    }

    [Fact]
    public void GivenTheFirstBranchProducesNoValue_WhenRequiringAll_ThenFail()
    {
        Outcome<string> outcome =
            Schema.All(new Rejects<string>(), new RefinesAndKeeps<string>())
                  .Evaluate("alice", At);

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.Count.ShouldBe(2);
    }

    [Fact]
    public void
        GivenALaterBranchProducesNoValue_WhenRequiringAll_ThenKeepTheFirstValue()
    {
        Outcome<string> outcome =
            Schema.All(new PassThrough<string>(), new Rejects<string>())
                  .Evaluate("alice", At);

        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe("alice");
        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenOneBranch_WhenRequiringAll_ThenApplyIt()
    {
        Outcome<string> outcome =
            Schema.All(new Rejects<string>()).Evaluate("alice", At);

        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task GivenBranchesFail_WhenRequiringAllAsynchronously_ThenGatherThem()
    {
        Outcome<string> outcome = await Schema
                                       .All(
                                            new AsyncPassThrough<string>(),
                                            new AsyncRejects<string>())
                                       .EvaluateAsync(
                                            "alice",
                                            At,
                                            TestContext.Current
                                               .CancellationToken);

        outcome.HasValue.ShouldBeTrue();
        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task
        GivenEveryBranchPasses_WhenRequiringAllAsynchronously_ThenReportNothing()
    {
        Outcome<string> outcome = await Schema
                                       .All(
                                            new AsyncPassThrough<string>(),
                                            new AsyncPassThrough<string>())
                                       .EvaluateAsync(
                                            "alice",
                                            At,
                                            TestContext.Current
                                               .CancellationToken);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("alice");
    }

    [Fact]
    public void GivenTheFirstBranchPasses_WhenAllowingAny_ThenSkipTheRest()
    {
        var second = new Counting<string>(new PassThrough<string>());

        Outcome<string> outcome =
            Schema.Any(new PassThrough<string>(), second)
                  .Evaluate("alice", At);

        second.Evaluations.ShouldBe(0);
        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("alice");
    }

    [Fact]
    public void GivenALaterBranchPasses_WhenAllowingAny_ThenReportNothing()
    {
        Outcome<string> outcome =
            Schema.Any(new Rejects<string>(), new PassThrough<string>())
                  .Evaluate("alice", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("alice");
    }

    [Fact]
    public void GivenABranchOnlyRefines_WhenAllowingAny_ThenTreatItAsAFailure()
    {
        Outcome<string> outcome =
            Schema.Any(new RefinesAndKeeps<string>(), new Rejects<string>())
                  .Evaluate("alice", At);

        outcome.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void GivenEveryBranchFails_WhenAllowingAny_ThenNestTheirFailures()
    {
        Outcome<string> outcome = Schema.Any(
                                            new Rejects<string>(),
                                            new Rejects<string>())
                                       .Evaluate("alice", At);

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.Count.ShouldBe(3);

        outcome.Violations[0]
               .Path.ToString()
               .ShouldBe("contact");

        outcome.Violations[0]
               .Code.ShouldBe(ViolationCodeCatalog.Codes.Mismatched);

        outcome.Violations[0]
               .Message.ShouldBe(
                    "Expected contact to satisfy one of the permitted alternatives.");

        outcome.Violations[1].Path.ToString().ShouldBe("contact[0]");
        outcome.Violations[2].Path.ToString().ShouldBe("contact[1]");
    }

    [Fact]
    public async Task
        GivenEveryBranchFails_WhenAllowingAnyAsynchronously_ThenNestTheirFailures()
    {
        Outcome<string> outcome = await Schema
                                       .Any(
                                            new AsyncRejects<string>(),
                                            new AsyncRejects<string>())
                                       .EvaluateAsync(
                                            "alice",
                                            At,
                                            TestContext.Current
                                               .CancellationToken);

        outcome.Violations.Count.ShouldBe(3);
        outcome.Violations[2].Path.ToString().ShouldBe("contact[1]");
    }

    [Fact]
    public async Task
        GivenTheFirstBranchPasses_WhenAllowingAnyAsynchronously_ThenSkipTheRest()
    {
        var second = new Counting<string>(new AsyncPassThrough<string>());

        Outcome<string> outcome = await Schema
                                       .Any(
                                            new AsyncPassThrough<string>(),
                                            second)
                                       .EvaluateAsync(
                                            "alice",
                                            At,
                                            TestContext.Current
                                               .CancellationToken);

        second.Evaluations.ShouldBe(0);
        outcome.Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenANamedBranch_WhenAllowingAny_ThenKeepTheBranchNumber()
    {
        Outcome<string> outcome = Schema.Any(
                                            new Rejects<string>(),
                                            new Rejects<string>().Named("byPhone"))
                                       .Evaluate("alice", At);

        outcome.Violations[2]
               .Path.ToString()
               .ShouldBe("contact[1].byPhone");
    }

    [Fact]
    public void GivenNoBranches_WhenCombining_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => Schema.All<string, string>(null!))
              .ParamName.ShouldBe("branches");

        Should.Throw<ArgumentException>(
                   () => Schema.All<string, string>())
              .ParamName.ShouldBe("branches");

        Should.Throw<ArgumentNullException>(
                   () => Schema.Any<string, string>(null!))
              .ParamName.ShouldBe("branches");

        Should.Throw<ArgumentException>(
                   () => Schema.Any<string, string>())
              .ParamName.ShouldBe("branches");
    }

    [Fact]
    public void GivenANullBranch_WhenCombining_ThenThrow()
    {
        Should.Throw<ArgumentException>(
                   () => Schema.All(new PassThrough<string>(), null!))
              .ParamName.ShouldBe("branches");
    }
}
