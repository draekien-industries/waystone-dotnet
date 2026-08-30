namespace Waystone.Monads.Shouldly.Analyzers;

using global::Shouldly;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

/// <remarks>
/// Mirrors <c>RulesTests</c> in Waystone.Monads.Analyzers rather than sharing it. The
/// two rule sets live in different assemblies with different id prefixes, and that
/// assembly's <c>EveryRuleIsSupportedByAnAnalyzer</c> reflects over its own types — so
/// a WMS rule cannot be registered there without weakening the check that every WM
/// rule has an analyzer.
/// </remarks>
public class RulesTests
{
    public static TheoryData<string> IdiomRules() => IdsStartingWith("WMS2");

    public static TheoryData<string> AllRules() => IdsStartingWith("WMS");

    [Theory]
    [MemberData(nameof(IdiomRules))]
    public void IdiomRulesInformAndAreOnByDefault(string id)
    {
        Rule(id).DefaultSeverity.ShouldBe(DiagnosticSeverity.Info);
        Rule(id).IsEnabledByDefault.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(AllRules))]
    public void EveryRuleCarriesAHelpLink(string id) =>
        Rule(id)
           .HelpLinkUri.ShouldBe(
                "https://draekien-industries.wpei.me/analyzers/assertion-rules#"
              + id.ToLowerInvariant());

    [Theory]
    [MemberData(nameof(AllRules))]
    public void EveryTitleFitsAnErrorListColumn(string id) =>
        Rule(id).Title.ToString().Length.ShouldBeLessThanOrEqualTo(60);

    [Theory]
    [MemberData(nameof(AllRules))]
    public void EveryRuleUsesTheIdiomTier(string id) =>
        id.ShouldStartWith("WMS2");

    /// <summary>
    /// No rule here carries a custom tag, and none should acquire one silently.
    /// </summary>
    /// <remarks>
    /// <c>Unnecessary</c> is the tag a WM2 rule with a fix normally takes, and both
    /// rules here decline it: the reported span is a whole assertion and the fix
    /// replaces it, so fading the span in an IDE would read as an invitation to delete
    /// a test's only check. Nothing in the build would notice the tag being added, so
    /// it is pinned.
    /// </remarks>
    [Fact]
    public void NoRuleCarriesACustomTag() =>
        Descriptors.SelectMany(rule => rule.CustomTags).ShouldBeEmpty();

    [Fact]
    public void EveryTierIsPopulated() => IdiomRules().Count.ShouldBe(2);

    /// <remarks>
    /// Every one of these rules fires on code that works, so none may ship above
    /// <c>Info</c>. There is no misuse tier here to promote one into.
    /// </remarks>
    [Fact]
    public void NoRuleShipsAtWarningOrAbove() =>
        Descriptors
           .Where(rule => rule.DefaultSeverity >= DiagnosticSeverity.Warning)
           .Select(rule => rule.Id)
           .ShouldBeEmpty();

    [Fact]
    public void RuleIdsAreUnique()
    {
        var ids = Descriptors.Select(rule => rule.Id).ToList();

        ids.Distinct().Count().ShouldBe(ids.Count);
    }

    [Fact]
    public void EveryRuleIsSupportedByAnAnalyzer()
    {
        var supported = typeof(AssertionAnalyzer).Assembly.GetTypes()
           .Where(
                type => !type.IsAbstract
                     && typeof(AssertionAnalyzer).IsAssignableFrom(type))
           .Select(type => (AssertionAnalyzer)Activator.CreateInstance(type)!)
           .SelectMany(analyzer => analyzer.SupportedDiagnostics)
           .Select(rule => rule.Id)
           .Distinct()
           .OrderBy(id => id, StringComparer.Ordinal);

        supported.ShouldBe(
            Descriptors.Select(rule => rule.Id)
               .OrderBy(id => id, StringComparer.Ordinal));
    }

    /// <remarks>
    /// A fix registered on an id no descriptor declares is dead code that no test
    /// would otherwise reach, and the reverse — a rule whose message names a
    /// replacement with no fix to write it — is the failure a consumer sees.
    /// </remarks>
    [Fact]
    public void EveryRuleHasACodeFix()
    {
        var fixable = typeof(AssertionCodeFix).Assembly.GetTypes()
           .Where(
                type => !type.IsAbstract
                     && typeof(AssertionCodeFix).IsAssignableFrom(type))
           .Select(type => (AssertionCodeFix)Activator.CreateInstance(type)!)
           .SelectMany(fixer => fixer.FixableDiagnosticIds)
           .Distinct()
           .OrderBy(id => id, StringComparer.Ordinal);

        fixable.ShouldBe(
            Descriptors.Select(rule => rule.Id)
               .OrderBy(id => id, StringComparer.Ordinal));
    }

    private static DiagnosticDescriptor Rule(string id) =>
        Descriptors.Single(rule => rule.Id == id);

    private static TheoryData<string> IdsStartingWith(string prefix)
    {
        var data = new TheoryData<string>();

        foreach (var rule in Descriptors.Where(
                     rule => rule.Id.StartsWith(
                         prefix,
                         StringComparison.Ordinal)))
        {
            data.Add(rule.Id);
        }

        return data;
    }

    private static IEnumerable<DiagnosticDescriptor> Descriptors =>
        RuleCatalog.Descriptors;
}
