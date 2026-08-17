namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GuardAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.GuardedUnwrap,
            Rules.CheckCombinedWithUnwrap);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols)
    {
        context.RegisterOperationAction(
            operation => AnalyzeConditional(operation, symbols),
            OperationKind.Conditional);

        context.RegisterOperationAction(
            operation => AnalyzeBinary(operation, symbols),
            OperationKind.Binary);
    }

    private static void AnalyzeConditional(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var conditional = (IConditionalOperation)context.Operation;

        var (instance, member) = Semantics.StateCheck(
            conditional.Condition,
            symbols);

        if (instance is null || member is null)
        {
            return;
        }

        var branch = member is "IsSome" or "IsOk"
            ? conditional.WhenTrue
            : conditional.WhenFalse;

        if (branch is null
         || !Semantics.ContainsPanickingCallOn(branch, instance, symbols))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.GuardedUnwrap,
                conditional.Condition.Syntax.GetLocation(),
                member));
    }

    private static (ISymbol? Instance, string? Member) StateCheckIn(
        IOperation operand,
        BinaryOperatorKind kind,
        MonadSymbols symbols)
    {
        var check = Semantics.StateCheck(operand, symbols);

        if (check.Instance is not null)
        {
            return check;
        }

        return operand is IBinaryOperation nested
            && nested.OperatorKind == kind
                ? StateCheckIn(nested.RightOperand, kind, symbols)
                : (null, null);
    }

    private static void AnalyzeBinary(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var binary = (IBinaryOperation)context.Operation;

        if (binary.OperatorKind is not (BinaryOperatorKind.ConditionalAnd
            or BinaryOperatorKind.ConditionalOr))
        {
            return;
        }

        var (instance, member) = StateCheckIn(
            binary.LeftOperand,
            binary.OperatorKind,
            symbols);

        if (instance is null || member is null)
        {
            return;
        }

        bool conjunction =
            binary.OperatorKind == BinaryOperatorKind.ConditionalAnd;

        string? replacement = (member, conjunction) switch
        {
            ("IsSome", true) => "IsSomeAnd",
            ("IsOk", true) => "IsOkAnd",
            ("IsErr", true) => "IsErrAnd",
            ("IsNone", false) => "IsNoneOr",
            _ => null,
        };

        if (replacement is null
         || !Semantics.ContainsPanickingCallOn(
                binary.RightOperand,
                instance,
                symbols))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.CheckCombinedWithUnwrap,
                binary.Syntax.GetLocation(),
                member,
                replacement));
    }
}
