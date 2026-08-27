namespace Waystone.Monads.Shouldly.Analyzers;

using global::Shouldly;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Xunit;

/// <remarks>
/// Mirrors <c>PresetTests</c> in Waystone.Monads.Analyzers, for the same reason
/// <c>RulesTests</c> is mirrored: the presets are per package, because the analyzers
/// are, and a consumer who installed only the core package must not be handed
/// severities for rules nothing in their build reports.
/// <para>
/// Both packages read one <c>WaystoneMonadsRuleset</c> property, so a consumer sets a
/// posture once. That sharing is the reason these files have to agree about what each
/// tier name means, and it is what these tests hold.
/// </para>
/// </remarks>
public class PresetTests
{
    public static TheoryData<string> Presets() =>
        new TheoryData<string> { "recommended", "strict" };

    [Theory]
    [MemberData(nameof(Presets))]
    public void EveryRuleAppearsInThePreset(string preset)
    {
        IReadOnlyDictionary<string, ReportDiagnostic> severities =
            SeveritiesIn(preset);

        List<string> missing = Ids
                              .Where(id => !severities.ContainsKey(id))
                              .OrderBy(id => id, StringComparer.Ordinal)
                              .ToList();

        missing.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Presets))]
    public void ThePresetNamesNoRuleThatDoesNotExist(string preset)
    {
        List<string> unknown = SeveritiesIn(preset)
                              .Keys.Where(id => !Ids.Contains(id))
                              .OrderBy(id => id, StringComparer.Ordinal)
                              .ToList();

        unknown.ShouldBeEmpty();
    }

    /// <summary>
    /// <c>recommended</c> changes nothing here, and that is the claim worth pinning.
    /// </summary>
    /// <remarks>
    /// There is no misuse tier in this package to promote — every rule fires on a test
    /// that passes. The file exists so that a consumer setting the shared property
    /// gets a defined answer from this package rather than an error, and if somebody
    /// later raises a rule in it, this is what asks them to justify it.
    /// </remarks>
    [Fact]
    public void RecommendedLeavesEveryRuleAtItsShippedSeverity() =>
        ExpectEvery("recommended", ReportDiagnostic.Info);

    /// <summary>
    /// <c>strict</c> raises both rules to warning, matching what the core package's
    /// <c>strict</c> does to its own idiom tier.
    /// </summary>
    [Fact]
    public void StrictRaisesEveryRuleToWarning() =>
        ExpectEvery("strict", ReportDiagnostic.Warn);

    /// <summary>
    /// The preset resolves as a global config and contributes nothing path-matched.
    /// </summary>
    /// <remarks>
    /// Neither rule here is reported against a file with no syntax tree, so globalness
    /// buys this package less than it buys the core one. It is kept for uniformity:
    /// one shared property selects both packages' presets, and a consumer who reads
    /// one file and then the other should not find them built differently.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Presets))]
    public void ThePresetResolvesAsAGlobalConfig(string preset)
    {
        AnalyzerConfigSet set = Load(preset);

        set.GlobalConfigOptions.TreeOptions.ShouldNotBeEmpty();
        set.GetOptionsForSourcePath("/subject.cs").TreeOptions.ShouldBeEmpty();
    }

    /// <summary>
    /// The preset loses to a consumer's own global config on a conflict.
    /// </summary>
    /// <remarks>
    /// A consumer's <c>.globalconfig</c> defaults to level 100 and any other global
    /// config to 0, so anything at or above 0 risks a tie — and Roslyn resolves a tie
    /// by *unsetting* the option, which drops the rule silently back to its shipped
    /// severity rather than reporting a conflict. Read from the text because
    /// <c>AnalyzerConfig.GlobalLevel</c> is internal to Roslyn.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Presets))]
    public void ThePresetSitsBelowAConsumersOwn(string preset) =>
        int.Parse(Properties(preset)["global_level"]).ShouldBeLessThan(0);

    [Theory]
    [MemberData(nameof(Presets))]
    public void ThePresetSetsSeveritiesAndNothingElse(string preset)
    {
        List<string> other = Properties(preset)
                            .Keys
                            .Where(key => key is not ("is_global" or "global_level"))
                            .Where(
                                 key => !key.StartsWith(
                                     "dotnet_diagnostic.",
                                     StringComparison.Ordinal))
                            .OrderBy(key => key, StringComparer.Ordinal)
                            .ToList();

        other.ShouldBeEmpty();
    }

    private static void ExpectEvery(string preset, ReportDiagnostic expected)
    {
        IReadOnlyDictionary<string, ReportDiagnostic> severities =
            SeveritiesIn(preset);

        List<string> offenders =
            Ids.Where(id => severities[id] != expected)
               .Select(id => $"{id} is {severities[id]}, expected {expected}")
               .OrderBy(text => text, StringComparer.Ordinal)
               .ToList();

        offenders.ShouldBeEmpty();
    }

    /// <remarks>
    /// Read off <c>GlobalConfigOptions</c> rather than a source path: Roslyn collects
    /// a global config's severities there and leaves the per-path result to sectioned
    /// configs, so resolving against a path returns nothing for these files.
    /// </remarks>
    private static IReadOnlyDictionary<string, ReportDiagnostic> SeveritiesIn(
        string preset) =>
        Load(preset).GlobalConfigOptions.TreeOptions;

    private static AnalyzerConfigSet Load(string preset)
    {
        string path = Path.Combine(
            Path.GetDirectoryName(typeof(PresetTests).Assembly.Location)!,
            "presets",
            $"{preset}.globalconfig");

        return AnalyzerConfigSet.Create(
            ImmutableArray.Create(
                AnalyzerConfig.Parse(File.ReadAllText(path), path)));
    }

    /// <remarks>
    /// Scanned out of the file rather than read off <c>AnalyzerConfig</c>, whose
    /// <c>GlobalSection</c> is internal. A preset carries no <c>[section]</c> header
    /// by construction, so every key is a global one and a flat scan is exact.
    /// </remarks>
    private static IReadOnlyDictionary<string, string> Properties(string preset)
    {
        string path = Path.Combine(
            Path.GetDirectoryName(typeof(PresetTests).Assembly.Location)!,
            "presets",
            $"{preset}.globalconfig");

        return File.ReadAllLines(path)
                   .Select(line => line.Trim())
                   .Where(
                        line => line.Length > 0
                             && !line.StartsWith("#", StringComparison.Ordinal))
                   .Select(line => line.Split(['='], 2))
                   .ToDictionary(
                        parts => parts[0].Trim(),
                        parts => parts[1].Trim());
    }

    private static ImmutableHashSet<string> Ids => RuleCatalog.Ids;
}
