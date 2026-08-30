namespace Waystone.Monads.Shouldly.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AwaitedAssertionAnalyzer : AssertionAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.ParenthesisedAwaitAssertion);

    private protected override void Register(
        CompilationStartAnalysisContext context,
        AssertionSymbols symbols) =>
        context.RegisterOperationAction(
            operation => Analyze(operation, symbols),
            OperationKind.Invocation);

    private static void Analyze(
        OperationAnalysisContext context,
        AssertionSymbols symbols)
    {
        var assertion = (IInvocationOperation)context.Operation;

        if (assertion.Syntax is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax
                {
                    Expression: ParenthesizedExpressionSyntax
                    {
                        Expression: AwaitExpressionSyntax awaited,
                    },
                } access,
            } syntax
         || assertion.SemanticModel is null
         || !MonadAssertion.IsShouldlyMethod(assertion.TargetMethod))
        {
            return;
        }

        string? replacement =
            MonadAssertion.Awaited(access.Name.Identifier.ValueText);

        if (replacement is null || !IsRewritablePosition(syntax))
        {
            return;
        }

        var task = assertion.SemanticModel
           .GetTypeInfo(awaited.Expression, context.CancellationToken)
           .Type;

        if (symbols.AwaitedMonad(task) is null)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.ParenthesisedAwaitAssertion,
                syntax.GetLocation(),
                ImmutableDictionary<string, string?>.Empty.Add(
                    MonadAssertion.ReplacementKey,
                    replacement),
                access.Name.Identifier.ValueText,
                replacement));
    }

    /// <summary>
    /// Checks whether the awaited assertion can stand where the parenthesised one
    /// stands, without parentheses of its own.
    /// </summary>
    /// <remarks>
    /// A whitelist rather than an exclusion list, because the fix moves the
    /// <c>await</c> outward and every postfix operator binds tighter than it does:
    /// left in a chained member access, <c>await task.ShouldBeSomeAsync().Name</c>
    /// reads the member off the task and does not compile. Enumerating the unsafe
    /// positions would have to be exhaustive to be correct, so the safe two are named
    /// instead and anything else is simply not reported. Widening this means proving
    /// the new position binds looser than <c>await</c>, not just that it looks
    /// harmless.
    /// </remarks>
    private static bool IsRewritablePosition(InvocationExpressionSyntax syntax) =>
        syntax.Parent is ExpressionStatementSyntax or EqualsValueClauseSyntax;
}
