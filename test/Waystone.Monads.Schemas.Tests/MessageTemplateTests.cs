namespace Waystone.Monads.Schemas;

using Shouldly;
using Xunit;

public sealed class MessageTemplateTests
{
    private static string Render(
        string template,
        object? received = null,
        object? expected = null,
        bool isSensitive = false,
        string? predicate = null) =>
        MessageTemplate.Render(
            template,
            ViolationPath.Root.Append("email"),
            ViolationCodeCatalog.Codes.Malformed,
            received,
            expected,
            predicate,
            isSensitive);

    [Fact]
    public void GivenAPredicate_WhenRendering_ThenSubstituteItsSourceText()
    {
        Render(
                "Expected email to satisfy {Predicate}.",
                predicate: "value => value.Length > 2")
           .ShouldBe("Expected email to satisfy value => value.Length > 2.");
    }

    [Fact]
    public void GivenNoPredicate_WhenRendering_ThenLeaveTheTokenInPlace()
    {
        Render("Must satisfy {Predicate}.")
           .ShouldBe("Must satisfy {Predicate}.");
    }

    [Fact]
    public void GivenASensitiveSchema_WhenRendering_ThenStillShowThePredicate()
    {
        Render(
                "{Received} failed {Predicate}.",
                "hunter2",
                isSensitive: true,
                predicate: "value => value.Length > 2")
           .ShouldBe("*** failed value => value.Length > 2.");
    }

    [Fact]
    public void GivenNoTokens_WhenRendering_ThenReturnTheTemplateUnchanged()
    {
        Render("Nothing to substitute.").ShouldBe("Nothing to substitute.");
    }

    [Fact]
    public void GivenEveryToken_WhenRendering_ThenSubstituteEachOne()
    {
        Render("{Path}/{Received}/{Expected}/{Code}", "abc", 5)
           .ShouldBe("email/abc/5/schema_violation.malformed");
    }

    [Fact]
    public void GivenNoExpectedValue_WhenRendering_ThenLeaveTheTokenInPlace()
    {
        Render("At least {Expected}.").ShouldBe("At least {Expected}.");
    }

    [Fact]
    public void GivenASensitiveSchema_WhenRendering_ThenStillShowTheExpectedValue()
    {
        Render("At least {Expected}.", "hunter2", 5, true)
           .ShouldBe("At least 5.");
    }

    [Fact]
    public void GivenAnUnknownToken_WhenRendering_ThenLeaveItInPlace()
    {
        Render("Keep {Whatever} here.").ShouldBe("Keep {Whatever} here.");
    }

    [Fact]
    public void GivenAnUnclosedBrace_WhenRendering_ThenKeepTheRestLiterally()
    {
        Render("Ends with {Path").ShouldBe("Ends with {Path");
    }

    [Fact]
    public void GivenNoReceivedValue_WhenRendering_ThenSayItWasAbsent()
    {
        Render("Got {Received}.").ShouldBe("Got null.");
    }

    [Fact]
    public void GivenASensitiveSchema_WhenRendering_ThenRedactTheReceivedValue()
    {
        Render("Got {Received}.", "hunter2", isSensitive: true)
           .ShouldBe("Got ***.");
    }

    [Fact]
    public void GivenAReceivedValueHoldingAToken_WhenRendering_ThenDoNotSubstituteItAgain()
    {
        Render("Got {Received}.", "{Code}").ShouldBe("Got {Code}.");
    }

    [Fact]
    public void GivenAnEmptyTemplate_WhenRendering_ThenReturnEmpty()
    {
        Render(string.Empty).ShouldBeEmpty();
    }

    [Fact]
    public void GivenTheRootPath_WhenRendering_ThenSubstituteTheEmptyString()
    {
        MessageTemplate.Render(
                "at [{Path}]",
                ViolationPath.Root,
                ViolationCodeCatalog.Codes.Duplicate,
                null,
                null,
                null,
                false)
           .ShouldBe("at []");
    }
}
