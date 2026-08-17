namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowAnalyzer : MonadAnalyzer
{
    private static readonly ImmutableHashSet<string> ContractExceptions =
        ImmutableHashSet.Create(
            "System.ArgumentException",
            "System.NotImplementedException",
            "System.NotSupportedException",
            "System.ObjectDisposedException");

    private static readonly ImmutableHashSet<string> FactoryNames =
        ImmutableHashSet.Create("Try", "TryAsync");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.ThrowInResultMember,
            Rules.ThrowCouldBeResult);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols) =>
        context.RegisterOperationAction(
            operation => Analyze(operation, symbols),
            OperationKind.Throw);

    private static void Analyze(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var thrown = (IThrowOperation)context.Operation;

        if (thrown.Exception is null
         || IsContractException(
                Semantics.Unconverted(thrown.Exception).Type)
         || IsInsideTryFactory(thrown, symbols))
        {
            return;
        }

        var member = EnclosingMember(thrown, context.ContainingSymbol);

        if (member is null)
        {
            return;
        }

        var returned = symbols.UnwrapAwaitable(member.ReturnType);

        if (symbols.IsResult(returned))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rules.ThrowInResultMember,
                    thrown.Syntax.GetLocation(),
                    Semantics.Display(returned!)));

            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.ThrowCouldBeResult,
                thrown.Syntax.GetLocation(),
                member.Name));
    }

    private static bool IsContractException(ITypeSymbol? type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (ContractExceptions.Contains(current.ToDisplayString()))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInsideTryFactory(
        IOperation operation,
        MonadSymbols symbols)
    {
        for (var current = operation.Parent;
             current is not null;
             current = current.Parent)
        {
            if (current is not IInvocationOperation invocation)
            {
                continue;
            }

            var method = invocation.TargetMethod;

            if (FactoryNames.Contains(method.Name)
             && (SymbolEqualityComparer.Default.Equals(
                     method.ContainingType,
                     symbols.OptionFactory)
              || SymbolEqualityComparer.Default.Equals(
                     method.ContainingType,
                     symbols.ResultFactory)))
            {
                return true;
            }
        }

        return false;
    }

    private static IMethodSymbol? EnclosingMember(
        IOperation operation,
        ISymbol containing)
    {
        for (var current = operation.Parent;
             current is not null;
             current = current.Parent)
        {
            switch (current)
            {
                case IAnonymousFunctionOperation function:
                    return function.Symbol;
                case ILocalFunctionOperation local:
                    return local.Symbol;
            }
        }

        return containing as IMethodSymbol;
    }
}
