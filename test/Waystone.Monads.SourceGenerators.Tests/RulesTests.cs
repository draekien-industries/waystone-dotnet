namespace Waystone.Monads.SourceGenerators;

using Microsoft.CodeAnalysis;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Waystone.Monads.SourceGenerators.ErrorCodes;
using Xunit;

public sealed class RulesTests
{
    public static TheoryData<string> AllRules()
    {
        TheoryData<string> data = new();

        foreach (DiagnosticDescriptor descriptor in Descriptors())
        {
            data.Add(descriptor.Id);
        }

        return data;
    }

    /// <summary>
    /// Reflects over the descriptors rather than listing ids, so a rule added
    /// without a help link fails here instead of shipping without one.
    /// </summary>
    /// <remarks>
    /// The link names the page and the anchor. An id-keyed indirection was
    /// considered and rejected: a GitBook redirect drops the fragment, so
    /// <c>wmg/WMG0005</c> would land a reader at the top of a page holding all six
    /// rather than on the one the build reported.
    /// </remarks>
    [Theory]
    [MemberData(nameof(AllRules))]
    public void EveryRuleCarriesAHelpLink(string id) =>
        Rule(id)
           .HelpLinkUri.ShouldBe(
                "https://draekien-industries.wpei.me/source-generation/diagnostics#"
              + id.ToLowerInvariant());

    [Theory]
    [MemberData(nameof(AllRules))]
    public void EveryRuleIsAnErrorAndOnByDefault(string id)
    {
        Rule(id).DefaultSeverity.ShouldBe(DiagnosticSeverity.Error);
        Rule(id).IsEnabledByDefault.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(AllRules))]
    public void EveryRuleUsesTheGeneratorIdSpace(string id) =>
        id.ShouldStartWith("WMG");

    [Fact]
    public void EveryRuleHasADistinctId() =>
        Descriptors()
           .Select(descriptor => descriptor.Id)
           .Distinct()
           .Count()
           .ShouldBe(Descriptors().Count);

    private static DiagnosticDescriptor Rule(string id) =>
        Descriptors().Single(descriptor => descriptor.Id == id);

    private static IReadOnlyList<DiagnosticDescriptor> Descriptors() =>
        typeof(Rules)
           .GetFields(BindingFlags.Public | BindingFlags.Static)
           .Where(field => field.FieldType == typeof(DiagnosticDescriptor))
           .Select(field => (DiagnosticDescriptor)field.GetValue(null)!)
           .ToList();
}
