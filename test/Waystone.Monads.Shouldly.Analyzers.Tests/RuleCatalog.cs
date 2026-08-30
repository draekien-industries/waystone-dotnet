namespace Waystone.Monads.Shouldly.Analyzers;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

/// <summary>
/// Every rule this assembly declares, read off <see cref="Rules" /> by reflection.
/// </summary>
/// <remarks>
/// One reflection query rather than one per test class. Both <c>RulesTests</c> and
/// <c>PresetTests</c> ask the same question of the same type, and the question is
/// about how rules are *declared* — a field that stopped being public static, or a
/// second container type, would need finding in one place rather than two.
/// <para>
/// Not shared with the Shouldly analyzer test project, which has its own copy. The
/// two <c>Rules</c> classes are different types in different assemblies, and that
/// assembly's <c>EveryRuleIsSupportedByAnAnalyzer</c> reflects over its own analyzer
/// types — merging them would weaken both checks.
/// </para>
/// </remarks>
internal static class RuleCatalog
{
    public static ImmutableArray<DiagnosticDescriptor> Descriptors { get; } =
        typeof(Rules)
           .GetFields(BindingFlags.Public | BindingFlags.Static)
           .Where(member => member.FieldType == typeof(DiagnosticDescriptor))
           .Select(member => (DiagnosticDescriptor)member.GetValue(null)!)
           .ToImmutableArray();

    /// <remarks>
    /// Case-insensitive because Roslyn lowercases a diagnostic id when it parses
    /// <c>dotnet_diagnostic.WM1001.severity</c>. Its own <c>TreeOptions</c> dictionary
    /// is case-insensitive too, so a lookup works either way — but the keys come back
    /// lowered, and a set that compared them ordinally would report every rule as
    /// unknown.
    /// </remarks>
    public static ImmutableHashSet<string> Ids { get; } =
        Descriptors.Select(rule => rule.Id)
                   .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
}
