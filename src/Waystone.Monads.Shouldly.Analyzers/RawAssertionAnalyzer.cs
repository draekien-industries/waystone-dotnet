namespace Waystone.Monads.Shouldly.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RawAssertionAnalyzer : AssertionAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.RawAssertion);

    protected override void Register(
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
                Expression: MemberAccessExpressionSyntax access,
            } syntax
         || assertion.SemanticModel is null
         || !MonadAssertion.IsShouldlyMethod(assertion.TargetMethod))
        {
            return;
        }

        switch (assertion.TargetMethod.Name)
        {
            case "ShouldBeTrue":
            case "ShouldBeFalse":
                ReportState(context, assertion, syntax, access, symbols);

                break;
            case "ShouldBe":
                ReportValue(context, assertion, syntax, access, symbols);

                break;
        }
    }

    private static void ReportState(
        OperationAnalysisContext context,
        IInvocationOperation assertion,
        InvocationExpressionSyntax syntax,
        MemberAccessExpressionSyntax access,
        AssertionSymbols symbols)
    {
        if (access.Expression is not MemberAccessExpressionSyntax state)
        {
            return;
        }

        string? replacement = MonadAssertion.StateAssertion(
            state.Name.Identifier.ValueText,
            assertion.TargetMethod.Name == "ShouldBeTrue");

        if (replacement is null)
        {
            return;
        }

        var receiver = assertion.SemanticModel!
           .GetTypeInfo(state.Expression, context.CancellationToken)
           .Type;

        if (!symbols.IsMonad(receiver))
        {
            return;
        }

        Report(
            context,
            syntax,
            state.Name.Identifier.ValueText,
            receiver!,
            replacement);
    }

    private static void ReportValue(
        OperationAnalysisContext context,
        IInvocationOperation assertion,
        InvocationExpressionSyntax syntax,
        MemberAccessExpressionSyntax access,
        AssertionSymbols symbols)
    {
        if (access.Expression is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax unwrap,
                ArgumentList.Arguments.Count: 0,
            }
         || !TakesOnlyExpectedAndMessage(assertion.TargetMethod))
        {
            return;
        }

        var receiver = assertion.SemanticModel!
           .GetTypeInfo(unwrap.Expression, context.CancellationToken)
           .Type;

        if (!symbols.IsMonad(receiver))
        {
            return;
        }

        string? replacement = MonadAssertion.ValueAssertion(
            unwrap.Name.Identifier.ValueText,
            symbols.IsOption(receiver));

        if (replacement is null)
        {
            return;
        }

        Report(
            context,
            syntax,
            unwrap.Name.Identifier.ValueText,
            receiver!,
            replacement);
    }

    /// <summary>
    /// Checks whether the comparison takes nothing beyond the expected value and a
    /// custom message, so the arguments written at the call site can be carried onto
    /// the replacement unchanged.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is what keeps the rule off <c>ShouldBe(expected, tolerance)</c> and the
    /// comparer and ignore-order overloads. Their extra arguments describe how to
    /// compare a bare value and have no counterpart on an assertion that takes the
    /// monad, so a fix that dropped or forwarded them positionally would either weaken
    /// the assertion or fail to compile.
    /// </para>
    /// <para>
    /// The state half of the rule needs no equivalent check. <c>ShouldBeTrue</c> and
    /// <c>ShouldBeFalse</c> assert a bool, which has nothing to configure, so the only
    /// argument either takes is the custom message.
    /// </para>
    /// <para>
    /// Counts from the unreduced form, where parameter 0 is the receiver and parameter
    /// 1 the expected value. An extension method reaches an operation action unreduced,
    /// so skipping one would land on <c>expected</c> and reject every overload —
    /// including the one the rule exists for.
    /// </para>
    /// </remarks>
    private static bool TakesOnlyExpectedAndMessage(IMethodSymbol method) =>
        (method.ReducedFrom ?? method).Parameters.Skip(2)
           .All(parameter => parameter.Name == "customMessage");

    private static void Report(
        OperationAnalysisContext context,
        InvocationExpressionSyntax syntax,
        string member,
        ITypeSymbol receiver,
        string replacement) =>
        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.RawAssertion,
                syntax.GetLocation(),
                ImmutableDictionary<string, string?>.Empty.Add(
                    MonadAssertion.ReplacementKey,
                    replacement),
                member,
                receiver.WithNullableAnnotation(NullableAnnotation.None)
                   .ToDisplayString(
                        SymbolDisplayFormat.MinimallyQualifiedFormat),
                replacement));
}
