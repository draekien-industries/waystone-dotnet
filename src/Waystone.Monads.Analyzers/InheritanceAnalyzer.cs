namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InheritanceAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.DerivesFromMonad);

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

        if (type.BaseType is null
         || !symbols.IsMonad(type.BaseType)
         || IsLibraryCase(type, symbols))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.DerivesFromMonad,
                LocationOf(type, type.BaseType.Name),
                Semantics.Display(type),
                Semantics.Display(type.BaseType)));
    }

    private static bool IsLibraryCase(
        INamedTypeSymbol type,
        MonadSymbols symbols)
    {
        var definition = type.OriginalDefinition;

        return SymbolEqualityComparer.Default.Equals(definition, symbols.Some)
            || SymbolEqualityComparer.Default.Equals(definition, symbols.None)
            || SymbolEqualityComparer.Default.Equals(definition, symbols.Ok)
            || SymbolEqualityComparer.Default.Equals(definition, symbols.Err);
    }

    private static Location LocationOf(
        INamedTypeSymbol type,
        string baseName)
    {
        var declared = type.DeclaringSyntaxReferences
           .Select(reference => reference.GetSyntax())
           .OfType<TypeDeclarationSyntax>()
           .Where(declaration => declaration.BaseList is not null)
           .SelectMany(declaration => declaration.BaseList!.Types)
           .FirstOrDefault(
                baseType => NameOf(baseType.Type) == baseName);

        return declared?.Type.GetLocation()
            ?? type.Locations.FirstOrDefault()
            ?? Location.None;
    }

    private static string? NameOf(TypeSyntax type) =>
        type switch
        {
            GenericNameSyntax generic => generic.Identifier.ValueText,
            SimpleNameSyntax simple => simple.Identifier.ValueText,
            QualifiedNameSyntax qualified => NameOf(qualified.Right),
            _ => null,
        };
}
