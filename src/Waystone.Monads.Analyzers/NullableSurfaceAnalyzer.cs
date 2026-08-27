namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullableSurfaceAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.NullableMemberAlongsideMonads);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols) =>
        context.RegisterSymbolAction(
            symbol => Analyze(symbol, symbols),
            SymbolKind.NamedType);

    private static void Analyze(
        SymbolAnalysisContext context,
        MonadSymbols symbols)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        var monadMember = type.GetMembers()
           .FirstOrDefault(
                member => Members.IsOrdinary(member)
                       && symbols.IsMonad(
                              symbols.UnwrapAwaitable(
                                  Members.ReturnTypeOf(member))));

        if (monadMember is null)
        {
            return;
        }

        string convention = symbols.IsOption(
            symbols.UnwrapAwaitable(Members.ReturnTypeOf(monadMember)))
            ? "Option"
            : "Result";

        foreach (var member in type.GetMembers())
        {
            if (!Members.IsOrdinary(member)
             || SymbolEqualityComparer.Default.Equals(member, monadMember))
            {
                continue;
            }

            var returned =
                symbols.UnwrapAwaitable(Members.ReturnTypeOf(member));

            if (returned is null
             || !Semantics.IsNullable(returned)
             || symbols.IsMonad(returned))
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rules.NullableMemberAlongsideMonads,
                    Semantics.TypeLocationOf(member),
                    member.Name,
                    monadMember.Name,
                    convention));
        }
    }
}
