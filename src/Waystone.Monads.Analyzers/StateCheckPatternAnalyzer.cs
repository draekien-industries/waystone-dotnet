namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StateCheckPatternAnalyzer : MonadAnalyzer
{
    private static readonly Dictionary<string, string> Combinators =
        new()
        {
            ["IsSome"] = "IsSomeAnd",
            ["IsNone"] = "IsNoneOr",
            ["IsOk"] = "IsOkAnd",
            ["IsErr"] = "IsErrAnd",
        };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.StateCheckedThroughPattern);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols) =>
        context.RegisterOperationAction(
            operation => Analyze(operation, symbols),
            OperationKind.PropertySubpattern);

    private static void Analyze(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var subpattern = (IPropertySubpatternOperation)context.Operation;

        if (subpattern.Member is not IPropertyReferenceOperation member
         || !symbols.IsMonad(member.Property.ContainingType)
         || !Combinators.TryGetValue(
                member.Property.Name,
                out var combinator))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.StateCheckedThroughPattern,
                subpattern.Syntax.GetLocation(),
                member.Property.Name,
                Semantics.Display(member.Property.ContainingType),
                combinator));
    }
}
