namespace Waystone.Monads.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FreeDelegateAnalyzer : MonadAnalyzer
{
    internal static readonly Dictionary<string, string> EagerSiblings =
        new()
        {
            ["AndThen"] = "And",
            ["OrElse"] = "Or",
            ["UnwrapOrElse"] = "UnwrapOr",
            ["MapOrElse"] = "MapOr",
            ["OkOrElse"] = "OkOr",
        };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.FreeDelegatePassedToLazyMember);

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

        if (!EagerSiblings.TryGetValue(name, out string? eager)
         || !symbols.IsMonadInvocation(invocation))
        {
            return;
        }

        if (FirstDelegateBodyOf(invocation) is not { } body || !IsFree(body))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.FreeDelegatePassedToLazyMember,
                Semantics.NameLocationOf(invocation),
                name,
                eager));
    }

    /// <remarks>
    /// The first delegate argument is the deferred value on every member here.
    /// <c>MapOrElse</c> is the one that takes two, and its second is the map,
    /// which has no eager sibling to move to. Reading the first also steps over
    /// the leading <c>state</c> of the state overloads without naming it.
    /// </remarks>
    private static IOperation? FirstDelegateBodyOf(IInvocationOperation invocation)
    {
        foreach (var argument in invocation.Arguments)
        {
            if (Semantics.LambdaIn(argument.Value) is { } lambda)
            {
                return ReturnedBy(lambda);
            }
        }

        return null;
    }

    /// <remarks>
    /// Null for a lambda that does more than return an expression. A body with
    /// statements in it defers whatever those statements do, whatever the
    /// expression it ends on.
    /// </remarks>
    private static IOperation? ReturnedBy(IAnonymousFunctionOperation lambda)
    {
        if (lambda.Body is not { Operations.Length: 1 } body
         || body.Operations[0] is not IReturnOperation
            {
                ReturnedValue: { } returned,
            })
        {
            return null;
        }

        return Semantics.Unconverted(returned);
    }

    /// <remarks>
    /// Narrower than <c>LazyVariantAnalyzer.CostOf</c> on purpose; the descriptor
    /// carries why. A property reference is absent because its getter may compute,
    /// and a composite is absent because the parts it is built from would each have
    /// to be judged.
    /// </remarks>
    private static bool IsFree(IOperation operation) =>
        operation.ConstantValue.HasValue
     || operation is ILocalReferenceOperation
                  or IParameterReferenceOperation
                  or IDefaultValueOperation;
}
