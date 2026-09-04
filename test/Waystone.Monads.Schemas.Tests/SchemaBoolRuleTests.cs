namespace Waystone.Monads.Schemas;

using Shouldly;
using Xunit;

public sealed class SchemaBoolRuleTests
{
    private static readonly ParseContext At =
        ParseContext.Root.At("acceptedTerms");

    [Fact]
    public void GivenASetFlag_WhenRequiringItSet_ThenReportNothing() =>
        Schema.Bool.IsTrue().Evaluate(true, At).Violations.ShouldBeEmpty();

    [Fact]
    public void GivenAClearFlag_WhenRequiringItSet_ThenReportNotAllowed()
    {
        Violation violation = Schema.Bool.IsTrue()
                                    .Evaluate(false, At)
                                    .Violations.ShouldHaveSingleItem();

        violation.Message.ShouldBe("Expected acceptedTerms to be accepted.");
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.NotAllowed);
    }

    [Fact]
    public void GivenAClearFlag_WhenRequiringItClear_ThenReportNothing() =>
        Schema.Bool.IsFalse().Evaluate(false, At).Violations.ShouldBeEmpty();

    [Fact]
    public void GivenASetFlag_WhenRequiringItClear_ThenReportNotAllowed()
    {
        Violation violation = Schema.Bool.IsFalse()
                                   .Evaluate(true, At)
                                   .Violations.ShouldHaveSingleItem();

        violation.Message.ShouldBe("Expected acceptedTerms not to be set.");
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.NotAllowed);
    }

    /// <summary>
    /// A refinement like every other rule, so the flag still reaches the
    /// constructed object and a later rule still runs.
    /// </summary>
    [Fact]
    public void GivenAFailingFlag_WhenRequiringItSet_ThenKeepTheValue() =>
        Schema.Bool.IsTrue().Evaluate(false, At).Value.ShouldBeFalse();
}
