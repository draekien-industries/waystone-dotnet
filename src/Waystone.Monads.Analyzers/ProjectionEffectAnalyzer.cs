namespace Waystone.Monads.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProjectionEffectAnalyzer : MonadAnalyzer
{
    /// <remarks>
    /// The members <c>ICollection&lt;T&gt;</c> and the interfaces built on it
    /// declare for changing their contents. A name alone decides nothing — the
    /// declaring type has to be a collection as well — because <c>Add</c> is as
    /// good a name for a pure arithmetic helper as for a mutation.
    /// </remarks>
    private static readonly ImmutableHashSet<string> MutatingMembers =
        ImmutableHashSet.Create(
            "Add",
            "AddRange",
            "Clear",
            "Insert",
            "Remove",
            "RemoveAll",
            "RemoveAt");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.EffectInsideProjection);

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

        if (!Semantics.IsChainCall(invocation, symbols))
        {
            return;
        }

        List<string> mutated = MutatedBy(invocation);

        if (mutated.Count == 0)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.EffectInsideProjection,
                Semantics.NameLocationOf(invocation),
                invocation.TargetMethod.Name,
                string.Join("', '", mutated)));
    }

    /// <remarks>
    /// A delegate returning <see langword="void" /> is skipped, which is what
    /// leaves <c>Inspect</c>, <c>InspectErr</c> and the <c>Action</c> overloads of
    /// <c>Match</c> alone: they exist to run an effect, and their delegates are the
    /// only ones here that return nothing.
    /// </remarks>
    private static List<string> MutatedBy(IInvocationOperation invocation)
    {
        var mutated = new List<string>();

        foreach (var argument in invocation.Arguments)
        {
            if (Semantics.LambdaIn(argument.Value) is not { } lambda
             || lambda.Symbol.ReturnsVoid)
            {
                continue;
            }

            foreach (string name in MutatedBy(lambda))
            {
                if (!mutated.Contains(name))
                {
                    mutated.Add(name);
                }
            }
        }

        return mutated;
    }

    private static IEnumerable<string> MutatedBy(
        IAnonymousFunctionOperation lambda)
    {
        foreach (var descendant in lambda.Descendants())
        {
            if (TargetOf(descendant) is not { } target
             || !IsOutsideState(RootOf(target), lambda))
            {
                continue;
            }

            if (Semantics.ReferencedSymbol(NameableIn(target)) is { } mutated)
            {
                yield return mutated.Name;
            }
        }
    }

    private static IOperation? TargetOf(IOperation operation) =>
        operation switch
        {
            ISimpleAssignmentOperation assignment => assignment.Target,
            ICompoundAssignmentOperation compound => compound.Target,
            ICoalesceAssignmentOperation coalesce => coalesce.Target,
            IIncrementOrDecrementOperation change => change.Target,
            IInvocationOperation call when IsCollectionMutation(call) =>
                Semantics.ReceiverOf(call),
            _ => null,
        };

    private static bool IsCollectionMutation(IInvocationOperation call) =>
        MutatingMembers.Contains(call.TargetMethod.Name)
     && IsCollection(call.TargetMethod.ContainingType);

    private static bool IsCollection(INamedTypeSymbol type) =>
        type.OriginalDefinition.SpecialType
     == SpecialType.System_Collections_Generic_ICollection_T
     || type.AllInterfaces.Any(
            declared => declared.OriginalDefinition.SpecialType
             == SpecialType.System_Collections_Generic_ICollection_T);

    /// <remarks>
    /// An element carries no symbol of its own, so the array holding it is what the
    /// message can name. Without this the rule finds the mutation, finds nothing to
    /// call it, and reports nothing at all.
    /// </remarks>
    private static IOperation NameableIn(IOperation target)
    {
        var current = Semantics.Unconverted(target);

        while (current is IArrayElementReferenceOperation array)
        {
            current = Semantics.Unconverted(array.ArrayReference);
        }

        return current;
    }

    /// <remarks>
    /// Walks to the reference the mutation reaches through, so that
    /// <c>state.Total += 1</c> is judged by <c>state</c> rather than by
    /// <c>Total</c>. That is what keeps the delegate's own parameter out of scope:
    /// its root is the parameter, which is declared inside the lambda.
    /// </remarks>
    private static IOperation RootOf(IOperation target)
    {
        var current = Semantics.Unconverted(target);

        while (true)
        {
            var instance = current switch
            {
                IMemberReferenceOperation member => member.Instance,
                IArrayElementReferenceOperation array => array.ArrayReference,
                _ => null,
            };

            if (instance is null)
            {
                return current;
            }

            current = Semantics.Unconverted(instance);
        }
    }

    private static bool IsOutsideState(
        IOperation root,
        IAnonymousFunctionOperation lambda) =>
        root switch
        {
            ILocalReferenceOperation local => !Semantics.IsDeclaredWithin(
                local.Local,
                lambda.Syntax),
            IParameterReferenceOperation parameter =>
                !Semantics.IsDeclaredWithin(parameter.Parameter, lambda.Syntax),
            IInstanceReferenceOperation => true,
            IFieldReferenceOperation => true,
            IPropertyReferenceOperation => true,
            _ => false,
        };
}
