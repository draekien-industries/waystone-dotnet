namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

/// <remarks>
/// The two severity presets packed under <c>build/</c> are hand-written, and nothing
/// in the build reads them â€” a rule added to <c>Rules.cs</c> without a matching entry
/// would ship a preset that silently omits it, which is the drift this whole family
/// exists to prevent.
/// <para>
/// Every assertion here goes through Roslyn's own <see cref="AnalyzerConfig" />
/// parser rather than a regex over the file, so what is pinned is the severity a
/// compiler would actually resolve rather than the text we happened to write. That
/// is the difference between checking the file says <c>error</c> and checking the
/// rule becomes one.
/// </para>
/// </remarks>
public class PresetTests
{
    public static TheoryData<string> Presets() =>
        new TheoryData<string> { "recommended", "strict" };

    /// <summary>
    /// The drift guard. A rule added to any tier without a preset entry fails here.
    /// </summary>
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

    /// <summary>
    /// The other half of the guard: a retired id left behind in a preset sets the
    /// severity of a rule no analyzer reports, which Roslyn ignores in silence.
    /// </summary>
    /// <remarks>
    /// Not hypothetical â€” the shipped id space has gaps at WM1004, WM1007, WM1009,
    /// WM1010, WM2010 and WM2014, every one of them a rule that existed and was
    /// retired. Ids are never reused, so a leftover entry stays wrong forever.
    /// </remarks>
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
    /// <c>recommended</c>'s whole claim: the seven misuse rules break the build and
    /// nothing else moves.
    /// </summary>
    [Fact]
    public void RecommendedRaisesTheMisuseTierAndLeavesTheRestAlone()
    {
        IReadOnlyDictionary<string, ReportDiagnostic> severities =
            SeveritiesIn("recommended");

        ExpectTier(severities, "WM1", ReportDiagnostic.Error);
        ExpectTier(severities, "WM2", ReportDiagnostic.Info);
        ExpectTier(severities, "WM3", ReportDiagnostic.Suppress);
    }

    /// <summary>
    /// <c>strict</c> raises all three tiers, including the two that ship off.
    /// </summary>
    [Fact]
    public void StrictRaisesEveryTier()
    {
        IReadOnlyDictionary<string, ReportDiagnostic> severities =
            SeveritiesIn("strict");

        ExpectTier(severities, "WM1", ReportDiagnostic.Error);
        ExpectTier(severities, "WM2", ReportDiagnostic.Warn);
        ExpectTier(severities, "WM3", ReportDiagnostic.Warn);
    }

    /// <summary>
    /// The preset resolves as a global config and contributes nothing path-matched.
    /// </summary>
    /// <remarks>
    /// Being global is what makes the preset reach WM2020, whose diagnostic is
    /// reported against <c>ErrorCodes.txt</c> and so has no syntax tree for a
    /// path-matched section to be resolved against. Roslyn keeps the two kinds apart:
    /// a global config's severities land in <see cref="AnalyzerConfigSet.GlobalConfigOptions" />
    /// and a sectioned one's in the per-path result. Drop <c>is_global</c> and both
    /// come back empty — the file sets nothing at all and reports no error while doing
    /// it — so asserting the split pins which half the preset is on. Which ids are on
    /// that half is held by the two tests above; this one only says which half.
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
    /// severity rather than reporting a conflict. A negative level cannot tie with
    /// anything a consumer would write. Read from the text because
    /// <c>AnalyzerConfig.GlobalLevel</c> is internal to Roslyn.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Presets))]
    public void ThePresetSitsBelowAConsumersOwn(string preset) =>
        int.Parse(Properties(preset)["global_level"]).ShouldBeLessThan(0);

    /// <summary>
    /// The presets set severities and nothing else. An analyzer option or a formatting
    /// key would be a second thing the file does, unrelated to the tier it names.
    /// </summary>
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

    private static void ExpectTier(
        IReadOnlyDictionary<string, ReportDiagnostic> severities,
        string prefix,
        ReportDiagnostic expected)
    {
        List<string> offenders =
            Ids.Where(id => id.StartsWith(prefix, StringComparison.Ordinal))
               .Where(id => severities[id] != expected)
               .Select(id => $"{id} is {severities[id]}, expected {expected}")
               .OrderBy(text => text, StringComparer.Ordinal)
               .ToList();

        offenders.ShouldBeEmpty();
    }

    /// <remarks>
    /// Resolved against an arbitrary source path. A global config applies to every
    /// path by construction, so the choice cannot matter â€” and if a section ever
    /// crept in, this is where it would stop applying.
    /// </remarks>
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
    /// by construction, so every key is a global one and a flat scan is exact — and
    /// the section-free shape is itself pinned, since a header would put its keys in
    /// this dictionary under a name the callers do not expect.
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

    /// <remarks>
    /// Case-insensitive because Roslyn lowercases a diagnostic id when it parses
    /// <c>dotnet_diagnostic.WM1001.severity</c>. Its own <c>TreeOptions</c> dictionary
    /// is case-insensitive too, so a lookup works either way — but the keys come back
    /// lowered, and a set that compared them ordinally would report all 29 as unknown.
    /// </remarks>
    private static ImmutableHashSet<string> Ids { get; } =
        typeof(Rules)
           .GetFields(BindingFlags.Public | BindingFlags.Static)
           .Where(member => member.FieldType == typeof(DiagnosticDescriptor))
           .Select(member => ((DiagnosticDescriptor)member.GetValue(null)!).Id)
           .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
}
