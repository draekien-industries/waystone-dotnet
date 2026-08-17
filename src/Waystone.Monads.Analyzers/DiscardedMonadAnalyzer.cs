namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DiscardedMonadAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.ResultDiscarded, Rules.OptionDiscarded);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols) =>
        context.RegisterOperationAction(
            operation => Analyze(operation, symbols),
            OperationKind.ExpressionStatement);

    private static void Analyze(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var statement = (IExpressionStatementOperation)context.Operation;

        var value = statement.Operation is IAwaitOperation await
            ? await.Operation
            : statement.Operation;

        if (value is not IInvocationOperation invocation)
        {
            return;
        }

        var returned = symbols.UnwrapAwaitable(invocation.Type);

        var rule = symbols.IsResult(returned) ? Rules.ResultDiscarded
            : symbols.IsOption(returned) ? Rules.OptionDiscarded
            : null;

        if (rule is null)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                rule,
                Semantics.NameLocationOf(invocation),
                returned!.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }
}
