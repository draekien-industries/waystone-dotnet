namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DeprecationAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.RenamedToAndThen);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols)
    {
        context.RegisterOperationAction(
            operation => AnalyzeInvocation(operation, symbols),
            OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var invocation = (IInvocationOperation)context.Operation;

        string replacement = invocation.TargetMethod.Name switch
        {
            "FlatMap" => "AndThen",
            "FlatMapAsync" => "AndThenAsync",
            _ => string.Empty,
        };

        if (replacement.Length == 0 || !symbols.IsMonadInvocation(invocation))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.RenamedToAndThen,
                Semantics.NameLocationOf(invocation),
                invocation.TargetMethod.Name,
                replacement));
    }
}
