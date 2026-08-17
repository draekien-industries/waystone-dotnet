namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullableReturnAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.NullableReturnCouldBeOption);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols) =>
        context.RegisterSymbolAction(
            symbol => Analyze(symbol, symbols),
            SymbolKind.Method,
            SymbolKind.Property);

    private static void Analyze(
        SymbolAnalysisContext context,
        MonadSymbols symbols)
    {
        var member = context.Symbol;

        if (!Members.IsOrdinary(member))
        {
            return;
        }

        var returned = Members.ReturnTypeOf(member);

        if (returned is null
         || !Semantics.IsNullable(returned)
         || symbols.IsMonad(returned))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.NullableReturnCouldBeOption,
                Semantics.TypeLocationOf(member),
                member.Name,
                returned.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat),
                Semantics.Display(returned)));
    }
}
