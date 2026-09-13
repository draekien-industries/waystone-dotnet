namespace Waystone.Internal.Analyzers.AwaitedReceivers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Enforces that a member the awaited receivers generator lifts carries no parameter
/// attribute the generated shape would lose.
/// </summary>
/// <remarks>
/// The generated shapes are analysed as ordinary source here, since they are
/// deliberately not marked as generated code — but they sit on an awaited receiver and
/// are never lifted themselves, so this rule passes over them.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UncarriedParameterAttributeAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rules.UncarriedParameterAttribute);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(
            static start =>
            {
                if (!AwaitedReceiverContract.IsInUse(start.Compilation)) return;

                start.RegisterSymbolAction(Analyse, SymbolKind.NamedType);
            });
    }

    private static void Analyse(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (!AwaitedReceiverContract.IsMarked(type)) return;

        foreach (IMethodSymbol method in AwaitedReceiverContract.Lifted(type))
        {
            Report(context, method);
        }
    }

    private static void Report(SymbolAnalysisContext context, IMethodSymbol method)
    {
        foreach (IParameterSymbol parameter in method.Parameters)
        {
            foreach (AttributeData attribute in parameter.GetAttributes())
            {
                // An attribute the compiler could not bind is already a CS0246 on
                // this very line, and it is not going to reach a generated member
                // either way.
                if (attribute.AttributeClass is not { TypeKind: not TypeKind.Error }
                        attributeClass
                 || AwaitedReceiverContract.Carries(attribute))
                {
                    continue;
                }

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rules.UncarriedParameterAttribute,
                        parameter.Locations[0],
                        attributeClass.Name,
                        parameter.Name,
                        method.Name));
            }
        }
    }
}
