namespace Waystone.Monads.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseLazyVariantCodeFix : MonadCodeFix
{
    private static readonly Dictionary<string, string> LazySiblings =
        new()
        {
            ["And"] = "AndThen",
            ["AndAsync"] = "AndThenAsync",
            ["Or"] = "OrElse",
            ["UnwrapOr"] = "UnwrapOrElse",
            ["UnwrapOrAsync"] = "UnwrapOrElseAsync",
            ["MapOr"] = "MapOrElse",
            ["MapOrAsync"] = "MapOrElseAsync",
            ["OkOr"] = "OkOrElse",
            ["OkOrAsync"] = "OkOrElseAsync",
        };

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM2016");

    private protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (MemberInvocationAt(node) is not var (invocation, access))
        {
            return;
        }

        string name = access.Name.Identifier.ValueText;

        if (!LazySiblings.TryGetValue(name, out var lazy)
         || invocation.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        var arguments = invocation.ArgumentList.Arguments;
        var eager = arguments[0];

        var wrapped = eager.WithExpression(
            Lambda(name, eager.Expression));

        var replacement = invocation
           .WithExpression(
                access.WithName(SyntaxFactory.IdentifierName(lazy)))
           .WithArgumentList(
                invocation.ArgumentList.WithArguments(
                    SyntaxFactory.SeparatedList(
                        new[] { wrapped }.Concat(arguments.Skip(1)))));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use " + lazy + "()",
                token => ReplaceAsync(
                    context.Document,
                    invocation,
                    replacement,
                    token),
                nameof(UseLazyVariantCodeFix)),
            diagnostic);
    }

    /// <remarks>
    /// <c>AndThen</c> is the one that takes the receiver's value, so its
    /// delegate needs a discarded parameter where the others take none.
    /// </remarks>
    private static LambdaExpressionSyntax Lambda(
        string eagerName,
        ExpressionSyntax body) =>
        eagerName is "And" or "AndAsync"
            ? SyntaxFactory.SimpleLambdaExpression(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")),
                body)
            : SyntaxFactory.ParenthesizedLambdaExpression(body);
}
