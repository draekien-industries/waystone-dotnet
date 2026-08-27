namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseAndThenWithFromNullableCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("CS8714");

    protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (MemberInvocationAt(node) is not var (invocation, access)
         || access.Name is not IdentifierNameSyntax
            {
                Identifier.ValueText: "Map",
            })
        {
            return;
        }

        var arguments = invocation.ArgumentList.Arguments;

        if (arguments.Count is not 1 and not 2)
        {
            return;
        }

        var projection = arguments[arguments.Count - 1];

        if (projection.Expression is not AnonymousFunctionExpressionSyntax
            {
                ExpressionBody: { } projected,
            } lambda)
        {
            return;
        }

        if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method
         || !symbols.IsOption(method.ContainingType)
         || method.TypeArguments.Length is not 1 and not 2
         || method.TypeArguments[method.TypeArguments.Length - 1] is
                ITypeParameterSymbol)
        {
            return;
        }

        var replacement = invocation
           .WithExpression(
                access.WithName(SyntaxFactory.IdentifierName("AndThen")))
           .WithArgumentList(
                invocation.ArgumentList.WithArguments(
                    arguments.Replace(
                        projection,
                        projection.WithExpression(
                            lambda.WithExpressionBody(
                                FactoryCall(
                                    symbols.OptionFactory,
                                    "FromNullable",
                                    ImmutableArray<ITypeSymbol>.Empty,
                                    model,
                                    access.SpanStart,
                                    projected))))));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use AndThen with Option.FromNullable",
                token => ReplaceAsync(
                    context.Document,
                    invocation,
                    replacement,
                    token),
                nameof(UseAndThenWithFromNullableCodeFix)),
            diagnostic);
    }
}
