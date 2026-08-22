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
        if (symbols.ErrorCodeCatalogAttribute is null) return;

        var declarations = new ConcurrentBag<ErrorCodeCatalogs.Declared>();

        string? assemblyFormat =
            ErrorCodeCatalogs.AssemblyFormat(context.Compilation);

        context.RegisterSymbolAction(
            symbol =>
            {
                foreach (ErrorCodeCatalogs.Declared declared in
                         ErrorCodeCatalogs.Collect(
                             (INamedTypeSymbol)symbol.Symbol,
                             symbols,
                             assemblyFormat))
                {
                    declarations.Add(declared);
                }
            },
            SymbolKind.NamedType);

        context.RegisterCompilationEndAction(end => Report(end, declarations));
    }

    private static void Report(
        CompilationAnalysisContext context,
        ConcurrentBag<ErrorCodeCatalogs.Declared> declarations)
    {
        foreach (IGrouping<string, ErrorCodeCatalogs.Declared> collision in declarations
                    .GroupBy(declared => declared.Code, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1))
        {
            List<ErrorCodeCatalogs.Declared> ordered = collision
               .OrderBy(
                    declared => declared.Member.ToDisplayString(),
                    StringComparer.Ordinal)
               .ToList();

            ErrorCodeCatalogs.Declared first = ordered[0];

            foreach (ErrorCodeCatalogs.Declared later in ordered.Skip(1))
            {
                if (SymbolEqualityComparer.Default.Equals(
                        first.Member.ContainingType,
                        later.Member.ContainingType))
                {
                    continue;
                }

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rules.ErrorCodeReusedAcrossEnums,
                        later.Member.Locations.FirstOrDefault(
                            location => location.IsInSource),
                        first.Member.ToDisplayString(),
                        later.Member.ToDisplayString(),
                        collision.Key));
            }
        }
    }
}
