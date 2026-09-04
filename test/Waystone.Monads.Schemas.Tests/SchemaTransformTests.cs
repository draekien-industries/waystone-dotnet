namespace Waystone.Monads.Schemas;

using System;
using System.Threading.Tasks;
using Shouldly;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class SchemaTransformTests
{
    private static readonly ParseContext At = ParseContext.Root.At("total");

    private static readonly ErrorCode Refused = new("money.not_positive");

    [Fact]
    public void GivenATotalConversion_WhenTransforming_ThenNarrowTheType()
    {
        Outcome<int> outcome = new PassThrough<string>()
                              .Transform(static value => value.Length)
                              .Evaluate("abcd", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(4);
    }

    [Fact]
    public void GivenAnEarlierRefinement_WhenTransforming_ThenCarryItThrough()
    {
        Outcome<int> outcome = new RefinesAndKeeps<string>()
                              .Transform(static value => value.Length)
                              .Evaluate("abcd", At);

        outcome.Value.ShouldBe(4);
        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenNoValue_WhenTransforming_ThenDoNotConvert()
    {
        var ran = false;

        Outcome<int> outcome = new Rejects<string>()
                             .Transform(
                                  value =>
                                  {
                                      ran = true;

                                      return value.Length;
                                  })
                             .Evaluate("abcd", At);

        ran.ShouldBeFalse();
        outcome.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void GivenATotalConversionReturningNull_WhenTransforming_ThenReportIt()
    {
        Outcome<string> outcome = new PassThrough<string>()
                                 .Transform(static _ => (string)null!)
                                 .Evaluate("abcd", At);

        outcome.HasValue.ShouldBeFalse();

        Violation violation = outcome.Violations.ShouldHaveSingleItem();

        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Malformed);
        violation.Message.ShouldBe(
            "Expected total to convert to a value, but the conversion produced none.");
    }

    [Fact]
    public void GivenARefinedValueConvertingToNull_WhenTransforming_ThenKeepWhatWasReported()
    {
        Outcome<string> outcome = new RefinesAndKeeps<string>()
                                 .Transform(static _ => (string)null!)
                                 .Evaluate("abcd", At);

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.Count.ShouldBe(2);

        outcome.Violations[^1]
               .Code.ShouldBe(ViolationCodeCatalog.Codes.Malformed);
    }

    [Fact]
    public void GivenAnAcceptedConversion_WhenTransforming_ThenYieldItsValue()
    {
        Outcome<int> outcome =
            new PassThrough<string>()
               .Transform(
                    static value => Result.Ok<int, Error>(value.Length))
               .Evaluate("abcd", At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(4);
    }

    [Fact]
    public void GivenAnAcceptedConversionAfterARefinement_WhenTransforming_ThenKeepBoth()
    {
        Outcome<int> outcome =
            new RefinesAndKeeps<string>()
               .Transform(
                    static value => Result.Ok<int, Error>(value.Length))
               .Evaluate("abcd", At);

        outcome.Value.ShouldBe(4);
        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenARefusedConversion_WhenTransforming_ThenHaltWithItsCodeAndMessage()
    {
        Outcome<int> outcome =
            new PassThrough<string>()
               .Transform(
                    static _ => Result.Err<int, Error>(
                        new Error(
                            Refused,
                            "Expected {Path} to be positive, got {Received}.")))
               .Evaluate("abcd", At);

        outcome.HasValue.ShouldBeFalse();
        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Code.ShouldBe(Refused);

        violation.Message.ShouldBe(
            "Expected total to be positive, got abcd.");
    }

    [Fact]
    public void GivenARefusedConversionAfterARefinement_WhenTransforming_ThenReportBoth()
    {
        Outcome<int> outcome =
            new RefinesAndKeeps<string>()
               .Transform(
                    static _ => Result.Err<int, Error>(
                        new Error(Refused, "Refused.")))
               .Evaluate("abcd", At);

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.Count.ShouldBe(2);
    }

    [Fact]
    public void GivenNoValue_WhenTransformingFallibly_ThenDoNotConvert()
    {
        Outcome<int> outcome =
            new Rejects<string>()
               .Transform(
                    static value => Result.Ok<int, Error>(value.Length))
               .Evaluate("abcd", At);

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task GivenAnAsynchronousSchema_WhenTransforming_ThenConvertItsValue()
    {
        Outcome<int> outcome = await new AsyncPassThrough<string>()
                                    .Transform(
                                         static value => Result.Ok<int, Error>(
                                             value.Length))
                                    .EvaluateAsync(
                                         "abcd",
                                         At,
                                         TestContext.Current
                                            .CancellationToken);

        outcome.Value.ShouldBe(4);
    }

    [Fact]
    public void GivenANullConversion_WhenTransforming_ThenThrow()
    {
        var inner = new PassThrough<string>();

        Should.Throw<ArgumentNullException>(
                   () => inner.Transform((Func<string, int>)null!))
              .ParamName.ShouldBe("convert");

        Should.Throw<ArgumentNullException>(
                   () => inner.Transform(
                       (Func<string, Result<int, Error>>)null!))
              .ParamName.ShouldBe("convert");

        Should.Throw<ArgumentNullException>(
                   () => new MapSchema<string, string, int>(
                       null!,
                       static value => value.Length))
              .ParamName.ShouldBe("inner");

        Should.Throw<ArgumentNullException>(
                   () => new TransformSchema<string, string, int>(
                       null!,
                       static value => Result.Ok<int, Error>(value.Length)))
              .ParamName.ShouldBe("inner");
    }
}
