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
    public void EveryRuleIsOnByDefault(string id) =>
        Rule(id).IsEnabledByDefault.ShouldBeTrue();

    /// <summary>
    /// A schema that cannot be generated fails the build, because the alternative is
    /// a missing member reported against a file its author cannot open. A schema
    /// that generates and runs does not, however wrong it looks.
    /// </summary>
    /// <remarks>
    /// The list is spelled out rather than derived, so promoting a rule to an error
    /// has to be a deliberate edit here. Every rule that warns fires on code with a
    /// reading that is correct, and an error would leave that author nothing but the
    /// id in an <c>.editorconfig</c>.
    /// </remarks>
    [Fact]
    public void OnlyTheRulesThatBlockGenerationAreErrors()
    {
        Descriptors()
           .Where(
                descriptor => descriptor.DefaultSeverity
                           == DiagnosticSeverity.Warning)
           .Select(descriptor => descriptor.Id)
           .ShouldBe(["WMSC0005"]);

        Descriptors()
           .ShouldAllBe(
                descriptor => descriptor.DefaultSeverity
                           == DiagnosticSeverity.Error
                           || descriptor.DefaultSeverity
                           == DiagnosticSeverity.Warning);
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
