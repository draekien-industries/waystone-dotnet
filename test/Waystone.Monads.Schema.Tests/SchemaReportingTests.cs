namespace Waystone.Monads.Schemas;

using System;
using System.Threading.Tasks;
using Shouldly;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class SchemaReportingTests
{
    private static readonly ParseContext At = ParseContext.Root.At("email");

    private static readonly ErrorCode Domain = new("contact.unusable");

    [Fact]
    public void GivenAFailure_WhenReplacingTheMessage_ThenReportTheNewText()
    {
        Outcome<string> outcome =
            new Rejects<string>()
               .WithMessage("Expected {Path} to be usable, got {Received}.")
               .Evaluate("nope", At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Expected email to be usable, got nope.");
    }

    [Fact]
    public void
        GivenSeveralFailures_WhenReplacingTheMessage_ThenReplaceEveryOne()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .Check(
                                      static _ => false,
                                      ViolationCode.OutOfRange,
                                      "First.")
                                 .Check(
                                      static _ => false,
                                      ViolationCode.Mismatched,
                                      "Second.")
                                 .WithMessage("Unusable.")
                                 .Evaluate("nope", At);

        outcome.Violations.Count.ShouldBe(2);
        outcome.Violations.ShouldAllBe(v => v.Message == "Unusable.");
    }

    [Fact]
    public void
        GivenARefinementFailure_WhenReplacingTheMessage_ThenKeepTheValue()
    {
        Outcome<string> outcome = new RefinesAndKeeps<string>()
                                 .WithMessage("Unusable.")
                                 .Evaluate("nope", At);

        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe("nope");
        outcome.Violations.ShouldHaveSingleItem().Message.ShouldBe("Unusable.");
    }

    [Fact]
    public void GivenNoFailure_WhenReplacingTheMessage_ThenChangeNothing()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .WithMessage("Unusable.")
                                 .Evaluate("fine", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("fine");
    }

    [Fact]
    public void GivenAFailure_WhenReplacingTheCode_ThenReportTheNewCode()
    {
        Outcome<string> outcome = new Rejects<string>()
                                 .WithCode(ViolationCode.Conflicting)
                                 .Evaluate("nope", At);

        outcome.Violations.ShouldHaveSingleItem()
               .Code.ShouldBe(ViolationCodeCatalog.Codes.Conflicting);
    }

    [Fact]
    public void GivenADomainCode_WhenReplacingTheCode_ThenReportIt()
    {
        Outcome<string> outcome = new RefinesAndKeeps<string>()
                                 .WithCode(Domain)
                                 .Evaluate("nope", At);

        outcome.HasValue.ShouldBeTrue();
        outcome.Violations.ShouldHaveSingleItem().Code.ShouldBe(Domain);
    }

    [Fact]
    public void GivenNoFailure_WhenReplacingTheCode_ThenChangeNothing()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .WithCode(Domain)
                                 .Evaluate("fine", At);

        outcome.Violations.ShouldBeEmpty();
    }

    [Fact]
    public void
        GivenTheCodeIsReplacedFirst_WhenReplacingTheMessage_ThenRenderTheNewCode()
    {
        Outcome<string> outcome = new Rejects<string>()
                                 .WithCode(Domain)
                                 .WithMessage("Reported as {Code}.")
                                 .Evaluate("nope", At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Reported as contact.unusable.");
    }

    [Fact]
    public void GivenAName_WhenReporting_ThenReplaceTheInnermostSegment()
    {
        Outcome<string> outcome = new Rejects<string>("Rejected {Path}.")
                                 .Named("address")
                                 .Evaluate("nope", At);

        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Path.ToString().ShouldBe("address");
        violation.Message.ShouldBe("Rejected address.");
    }

    [Fact]
    public void GivenANestedPath_WhenNaming_ThenKeepTheSegmentsAbove()
    {
        Outcome<string> outcome =
            new Rejects<string>("Rejected {Path}.")
               .Named("address")
               .Evaluate("nope", ParseContext.Root.At("order").At("email"));

        outcome.Violations.ShouldHaveSingleItem()
               .Path.ToString()
               .ShouldBe("order.address");
    }

    [Fact]
    public void GivenTheRootPath_WhenNaming_ThenMakeTheNameTheFirstSegment()
    {
        Outcome<string> outcome = new Rejects<string>("Rejected {Path}.")
                                 .Named("address")
                                 .Evaluate("nope", ParseContext.Root);

        outcome.Violations.ShouldHaveSingleItem()
               .Path.ToString()
               .ShouldBe("address");
    }

    [Fact]
    public async Task GivenAnAsynchronousSchema_WhenNaming_ThenRenameItsPath()
    {
        Outcome<string> outcome = await new AsyncRejects<string>()
                                       .Named("address")
                                       .EvaluateAsync(
                                            "nope",
                                            At,
                                            TestContext.Current
                                               .CancellationToken);

        outcome.Violations.ShouldHaveSingleItem()
               .Path.ToString()
               .ShouldBe("address");
    }

    [Fact]
    public void GivenASensitiveSchema_WhenReplacingTheMessage_ThenRedactIt()
    {
        Outcome<string> outcome = new Rejects<string>()
                                 .WithMessage("Got {Received}.")
                                 .Sensitive()
                                 .Evaluate("hunter2", At);

        outcome.Violations.ShouldHaveSingleItem().Message.ShouldBe("Got ***.");
    }

    [Fact]
    public void GivenANullArgument_WhenReporting_ThenThrow()
    {
        var inner = new PassThrough<string>();

        Should.Throw<ArgumentNullException>(() => inner.WithMessage(null!))
              .ParamName.ShouldBe("template");

        Should.Throw<ArgumentNullException>(
                   () => inner.WithCode((ErrorCode)null!))
              .ParamName.ShouldBe("code");

        Should.Throw<ArgumentNullException>(() => inner.Named(null!))
              .ParamName.ShouldBe("name");
    }
}
