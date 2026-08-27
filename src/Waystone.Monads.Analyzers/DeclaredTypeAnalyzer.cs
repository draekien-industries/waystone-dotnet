namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DeclaredTypeAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.NullableMonadDeclared,
            Rules.NestedOption,
            Rules.DerivedMonadTypeDeclared);

    private static readonly ImmutableHashSet<string> MonadNames =
        ImmutableHashSet.Create(
            "Option",
            "Some",
            "None",
            "Result",
            "Ok",
            "Err");

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols) =>
        context.RegisterSyntaxNodeAction(
            node => Analyze(node, symbols),
            SyntaxKind.GenericName);

    private static void Analyze(
        SyntaxNodeAnalysisContext context,
        MonadSymbols symbols)
    {
        var node = (GenericNameSyntax)context.Node;

        if (!MonadNames.Contains(node.Identifier.ValueText))
        {
            return;
        }

        if (!Semantics.IsDeclarationTypePosition(node))
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(node, context.CancellationToken)
                .Symbol is not INamedTypeSymbol type)
        {
            return;
        }

        var location = node.GetLocation();

        if (node.Parent is NullableTypeSyntax annotated)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rules.NullableMonadDeclared,
                    annotated.GetLocation(),
                    Semantics.Display(type),
                    symbols.IsOption(type) ? "None" : "Err"));
        }

        if (symbols.IsDerivedCase(type))
        {
            var declared = symbols.BaseCaseOf(type);

            if (declared is not null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rules.DerivedMonadTypeDeclared,
                        location,
                        Semantics.Display(type),
                        Semantics.Display(declared)));
            }

            return;
        }

        var arguments = symbols.TypeArgumentsOf(type);

        if (symbols.IsOption(type)
         && arguments.Length == 1
         && symbols.IsOption(arguments[0]))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rules.NestedOption,
                    location,
                    Semantics.Display(type)));
        }
    }
}
