namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionCreationAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.SomeFromDefaultValue,
            Rules.PossiblyNullPassedToSome);

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
        var method = invocation.TargetMethod;

        if (method.Name != "Some"
         || !SymbolEqualityComparer.Default.Equals(
                method.ContainingType,
                symbols.OptionFactory)
         || invocation.Arguments.Length != 1
         || method.TypeArguments.Length != 1)
        {
            return;
        }

        var argument = invocation.Arguments[0].Value;

        var valueType = method.TypeArguments[0];

        if (Semantics.IsDefaultValue(argument))
        {
            if (!valueType.IsValueType)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rules.SomeFromDefaultValue,
                        argument.Syntax.GetLocation(),
                        Semantics.Display(valueType)));
            }

            return;
        }

        if (Semantics.IsMaybeNull(argument))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rules.PossiblyNullPassedToSome,
                    argument.Syntax.GetLocation()));
        }
    }
}
