namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Shouldly;
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

    /// <summary>
    /// The prefix is <c>WMSC</c> rather than the <c>WMS</c> the design first asked
    /// for, because <c>WMS</c> already ships from
    /// <c>Waystone.Monads.Shouldly.Analyzers</c>. Sharing it would give one
    /// <c>.editorconfig</c> prefix two unrelated packages to suppress.
    /// </summary>
    [Theory]
    [MemberData(nameof(AllRules))]
    public void EveryRuleUsesTheSchemaGeneratorIdSpace(string id) =>
        id.ShouldStartWith("WMSC");

    [Theory]
    [MemberData(nameof(AllRules))]
    public void EveryRuleExplainsItself(string id) =>
        Rule(id).Description.ToString().ShouldNotBeNullOrWhiteSpace();

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
