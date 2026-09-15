namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseMapCodeFix : MonadCodeFix
{
    private const string ProjectingMember = "Map";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM2026");

    private protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (MemberInvocationAt(node) is not var (invocation, access)
         || LambdaIn(invocation) is not { } lambda
         || LiftedBy(lambda) is not { } projection)
        {
            return;
        }

        var replacement = invocation
           .WithExpression(access.WithName(Projecting(access.Name)))
           .WithArgumentList(
                invocation.ArgumentList.ReplaceNode(
                    lambda,
                    Projecting(lambda, projection)));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use Map",
                token => ReplaceAsync(
                    context.Document,
                    invocation,
                    replacement,
                    token),
                nameof(UseMapCodeFix)),
            diagnostic);
    }

    /// <remarks>
    /// Lambdas only. An anonymous method has no arrow and no expression body to
    /// rewrite to, so the rule reports one and offers no fix rather than emitting a
    /// shape that does not parse.
    /// </remarks>
    private static LambdaExpressionSyntax? LambdaIn(
        InvocationExpressionSyntax invocation) =>
        invocation.ArgumentList.Arguments
           .Select(argument => argument.Expression)
           .OfType<LambdaExpressionSyntax>()
           .FirstOrDefault();

    /// <remarks>
    /// Reads the syntax rather than taking the operation the analyzer already
    /// found, because a code fix is handed a diagnostic and a document rather than
    /// the analysis that produced it. The shapes have to agree: a lift the fix
    /// cannot find leaves the diagnostic with no fix offered, which is a worse
    /// outcome than the rule staying quiet.
    /// </remarks>
    private static ExpressionSyntax? LiftedBy(LambdaExpressionSyntax lambda) =>
        BodyOf(lambda) is InvocationExpressionSyntax
        {
            ArgumentList.Arguments.Count: 1,
        } lift
            ? lift.ArgumentList.Arguments[0].Expression
            : null;

    private static ExpressionSyntax? BodyOf(LambdaExpressionSyntax lambda) =>
        lambda.ExpressionBody
     ?? (lambda.Block is { Statements.Count: 1 } block
      && block.Statements[0] is ReturnStatementSyntax returned
            ? returned.Expression
            : null);

    /// <remarks>
    /// Keeps whatever type arguments the call carried. <c>Map</c> and
    /// <c>AndThen</c> declare the same type parameters in the same order on every
    /// overload, so an explicit <c>AndThen&lt;int&gt;</c> becomes an explicit
    /// <c>Map&lt;int&gt;</c> rather than losing the annotation the author wrote.
    /// </remarks>
    private static SimpleNameSyntax Projecting(SimpleNameSyntax name) =>
        name is GenericNameSyntax generic
            ? generic.WithIdentifier(
                SyntaxFactory.Identifier(ProjectingMember))
            : SyntaxFactory.IdentifierName(ProjectingMember);

    /// <remarks>
    /// The arrow is given a single trailing space rather than keeping its own. A
    /// block-bodied delegate holds the newline before its brace there, and an
    /// expression body inheriting it lands on a line of its own under an arrow with
    /// nothing after it.
    /// </remarks>
    private static LambdaExpressionSyntax Projecting(
        LambdaExpressionSyntax lambda,
        ExpressionSyntax projection) =>
        (LambdaExpressionSyntax)lambda
           .WithArrowToken(
                lambda.ArrowToken.WithTrailingTrivia(SyntaxFactory.Space))
           .WithBlock(null)
           .WithExpressionBody(projection.WithoutTrivia());
}
