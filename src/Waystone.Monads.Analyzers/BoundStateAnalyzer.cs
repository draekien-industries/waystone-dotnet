namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BoundStateAnalyzer : MonadAnalyzer
{
    private const string BinderMethodName = "With";

    private const string StateParameterName = "state";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.OptionBoundAsState);

    private protected override void Register(
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

        if (invocation.TargetMethod.Name != BinderMethodName
         || !BindsForAnOption(invocation.TargetMethod, symbols))
        {
            return;
        }

        ITypeSymbol? bound = BoundBy(invocation);

        if (!IsAnOption(bound, symbols))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.OptionBoundAsState,
                Semantics.NameLocationOf(invocation),
                Semantics.Display(bound!)));
    }

    private static bool BindsForAnOption(
        IMethodSymbol method,
        MonadSymbols symbols) =>
        method.ReturnType is INamedTypeSymbol { ContainingType: { } declaring }
     && symbols.IsOption(declaring);

    private static ITypeSymbol? BoundBy(IInvocationOperation invocation) =>
        invocation.Arguments
                  .Where(
                       argument =>
                           argument.Parameter?.Name == StateParameterName)
                  .Select(
                       argument =>
                           Semantics.Unconverted(argument.Value).Type)
                  .FirstOrDefault();

    private static bool IsAnOption(ITypeSymbol? bound, MonadSymbols symbols) =>
        symbols.IsOption(bound) || symbols.IsOption(symbols.BaseCaseOf(bound));
}
