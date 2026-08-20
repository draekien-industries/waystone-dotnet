namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LazyVariantAnalyzer : MonadAnalyzer
{
    private static readonly Dictionary<string, string> LazySiblings =
        new()
        {
            ["And"] = "AndThen",
            ["AndAsync"] = "AndThenAsync",
            ["Or"] = "OrElse",
            ["UnwrapOr"] = "UnwrapOrElse",
            ["UnwrapOrAsync"] = "UnwrapOrElseAsync",
            ["MapOr"] = "MapOrElse",
            ["MapOrAsync"] = "MapOrElseAsync",
            ["OkOr"] = "OkOrElse",
            ["OkOrAsync"] = "OkOrElseAsync",
        };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.EagerArgumentNotFree);

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
        var name = invocation.TargetMethod.Name;

        if (!LazySiblings.TryGetValue(name, out var lazy)
         || !symbols.IsMonadInvocation(invocation))
        {
            return;
        }

        var eager = EagerArgumentOf(invocation);

        if (eager is null || IsFree(eager))
        {
            return;
        }

        var receiver = Semantics.ReceiverOf(invocation);

        if (receiver?.Type is not { } receiverType)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.EagerArgumentNotFree,
                Semantics.NameLocationOf(invocation),
                name,
                Semantics.Display(receiverType),
                lazy));
    }

    private static IOperation? EagerArgumentOf(IInvocationOperation invocation)
    {
        var receiver = Semantics.ReceiverOf(invocation);

        foreach (var argument in invocation.Arguments)
        {
            if (argument.Value == receiver
             || argument.IsImplicit
             || argument.Parameter is null)
            {
                continue;
            }

            if (IsDelegate(argument.Parameter.Type))
            {
                continue;
            }

            return argument.Value;
        }

        return null;
    }

    private static bool IsDelegate(ITypeSymbol type) =>
        type.TypeKind == TypeKind.Delegate;

    private static bool IsFree(IOperation operation) =>
        operation.ConstantValue.HasValue
     || Semantics.Unconverted(operation) is ILocalReferenceOperation
            or IParameterReferenceOperation
            or IFieldReferenceOperation
            or IPropertyReferenceOperation
            or IDefaultValueOperation
            or IInstanceReferenceOperation
            or ILiteralOperation;
}
