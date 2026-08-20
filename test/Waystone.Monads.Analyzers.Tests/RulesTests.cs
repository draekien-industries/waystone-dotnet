namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

public class RulesTests
{
    public static TheoryData<string> MisuseRules() => IdsStartingWith("WM1");

    public static TheoryData<string> IdiomRules() => IdsStartingWith("WM2");

    public static TheoryData<string> MigrationRules() => IdsStartingWith("WM3");

    public static TheoryData<string> AllRules() => IdsStartingWith("WM");

    [Theory]
    [MemberData(nameof(MisuseRules))]
    public void MisuseRulesWarnAndAreOnByDefault(string id)
    {
        Rule(id).DefaultSeverity.ShouldBe(DiagnosticSeverity.Warning);
        Rule(id).IsEnabledByDefault.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(IdiomRules))]
    public void IdiomRulesInformAndAreOnByDefault(string id)
    {
        Rule(id).DefaultSeverity.ShouldBe(DiagnosticSeverity.Info);
        Rule(id).IsEnabledByDefault.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(MigrationRules))]
    public void MigrationRulesAreOffByDefault(string id) =>
        Rule(id).IsEnabledByDefault.ShouldBeFalse();

    [Theory]
    [MemberData(nameof(AllRules))]
    public void EveryRuleCarriesAHelpLink(string id) =>
        Rule(id)
           .HelpLinkUri.ShouldBe(
                "https://draekien-industries.wpei.me/using-the-library/analyzer-rules#"
              + id.ToLowerInvariant());

    [Theory]
    [MemberData(nameof(AllRules))]
    public void EveryTitleFitsAnErrorListColumn(string id) =>
        Rule(id).Title.ToString().Length.ShouldBeLessThanOrEqualTo(60);

    [Theory]
    [MemberData(nameof(MigrationRules))]
    public void MigrationDescriptionsSayTheyAreOff(string id) =>
        Rule(id)
           .Description.ToString()
           .ShouldStartWith("Disabled by default.");

    [Fact]
    public void EveryTierIsPopulated()
    {
        MisuseRules().Count.ShouldBe(10);
        IdiomRules().Count.ShouldBe(14);
        MigrationRules().Count.ShouldBe(2);
    }

    [Fact]
    public void RuleIdsAreUnique()
    {
        var ids = Descriptors.Select(rule => rule.Id).ToList();

        ids.Distinct().Count().ShouldBe(ids.Count);
    }

    [Fact]
    public void EveryRuleIsSupportedByAnAnalyzer()
    {
        var supported = typeof(MonadAnalyzer).Assembly.GetTypes()
           .Where(
                type => !type.IsAbstract
                     && typeof(MonadAnalyzer).IsAssignableFrom(type))
           .Select(type => (MonadAnalyzer)Activator.CreateInstance(type)!)
           .SelectMany(analyzer => analyzer.SupportedDiagnostics)
           .Select(rule => rule.Id)
           .Distinct()
           .OrderBy(id => id, StringComparer.Ordinal);

        supported.ShouldBe(
            Descriptors.Select(rule => rule.Id)
               .OrderBy(id => id, StringComparer.Ordinal));
    }

    private static DiagnosticDescriptor Rule(string id) =>
        Descriptors.Single(rule => rule.Id == id);

    private static TheoryData<string> IdsStartingWith(string prefix)
    {
        var data = new TheoryData<string>();

        foreach (var rule in Descriptors.Where(
                     rule => rule.Id.StartsWith(prefix, StringComparison.Ordinal)))
        {
            data.Add(rule.Id);
        }

        return data;
    }

    private static IEnumerable<DiagnosticDescriptor> Descriptors =>
        typeof(Rules)
           .GetFields(BindingFlags.Public | BindingFlags.Static)
           .Where(member => member.FieldType == typeof(DiagnosticDescriptor))
           .Select(member => (DiagnosticDescriptor)member.GetValue(null)!);
}
