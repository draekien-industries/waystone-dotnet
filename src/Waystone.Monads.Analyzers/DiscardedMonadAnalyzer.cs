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

        var awaited = statement.Operation as IAwaitOperation;

        if (Semantics.Unconverted(awaited?.Operation ?? statement.Operation)
            is not IInvocationOperation invocation)
        {
            return;
        }

        if (invocation.TargetMethod.Name is "ConfigureAwait"
         && Semantics.ReceiverOf(invocation) is { } receiver
         && Semantics.Unconverted(receiver) is IInvocationOperation awaitedCall)
        {
            invocation = awaitedCall;
        }

        var returned = awaited is null
            ? symbols.UnwrapAwaitable(invocation.Type)
            : awaited.Type;

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
                invocation.TargetMethod.Name,
                Semantics.Display(returned!)));
    }
}
