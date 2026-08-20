namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StateOverloadAnalyzer : MonadAnalyzer
{
    private const string StateTypeParameterName = "TState";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.DelegateCapturesInsteadOfState);

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
        var method = invocation.TargetMethod;

        if (!IsDeclaredByTheLibrary(method, symbols)
         || TakesState(method)
         || !HasAStateOverload(method))
        {
            return;
        }

        List<string> captured = CapturedBy(invocation);

        if (captured.Count == 0)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.DelegateCapturesInsteadOfState,
                Semantics.NameLocationOf(invocation),
                method.Name,
                string.Join("', '", captured)));
    }

    private static bool IsDeclaredByTheLibrary(
        IMethodSymbol method,
        MonadSymbols symbols) =>
        symbols.IsMonad(method.ContainingType)
     || symbols.IsDerivedCase(method.ContainingType)
     || SymbolEqualityComparer.Default.Equals(
            method.ContainingType,
            symbols.OptionFactory)
     || SymbolEqualityComparer.Default.Equals(
            method.ContainingType,
            symbols.ResultFactory);

    private static bool TakesState(IMethodSymbol method) =>
        method.OriginalDefinition.TypeParameters.Any(
            parameter => parameter.Name == StateTypeParameterName);

    private static bool HasAStateOverload(IMethodSymbol method) =>
        method.ContainingType.GetMembers(method.Name)
              .OfType<IMethodSymbol>()
              .Any(TakesState);

    private static List<string> CapturedBy(IInvocationOperation invocation)
    {
        var captured = new List<string>();

        foreach (var argument in invocation.Arguments)
        {
            if (LambdaIn(argument.Value) is not { } lambda)
            {
                continue;
            }

            foreach (var name in CapturedBy(lambda))
            {
                if (!captured.Contains(name))
                {
                    captured.Add(name);
                }
            }
        }

        return captured;
    }

    private static IAnonymousFunctionOperation? LambdaIn(IOperation operation)
    {
        var value = Semantics.Unconverted(operation);

        if (value is IDelegateCreationOperation creation)
        {
            value = Semantics.Unconverted(creation.Target);
        }

        return value as IAnonymousFunctionOperation;
    }

    private static IEnumerable<string> CapturedBy(
        IAnonymousFunctionOperation lambda)
    {
        foreach (var descendant in lambda.Descendants())
        {
            ISymbol? referenced = descendant switch
            {
                ILocalReferenceOperation local => local.Local,
                IParameterReferenceOperation parameter => parameter.Parameter,
                _ => null,
            };

            if (referenced is not null
             && !IsDeclaredWithin(referenced, lambda.Syntax))
            {
                yield return referenced.Name;
            }
        }
    }

    private static bool IsDeclaredWithin(ISymbol symbol, SyntaxNode lambda) =>
        symbol.DeclaringSyntaxReferences.Any(
            reference => reference.SyntaxTree == lambda.SyntaxTree
                      && lambda.Span.Contains(reference.Span));
}
