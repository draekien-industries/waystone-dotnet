namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseUnwrapOrDefaultCodeFix : MonadCodeFix
{
    private static readonly ImmutableHashSet<string> Fixable =
        ImmutableHashSet.Create(
            "Unwrap",
            "UnwrapAsync",
            "Expect",
            "ExpectAsync",
            "UnwrapOr",
            "UnwrapOrAsync");

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM2001", "WM2002", "WM2007");

    protected override void Register(
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

        if (!Fixable.Contains(name))
        {
            return;
        }

        string replacementName = name.EndsWith("Async")
            ? "UnwrapOrDefaultAsync"
            : "UnwrapOrDefault";

        var replacement = invocation
           .WithExpression(
                access.WithName(
                    SyntaxFactory.IdentifierName(replacementName)))
           .WithArgumentList(SyntaxFactory.ArgumentList());

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use " + replacementName + "()",
                token => ReplaceAsync(
                    context.Document,
                    invocation,
                    replacement,
                    token),
                nameof(UseUnwrapOrDefaultCodeFix)),
            diagnostic);
    }
}
