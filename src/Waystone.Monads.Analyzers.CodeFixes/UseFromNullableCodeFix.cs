namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseFromNullableCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM1005");

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

        var replacement = invocation.WithExpression(
            access.WithName(SyntaxFactory.IdentifierName("FromNullable")));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use Option.FromNullable",
                token => ReplaceAsync(
                    context.Document,
                    invocation,
                    replacement,
                    token),
                nameof(UseFromNullableCodeFix)),
            diagnostic);
    }
}
