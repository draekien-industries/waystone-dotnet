namespace Waystone.Conventions.Tests;

using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;

public sealed class InternalRuleIdsTests
{
    private const string Document = """
        <doc>
          <assembly><name>Waystone.Monads</name></assembly>
          <members>
            <member name="M:Waystone.Monads.Options.Option`1.AndThenAsync``1(System.Func{`0,System.Threading.Tasks.ValueTask{Waystone.Monads.Options.Option{``0}}})">
              <remarks>{0}</remarks>
            </member>
          </members>
        </doc>
        """;

    [Theory]
    [InlineData("WA0001")]
    [InlineData("WA0002")]
    [InlineData("WSG0001")]
    [InlineData("WSG0004")]
    public void AnInternalRuleIdOnAReachableMemberIsReported(string ruleId)
    {
        IReadOnlyList<Citation> citations =
            InternalRuleIds.In(Document.Replace("{0}", $"<c>{ruleId}</c> enforces that."));

        Citation citation = citations.ShouldHaveSingleItem();
        citation.RuleId.ShouldBe(ruleId);
        citation.Member.ShouldStartWith("M:Waystone.Monads.Options.Option`1.AndThenAsync");
    }

    [Theory]
    [InlineData("WM1006")]
    [InlineData("WM2017")]
    [InlineData("WMS2001")]
    [InlineData("WMSC0001")]
    public void AShippedRuleIdIsLeftAlone(string ruleId)
    {
        InternalRuleIds.In(Document.Replace("{0}", $"<c>{ruleId}</c> reports it."))
                       .ShouldBeEmpty();
    }

    [Theory]
    [InlineData("WA001")]
    [InlineData("WA00012")]
    [InlineData("SWA0001")]
    [InlineData("WAIT")]
    public void ProseThatMerelyResemblesAnIdIsLeftAlone(string text)
    {
        InternalRuleIds.In(Document.Replace("{0}", text)).ShouldBeEmpty();
    }

    [Fact]
    public void AMemberUnderWaystoneInternalIsOutOfScope()
    {
        const string internalMember = """
            <doc>
              <members>
                <member name="T:Waystone.Internal.SourceGenerators.GenerateAwaitedReceiversAttribute">
                  <summary>Reports <c>WSG0001</c> when the type is not partial.</summary>
                </member>
              </members>
            </doc>
            """;

        InternalRuleIds.In(internalMember).ShouldBeEmpty();
    }

    [Fact]
    public void AMemberWhoseNameCarriesNoSymbolPrefixIsStillScanned()
    {
        const string malformed = """
            <doc>
              <members>
                <member name="Waystone.Internal.SourceGenerators.Whatever">
                  <summary>Reports <c>WSG0001</c>.</summary>
                </member>
              </members>
            </doc>
            """;

        InternalRuleIds.In(malformed).ShouldHaveSingleItem().RuleId.ShouldBe("WSG0001");
    }

    [Fact]
    public void EveryCitationOnAMemberIsReported()
    {
        IReadOnlyList<Citation> citations = InternalRuleIds.In(
            Document.Replace("{0}", "<c>WA0002</c> and <c>WA0003</c> both apply."));

        citations.Select(static citation => citation.RuleId)
                 .ShouldBe(["WA0002", "WA0003"]);
    }

    [Fact]
    public void ADocumentWithNoMembersReportsNothing()
    {
        InternalRuleIds.In("<doc><assembly><name>Empty</name></assembly></doc>")
                       .ShouldBeEmpty();
    }
}
