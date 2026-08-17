namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SimplificationAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.MapThenFlatten,
            Rules.UnwrapOrWithDefault,
            Rules.MonadComparedToNull);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols)
    {
        context.RegisterOperationAction(
            operation => AnalyzeInvocation(operation, symbols),
            OperationKind.Invocation);

        context.RegisterOperationAction(
            operation => AnalyzeComparison(operation, symbols),
            OperationKind.Binary);

        context.RegisterOperationAction(
            operation => AnalyzePattern(operation, symbols),
            OperationKind.IsPattern);
    }

    private static void AnalyzeInvocation(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var invocation = (IInvocationOperation)context.Operation;
        string name = invocation.TargetMethod.Name;

        if (!symbols.IsMonadInvocation(invocation))
        {
            return;
        }

        if (name is "Flatten" or "FlattenAsync")
        {
            AnalyzeFlatten(context, invocation, symbols);

            return;
        }

        if (name is "UnwrapOr" or "UnwrapOrAsync")
        {
            AnalyzeUnwrapOr(context, invocation);
        }
    }

    private static void AnalyzeFlatten(
        OperationAnalysisContext context,
        IInvocationOperation invocation,
        MonadSymbols symbols)
    {
        if (Semantics.Unconverted(
                Semantics.ReceiverOf(invocation) ?? invocation)
            is not IInvocationOperation receiver
         || receiver.TargetMethod.Name is not ("Map" or "MapAsync")
         || !symbols.IsMonadInvocation(receiver))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.MapThenFlatten,
                Semantics.NameLocationOf(invocation)));
    }

    private static void AnalyzeUnwrapOr(
        OperationAnalysisContext context,
        IInvocationOperation invocation)
    {
        if (invocation.Arguments.Length == 0
         || invocation.Type is null)
        {
            return;
        }

        var fallback = invocation.Arguments[invocation.Arguments.Length - 1]
           .Value;

        if (!Semantics.IsDefaultValue(fallback))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.UnwrapOrWithDefault,
                Semantics.NameLocationOf(invocation),
                Semantics.Display(invocation.Type)));
    }

    private static void AnalyzeComparison(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var binary = (IBinaryOperation)context.Operation;

        if (binary.OperatorKind is not (BinaryOperatorKind.Equals
            or BinaryOperatorKind.NotEquals))
        {
            return;
        }

        var monad = NullComparisonSubject(binary, symbols);

        if (monad?.Type is null)
        {
            return;
        }

        Report(
            context,
            binary.Syntax.GetLocation(),
            monad.Type,
            binary.OperatorKind == BinaryOperatorKind.Equals,
            symbols);
    }

    private static void AnalyzePattern(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var pattern = (IIsPatternOperation)context.Operation;

        var value = pattern.Value;

        if (value.Type is null || !symbols.IsMonad(value.Type))
        {
            return;
        }

        bool negated = pattern.Pattern is INegatedPatternOperation;

        var inner = negated
            ? ((INegatedPatternOperation)pattern.Pattern).Pattern
            : pattern.Pattern;

        if (inner is not IConstantPatternOperation
            {
                Value.ConstantValue: { HasValue: true, Value: null },
            })
        {
            return;
        }

        Report(
            context,
            pattern.Syntax.GetLocation(),
            value.Type,
            !negated,
            symbols);
    }

    private static void Report(
        OperationAnalysisContext context,
        Location location,
        ITypeSymbol type,
        bool testsForNull,
        MonadSymbols symbols)
    {
        string suggestion = symbols.IsOption(type)
            ? testsForNull ? "IsNone" : "IsSome"
            : testsForNull ? "IsErr" : "IsOk";

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.MonadComparedToNull,
                location,
                Semantics.Display(type),
                suggestion));
    }

    private static IOperation? NullComparisonSubject(
        IBinaryOperation binary,
        MonadSymbols symbols)
    {
        var left = Semantics.Unconverted(binary.LeftOperand);
        var right = Semantics.Unconverted(binary.RightOperand);

        if (IsNullConstant(left) && symbols.IsMonad(right.Type))
        {
            return right;
        }

        return IsNullConstant(right) && symbols.IsMonad(left.Type)
            ? left
            : null;
    }

    private static bool IsNullConstant(IOperation operation) =>
        operation.ConstantValue is { HasValue: true, Value: null };
}
