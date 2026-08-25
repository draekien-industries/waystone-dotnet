namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        if (eager is null)
        {
            return;
        }

        var cost = CostOf(eager);

        if (cost == Cost.Free)
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
                lazy,
                ReasonFor(cost)));
    }

    private static string ReasonFor(Cost cost) =>
        cost == Cost.Mutating
            ? "and evaluating it changes state"
            : "and computing it may be expensive";

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

    /// <remarks>
    /// Ordered so that the worst finding in a compound expression wins, which
    /// is what lets <see cref="Max" /> combine operands without the caller
    /// tracking which branch produced the answer.
    /// </remarks>
    private enum Cost
    {
        Free = 0,
        Computed = 1,
        Mutating = 2,
    }

    /// <remarks>
    /// Recursive rather than a match on the outermost operation, because an
    /// expression built out of free parts is itself free: a leaf-only test
    /// reports <c>fallback + 1</c> and <c>defaults[0]</c>, where the lazy
    /// sibling buys a delegate allocation and avoids nothing. Every composite
    /// admitted here is one the compiler lowers to arithmetic or a load, and
    /// each is gated on the operand that would make it a call instead —
    /// <see cref="IBinaryOperation.OperatorMethod" /> and its unary and
    /// conversion counterparts are how user-defined operators stay reportable.
    /// Anything unrecognised is <see cref="Cost.Computed" />, so an operation
    /// kind nobody thought about keeps the rule's existing answer.
    /// <see cref="Cost.Mutating" /> is reported only where the walk reaches the
    /// mutation itself; one buried in a call's arguments reads as
    /// <see cref="Cost.Computed" />, because the walk stops at a call it
    /// cannot judge. Both reasons carry the same fix, so the imprecision costs
    /// the reader a weaker sentence rather than a wrong one.
    /// </remarks>
    private static Cost CostOf(IOperation operation)
    {
        if (operation.ConstantValue.HasValue)
        {
            return Cost.Free;
        }

        switch (operation)
        {
            case ILocalReferenceOperation:
            case IParameterReferenceOperation:
            case IFieldReferenceOperation:
            case IPropertyReferenceOperation:
            case IDefaultValueOperation:
            case IInstanceReferenceOperation:
            case ILiteralOperation:
            case ITypeOfOperation:
                return Cost.Free;

            case IIncrementOrDecrementOperation:
            case ISimpleAssignmentOperation:
            case ICompoundAssignmentOperation:
            case ICoalesceAssignmentOperation:
                return Cost.Mutating;

            case IConversionOperation conversion:
                return conversion.OperatorMethod is null
                    ? CostOf(conversion.Operand)
                    : Cost.Computed;

            case IParenthesizedOperation parenthesized:
                return CostOf(parenthesized.Operand);

            case IUnaryOperation unary:
                return unary.OperatorMethod is null
                    ? CostOf(unary.Operand)
                    : Cost.Computed;

            case IBinaryOperation binary:
                return binary.OperatorMethod is null
                    ? Max(CostOf(binary.LeftOperand), CostOf(binary.RightOperand))
                    : Cost.Computed;

            case ICoalesceOperation coalesce:
                return Max(CostOf(coalesce.Value), CostOf(coalesce.WhenNull));

            case IConditionalOperation conditional:
                return conditional.WhenFalse is { } whenFalse
                    ? Max(
                        CostOf(conditional.Condition),
                        Max(CostOf(conditional.WhenTrue), CostOf(whenFalse)))
                    : Cost.Computed;

            case IArrayElementReferenceOperation element:
                return element.Indices.Aggregate(
                    CostOf(element.ArrayReference),
                    (worst, index) => Max(worst, CostOf(index)));

            case ITupleOperation tuple:
                return tuple.Elements.Aggregate(
                    Cost.Free,
                    (worst, e) => Max(worst, CostOf(e)));

            case IIsTypeOperation isType:
                return CostOf(isType.ValueOperand);

            default:
                return Cost.Computed;
        }
    }

    private static Cost Max(Cost left, Cost right) =>
        left > right ? left : right;
}
