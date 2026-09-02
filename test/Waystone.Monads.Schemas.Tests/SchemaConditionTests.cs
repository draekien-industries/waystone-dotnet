namespace Waystone.Monads.Schemas;

using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

public sealed class SchemaConditionTests
{
    private static readonly ParseContext At = ParseContext.Root.At("nickname");

    [Fact]
    public void GivenTheConditionHolds_WhenApplyingWhen_ThenRunTheRules()
    {
        Outcome<string> outcome = new Rejects<string>()
                                 .When(static value => value.Length > 2)
                                 .Evaluate("alice", At);

        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void
        GivenTheConditionDoesNotHold_WhenApplyingWhen_ThenPassTheInputThrough()
    {
        Outcome<string> outcome = new Rejects<string>()
                                 .When(static value => value.Length > 2)
                                 .Evaluate("al", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("al");
    }

    [Fact]
    public void
        GivenTheConditionHolds_WhenApplyingUnless_ThenPassTheInputThrough()
    {
        Outcome<string> outcome = new Rejects<string>()
                                 .Unless(static value => value.Length > 2)
                                 .Evaluate("alice", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("alice");
    }

    [Fact]
    public void GivenTheConditionDoesNotHold_WhenApplyingUnless_ThenRunTheRules()
    {
        Outcome<string> outcome = new Rejects<string>()
                                 .Unless(static value => value.Length > 2)
                                 .Evaluate("al", At);

        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task
        GivenTheConditionHolds_WhenApplyingWhenAsynchronously_ThenRunTheRules()
    {
        Outcome<string> outcome = await new AsyncRejects<string>()
                                       .When(static _ => true)
                                       .EvaluateAsync(
                                            "alice",
                                            At,
                                            TestContext.Current
                                               .CancellationToken);

        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task
        GivenTheConditionDoesNotHold_WhenApplyingWhenAsynchronously_ThenSkipThem()
    {
        Outcome<string> outcome = await new AsyncRejects<string>()
                                       .When(static _ => false)
                                       .EvaluateAsync(
                                            "alice",
                                            At,
                                            TestContext.Current
                                               .CancellationToken);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("alice");
    }

    [Fact]
    public void GivenANullArgument_WhenApplyingACondition_ThenThrow()
    {
        var inner = new PassThrough<string>();

        Should.Throw<ArgumentNullException>(() => inner.When(null!))
              .ParamName.ShouldBe("predicate");

        Should.Throw<ArgumentNullException>(() => inner.Unless(null!))
              .ParamName.ShouldBe("predicate");

        Should.Throw<ArgumentNullException>(
                   () => SchemaExtensions.When<string>(null!, static _ => true))
              .ParamName.ShouldBe("inner");
    }
}
