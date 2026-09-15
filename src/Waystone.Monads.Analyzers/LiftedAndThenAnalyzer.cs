namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LiftedAndThenAnalyzer : MonadAnalyzer
{
    internal const string BindingMember = "AndThen";

    internal const string ProjectingMember = "Map";

    private const string SomeName = "Some";

    private const string OkName = "Ok";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.LiftedAndThen);

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

        if (invocation.TargetMethod.Name != BindingMember
         || !Semantics.IsChainCall(invocation, symbols)
         || LiftedBy(invocation, symbols) is null)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.LiftedAndThen,
                Semantics.NameLocationOf(invocation),
                BindingMember,
                ProjectingMember));
    }

    /// <remarks>
    /// The first delegate argument is the bound step on every overload. The state
    /// overloads take their state before it, and nothing here takes a second
    /// delegate.
    /// </remarks>
    private static IInvocationOperation? LiftedBy(
        IInvocationOperation invocation,
        MonadSymbols symbols)
    {
        foreach (var argument in invocation.Arguments)
        {
            if (Semantics.LambdaIn(argument.Value) is { } lambda)
            {
                return LiftReturnedBy(lambda, symbols);
            }
        }

        return null;
    }

    /// <remarks>
    /// Null for a delegate that does more than return one expression. A body with
    /// statements before the return does work the rewrite would have to carry, and
    /// a delegate that returns from more than one place has a branch whose other
    /// arm may well be a <c>None</c>.
    /// </remarks>
    private static IInvocationOperation? LiftReturnedBy(
        IAnonymousFunctionOperation lambda,
        MonadSymbols symbols)
    {
        if (lambda.Body is not { Operations.Length: 1 } body
         || body.Operations[0] is not IReturnOperation
            {
                ReturnedValue: { } returned,
            })
        {
            return null;
        }

        return Semantics.Unconverted(returned) is IInvocationOperation call
            && IsLift(call.TargetMethod, symbols)
                ? call
                : null;
    }

    /// <remarks>
    /// Keyed on the factory rather than on the returned type, because every
    /// candidate returns the same type. <c>FromNullable</c> and <c>Try</c> are
    /// absent deliberately: both produce the absent case for some inputs, so
    /// <c>Map</c> is not what either of them means.
    /// </remarks>
    private static bool IsLift(IMethodSymbol method, MonadSymbols symbols) =>
        method.Parameters.Length == 1
     && (IsDeclaredBy(method, SomeName, symbols.OptionFactory)
      || IsDeclaredBy(method, OkName, symbols.ResultFactory));

    private static bool IsDeclaredBy(
        IMethodSymbol method,
        string name,
        INamedTypeSymbol factory) =>
        method.Name == name
     && SymbolEqualityComparer.Default.Equals(
            method.ContainingType,
            factory);
}
