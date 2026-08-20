namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TryFactoryAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.AsyncFactoryPassedToTry);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols) =>
        context.RegisterOperationAction(
            operation => Analyze(operation, symbols),
            OperationKind.Invocation);

    private static void Analyze(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        if (method.Name != "Try"
         || method.TypeArguments.Length == 0
         || !IsFactory(method.ContainingType, symbols))
        {
            return;
        }

        var valueType = method.TypeArguments[0];

        if (!IsAwaitable(valueType, symbols))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.AsyncFactoryPassedToTry,
                Semantics.NameLocationOf(invocation),
                Semantics.Display(valueType)));
    }

    private static bool IsFactory(
        INamedTypeSymbol? containingType,
        MonadSymbols symbols) =>
        SymbolEqualityComparer.Default.Equals(
            containingType,
            symbols.OptionFactory)
     || SymbolEqualityComparer.Default.Equals(
            containingType,
            symbols.ResultFactory);

    private static bool IsAwaitable(
        ITypeSymbol type,
        MonadSymbols symbols) =>
        type is INamedTypeSymbol { IsGenericType: true } named
     && (SymbolEqualityComparer.Default.Equals(
             named.OriginalDefinition,
             symbols.Task)
      || SymbolEqualityComparer.Default.Equals(
             named.OriginalDefinition,
             symbols.ValueTask));
}
