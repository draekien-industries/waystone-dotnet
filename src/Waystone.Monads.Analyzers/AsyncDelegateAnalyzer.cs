namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncDelegateAnalyzer : MonadAnalyzer
{
    private const string AsyncSuffix = "Async";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.AsyncDelegatePassedToSyncMethod);

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

        if (method.Name.EndsWith(AsyncSuffix)
         || !IsOurs(invocation, method, symbols)
         || !TakesADelegate(method))
        {
            return;
        }

        ImmutableArray<ITypeSymbol> produced =
            symbols.TypeArgumentsOf(invocation.Type);

        if (!produced.Any(type => IsAwaitable(type, symbols)))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.AsyncDelegatePassedToSyncMethod,
                Semantics.NameLocationOf(invocation),
                method.Name,
                Semantics.Display(invocation.Type!),
                method.Name + AsyncSuffix));
    }

    private static bool IsOurs(
        IInvocationOperation invocation,
        IMethodSymbol method,
        MonadSymbols symbols) =>
        IsFactory(method.ContainingType, symbols)
     || symbols.IsMonadInvocation(invocation);

    private static bool TakesADelegate(IMethodSymbol method) =>
        method.Parameters.Any(
            parameter => parameter.Type.TypeKind == TypeKind.Delegate);

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
