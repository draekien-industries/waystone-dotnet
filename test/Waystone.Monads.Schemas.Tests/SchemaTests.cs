namespace Waystone.Monads.Schemas;

using System;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Waystone.Monads.Results;
using Xunit;

public sealed class SchemaTests
{
    [Fact]
    public void GivenAPassingSchema_WhenParsing_ThenReturnTheValue()
    {
        new Lengths().Parse("abcd").ShouldBe(Result.Ok<int, SchemaViolation>(4));
    }

    [Fact]
    public void GivenAFailingSchema_WhenParsing_ThenReturnTheViolations()
    {
        Result<string, SchemaViolation> result =
            new Rejects<string>().Parse("abc");

        result.IsErr.ShouldBeTrue();
        result.UnwrapErr().Violations[0].Message
              .ShouldBe("Rejected : got abc.");
    }

    [Fact]
    public async Task GivenAPassingSchema_WhenParsingAsynchronously_ThenReturnTheValue()
    {
        Result<string, SchemaViolation> result =
            await new AsyncPassThrough<string>().ParseAsync("abc", TestContext.Current.CancellationToken);

        result.ShouldBe(Result.Ok<string, SchemaViolation>("abc"));
    }

    [Fact]
    public async Task GivenASynchronousSchema_WhenParsingAsynchronously_ThenStillReturnTheValue()
    {
        Result<int, SchemaViolation> result =
            await new Lengths().ParseAsync(
                "abcd",
                TestContext.Current.CancellationToken);

        result.ShouldBe(Result.Ok<int, SchemaViolation>(4));
    }

    [Fact]
    public async Task GivenAFailingSchema_WhenParsingAsynchronously_ThenReturnTheViolations()
    {
        Result<string, SchemaViolation> result =
            await new Rejects<string>().ParseAsync("abc", TestContext.Current.CancellationToken);

        result.IsErr.ShouldBeTrue();
    }

    [Fact]
    public void GivenASensitiveSchema_WhenParsing_ThenRedactTheReceivedValue()
    {
        Result<string, SchemaViolation> result =
            new Rejects<string>().Sensitive().Parse("hunter2");

        result.UnwrapErr().Violations[0].Message
              .ShouldBe("Rejected : got ***.");
    }

    [Fact]
    public async Task GivenASensitiveSchema_WhenParsingAsynchronously_ThenRedactToo()
    {
        Result<string, SchemaViolation> result =
            await new Rejects<string>().Sensitive().ParseAsync("hunter2", TestContext.Current.CancellationToken);

        result.UnwrapErr().Violations[0].Message
              .ShouldBe("Rejected : got ***.");
    }

    [Fact]
    public void GivenASensitiveSchema_WhenMarkedTwice_ThenStillRedactOnce()
    {
        new Rejects<string>().Sensitive()
                             .Sensitive()
                             .Parse("hunter2")
                             .UnwrapErr()
                             .Violations[0]
                             .Message.ShouldBe("Rejected : got ***.");
    }

    [Fact]
    public void GivenASensitiveSchema_WhenItPasses_ThenNothingChanges()
    {
        new PassThrough<string>().Sensitive()
                                 .Parse("abc")
                                 .ShouldBe(
                                      Result.Ok<string, SchemaViolation>("abc"));
    }

    [Fact]
    public void GivenANullInnerSchema_WhenMarkingSensitive_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
            () => new SensitiveSchema<string, string>(null!));
    }

    [Fact]
    public void GivenAComposedSchema_WhenItPasses_ThenReturnWhatConfigureBuilt()
    {
        new ComposedOf(new Lengths()).Parse("abcd")
                                     .ShouldBe(
                                          Result.Ok<int, SchemaViolation>(4));
    }

    [Fact]
    public void GivenAComposedSchema_WhenItFails_ThenKeepItsOwnPaths()
    {
        Result<int, SchemaViolation> result =
            new ComposedOf(new RejectsText()).Parse("abcd");

        result.IsErr.ShouldBeTrue();
        result.UnwrapErr().Violations[0].Path.ToString().ShouldBe("subject");
    }

    [Fact]
    public void GivenANestedComposedSchema_WhenItFails_ThenRebaseItsPathsUnderTheField()
    {
        var nested = new ComposedOf(new RejectsText());

        Outcome<int> outcome = nested.Evaluate(
            "abcd",
            ParseContext.Root.At("order"));

        outcome.Violations[0].Path.ToString().ShouldBe("order.subject");
    }

    [Fact]
    public void GivenAComposedSchema_WhenEvaluatedAtTheRoot_ThenLeaveThePathsAlone()
    {
        var nested = new ComposedOf(new RejectsText());

        nested.Evaluate("abcd", ParseContext.Root)
              .Violations[0]
              .Path.ToString()
              .ShouldBe("subject");
    }

}
