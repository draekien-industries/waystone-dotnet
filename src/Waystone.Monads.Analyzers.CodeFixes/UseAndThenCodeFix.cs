namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseAndThenCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM2005", "WM2014");

    protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (diagnostic.Id == "WM2014")
        {
            RegisterRename(context, diagnostic, node);

            return;
        }

        RegisterCollapse(context, diagnostic, node);
    }

    private static void RegisterRename(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node)
    {
        if (MemberInvocationAt(node) is not { } target)
        {
            return;
        }

        string replacement = target.Access.Name.Identifier.ValueText switch
        {
            "FlatMap" => "AndThen",
            "FlatMapAsync" => "AndThenAsync",
            _ => string.Empty,
        };

        if (replacement.Length == 0)
        {
            return;
        }

        var renamed = target.Access.WithName(
            Renamed(target.Access.Name, replacement));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use " + replacement,
                token => ReplaceAsync(
                    context.Document,
                    target.Access,
                    renamed,
                    token),
                "RenameFlatMapToAndThen"),
            diagnostic);
    }

    private static void RegisterCollapse(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node)
    {
        if (node.FirstAncestorOrSelf<InvocationExpressionSyntax>() is not
            {
                Expression: MemberAccessExpressionSyntax
                {
                    Expression: InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax map,
                    } mapped,
                } flatten,
            })
        {
            return;
        }

        if (flatten.Name.Identifier.ValueText != "Flatten"
         || map.Name.Identifier.ValueText != "Map")
        {
            return;
        }

        var replacement = mapped.WithExpression(
            map.WithName(SyntaxFactory.IdentifierName("AndThen")));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use AndThen",
                token => ReplaceAsync(
                    context.Document,
                    node.FirstAncestorOrSelf<InvocationExpressionSyntax>()!,
                    replacement,
                    token),
                nameof(UseAndThenCodeFix)),
            diagnostic);
    }

    private static SimpleNameSyntax Renamed(
        SimpleNameSyntax name,
        string replacement) =>
        name is GenericNameSyntax generic
            ? generic.WithIdentifier(SyntaxFactory.Identifier(replacement))
            : SyntaxFactory.IdentifierName(replacement);
}
