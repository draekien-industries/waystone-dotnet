namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseStateCheckCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM2008");

    private protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        var (subject, testsForNull) = Subject(node);

        if (subject is null)
        {
            return;
        }

        var type = model.GetTypeInfo(subject, context.CancellationToken).Type;

        if (type is null || !symbols.IsMonad(type))
        {
            return;
        }

        string member = symbols.IsOption(type)
            ? testsForNull ? "IsNone" : "IsSome"
            : testsForNull ? "IsErr" : "IsOk";

        var replacement = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            subject.WithoutTrivia(),
            SyntaxFactory.IdentifierName(member));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use " + member,
                token => ReplaceAsync(
                    context.Document,
                    node,
                    replacement,
                    token),
                nameof(UseStateCheckCodeFix)),
            diagnostic);
    }

    private static (ExpressionSyntax? Subject, bool TestsForNull) Subject(
        SyntaxNode node) =>
        node switch
        {
            BinaryExpressionSyntax binary when IsNull(binary.Right) => (
                binary.Left,
                binary.IsKind(SyntaxKind.EqualsExpression)),
            BinaryExpressionSyntax binary when IsNull(binary.Left) => (
                binary.Right,
                binary.IsKind(SyntaxKind.EqualsExpression)),
            IsPatternExpressionSyntax pattern => (
                pattern.Expression,
                pattern.Pattern is ConstantPatternSyntax),
            _ => (null, false),
        };

    private static bool IsNull(ExpressionSyntax expression) =>
        expression.IsKind(SyntaxKind.NullLiteralExpression);
}
