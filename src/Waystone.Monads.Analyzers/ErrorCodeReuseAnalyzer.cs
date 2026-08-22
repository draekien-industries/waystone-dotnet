namespace Waystone.Monads.Analyzers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorCodeReuseAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.ErrorCodeReusedAcrossEnums);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols)
    {
        if (symbols.ErrorCodeProviderAttribute is null) return;

        var providers = new ConcurrentBag<IFieldSymbol>();

        context.RegisterSymbolAction(
            symbol => Collect((INamedTypeSymbol)symbol.Symbol, symbols, providers),
            SymbolKind.NamedType);

        context.RegisterCompilationEndAction(end => Report(end, providers));
    }

    private static void Collect(
        INamedTypeSymbol type,
        MonadSymbols symbols,
        ConcurrentBag<IFieldSymbol> providers)
    {
        if (type.TypeKind != TypeKind.Enum || !IsProvider(type, symbols)) return;

        foreach (IFieldSymbol member in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.IsConst) providers.Add(member);
        }
    }

    private static bool IsProvider(INamedTypeSymbol type, MonadSymbols symbols) =>
        type.GetAttributes()
            .Any(
                 attribute => SymbolEqualityComparer.Default.Equals(
                     attribute.AttributeClass,
                     symbols.ErrorCodeProviderAttribute));

    private static void Report(
        CompilationAnalysisContext context,
        ConcurrentBag<IFieldSymbol> providers)
    {
        foreach (IGrouping<string, IFieldSymbol> collision in providers
                    .GroupBy(CodeOf, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1))
        {
            List<IFieldSymbol> ordered = collision
                                        .OrderBy(
                                             member => member.ToDisplayString(),
                                             StringComparer.Ordinal)
                                        .ToList();

            IFieldSymbol first = ordered[0];

            foreach (IFieldSymbol later in ordered.Skip(1))
            {
                if (SymbolEqualityComparer.Default.Equals(
                        first.ContainingType,
                        later.ContainingType))
                {
                    continue;
                }

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rules.ErrorCodeReusedAcrossEnums,
                        later.Locations.FirstOrDefault(
                            location => location.IsInSource),
                        first.ToDisplayString(),
                        later.ToDisplayString(),
                        collision.Key));
            }
        }
    }

    private static string CodeOf(IFieldSymbol member) =>
        $"{member.ContainingType.Name}.{member.Name}";
}
