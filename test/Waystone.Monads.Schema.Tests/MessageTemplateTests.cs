namespace Waystone.Monads.Schemas;

using Shouldly;
using Xunit;

public sealed class MessageTemplateTests
{
    private static string Render(
        string template,
        object? received = null,
        bool isSensitive = false) =>
        MessageTemplate.Render(
            template,
            ViolationPath.Root.Append("email"),
            ViolationCodeCatalog.Codes.Malformed,
            received,
            isSensitive);

    [Fact]
    public void GivenNoTokens_WhenRendering_ThenReturnTheTemplateUnchanged()
    {
        Render("Nothing to substitute.").ShouldBe("Nothing to substitute.");
    }

    [Fact]
    public void GivenEveryToken_WhenRendering_ThenSubstituteEachOne()
    {
        Render("{Path}/{Received}/{Code}", "abc")
           .ShouldBe("email/abc/schema_violation.malformed");
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
                false)
           .ShouldBe("at []");
    }
}
