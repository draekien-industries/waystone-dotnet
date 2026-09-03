namespace Waystone.Monads.Schemas;

using System;
using System.Threading.Tasks;
using Shouldly;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class SchemaCheckTests
{
    private static readonly ParseContext At =
        ParseContext.Root.At("password");

    [Fact]
    public void GivenAPassingRule_WhenChecking_ThenReportNothing()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .Check(
                                      static value => value.Length > 2,
                                      ViolationCode.OutOfRange,
                                      "Too short.")
                                 .Evaluate("abc", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe("abc");
    }

    [Fact]
    public void GivenAFailingRule_WhenChecking_ThenKeepTheValueAndReportIt()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .Check(
                                      static value => value.Length > 2,
                                      ViolationCode.OutOfRange,
                                      "Expected {Path} to be longer, got {Received}.")
                                 .Evaluate("a", At);

        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe("a");
        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Path.ToString().ShouldBe("password");
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.OutOfRange);

        violation.Message.ShouldBe(
            "Expected password to be longer, got a.");
    }

    [Fact]
    public void GivenTwoFailingRules_WhenChecking_ThenReportBoth()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .Check(
                                      static value => value.Length > 2,
                                      ViolationCode.OutOfRange,
                                      "Too short.")
                                 .Check(
                                      static value => value != "a",
                                      ViolationCode.Mismatched,
                                      "Not that one.")
                                 .Evaluate("a", At);

        outcome.Violations.Count.ShouldBe(2);
        outcome.Value.ShouldBe("a");
    }

    [Fact]
    public void GivenAnEarlierFailureWithNoValue_WhenChecking_ThenDoNotRunTheRule()
    {
        var ran = false;

        Outcome<string> outcome = new Rejects<string>()
                                 .Check(
                                      value =>
                                      {
                                          ran = true;

                                          return true;
                                      },
                                      ViolationCode.OutOfRange,
                                      "Never reported.")
                                 .Evaluate("a", At);

        ran.ShouldBeFalse();
        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenADomainCode_WhenChecking_ThenReportThatCode()
    {
        var code = new ErrorCode("order.line_count_exceeded");

        Outcome<string> outcome = new PassThrough<string>()
                                 .Check(
                                      static _ => false,
                                      code,
                                      "Reported as {Code}.")
                                 .Evaluate("a", At);

        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Code.ShouldBe(code);

        violation.Message.ShouldBe(
            "Reported as order.line_count_exceeded.");
    }

    [Fact]
    public async Task GivenAFailingRule_WhenCheckingAsynchronously_ThenReportIt()
    {
        Outcome<string> outcome = await new AsyncPassThrough<string>()
                                       .Check(
                                            static _ => false,
                                            ViolationCode.OutOfRange,
                                            "Rejected {Path}.")
                                       .EvaluateAsync(
                                            "a",
                                            At,
                                            TestContext.Current
                                               .CancellationToken);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Rejected password.");
    }

    [Fact]
    public void GivenANullArgument_WhenConstructing_ThenThrow()
    {
        var inner = new PassThrough<string>();

        Should.Throw<ArgumentNullException>(
                   () => new CheckSchema<string, string>(
                       null!,
                       static _ => true,
                       ViolationCodeCatalog.Codes.Malformed,
                       "m"))
              .ParamName.ShouldBe("inner");

        Should.Throw<ArgumentNullException>(
                   () => inner.Check(
                       null!,
                       ViolationCodeCatalog.Codes.Malformed,
                       "m"))
              .ParamName.ShouldBe("predicate");

        Should.Throw<ArgumentNullException>(
                   () => inner.Check(static _ => true, null!, "m"))
              .ParamName.ShouldBe("code");

        Should.Throw<ArgumentNullException>(
                   () => inner.Check(
                       static _ => true,
                       ViolationCodeCatalog.Codes.Malformed,
                       null!))
              .ParamName.ShouldBe("message");
    }

    [Fact]
    public void GivenAPredicateToken_WhenChecking_ThenRenderThePredicateSource()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .Check(
                                      static value => value.Length > 2,
                                      ViolationCode.OutOfRange,
                                      "Expected {Path} to satisfy {Predicate}.")
                                 .Evaluate("a", At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe(
                    "Expected password to satisfy static value => value.Length > 2.");
    }

    [Fact]
    public void GivenAnErrorCodeOverload_WhenChecking_ThenRenderThePredicateSource()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .Check(
                                      static value => value.Length > 2,
                                      ViolationCodeCatalog.Codes.OutOfRange,
                                      "Expected {Path} to satisfy {Predicate}.")
                                 .Evaluate("a", At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe(
                    "Expected password to satisfy static value => value.Length > 2.");
    }

    [Fact]
    public void GivenAnOverridingExpression_WhenChecking_ThenRenderThatInstead()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .Check(
                                      static value => value.Length > 2,
                                      ViolationCode.OutOfRange,
                                      "Expected {Path} to satisfy {Predicate}.",
                                      "more than two characters")
                                 .Evaluate("a", At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe(
                    "Expected password to satisfy more than two characters.");
    }

    [Fact]
    public void GivenWithMessage_WhenChecking_ThenLeaveThePredicateTokenInPlace()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .Check(
                                      static value => value.Length > 2,
                                      ViolationCode.OutOfRange,
                                      "Replaced.")
                                 .WithMessage("Failed {Predicate}.")
                                 .Evaluate("a", At);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Failed {Predicate}.");
    }
}
