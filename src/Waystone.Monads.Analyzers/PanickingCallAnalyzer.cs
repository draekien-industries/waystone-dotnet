namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PanickingCallAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.UnwrapUsed, Rules.ExpectUsed);

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
        string name = invocation.TargetMethod.Name;

        var rule = Semantics.UnwrapNames.Contains(name) ? Rules.UnwrapUsed
            : Semantics.ExpectNames.Contains(name) ? Rules.ExpectUsed
            : null;

        if (rule is null || !symbols.IsMonadInvocation(invocation))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(rule, Semantics.NameLocationOf(invocation), name));
    }
}
