namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionCreationAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.SomeFromDefaultValue,
            Rules.DefaultOfValueTypeInOption,
            Rules.PossiblyNullPassedToSome);

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
            string type = Semantics.Display(valueType);

            context.ReportDiagnostic(
                valueType.IsValueType
                    ? Diagnostic.Create(
                        Rules.DefaultOfValueTypeInOption,
                        argument.Syntax.GetLocation(),
                        argument.Syntax.ToString(),
                        type)
                    : Diagnostic.Create(
                        Rules.SomeFromDefaultValue,
                        argument.Syntax.GetLocation(),
                        type));

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
