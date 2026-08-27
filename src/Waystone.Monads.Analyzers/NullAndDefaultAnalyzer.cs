namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullAndDefaultAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.NullAssignedToMonad,
            Rules.DefaultOfMonad);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols)
    {
        context.RegisterSyntaxNodeAction(
            node => AnalyzeNull(node, symbols),
            SyntaxKind.NullLiteralExpression);

        context.RegisterSyntaxNodeAction(
            node => AnalyzeDefault(node, symbols),
            SyntaxKind.DefaultLiteralExpression,
            SyntaxKind.DefaultExpression);
    }

    private static void AnalyzeNull(
        SyntaxNodeAnalysisContext context,
        MonadSymbols symbols)
    {
        var expression = (ExpressionSyntax)context.Node;

        if (expression.Parent is PostfixUnaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.SuppressNullableWarningExpression,
            } suppression)
        {
            expression = suppression;
        }

        var type = context.SemanticModel
           .GetTypeInfo(expression, context.CancellationToken)
           .ConvertedType;

        if (!symbols.IsMonad(type)
         || IsNullTest(expression)
         || TargetIsExplicitlyNullable(context, expression))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.NullAssignedToMonad,
                expression.GetLocation(),
                Semantics.Display(type!)));
    }

    private static void AnalyzeDefault(
        SyntaxNodeAnalysisContext context,
        MonadSymbols symbols)
    {
        var expression = (ExpressionSyntax)context.Node;

        var info = context.SemanticModel.GetTypeInfo(
            expression,
            context.CancellationToken);

        var type = info.Type ?? info.ConvertedType;

        if (!symbols.IsMonad(type)
         || IsNullTest(expression)
         || TargetIsExplicitlyNullable(context, expression))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.DefaultOfMonad,
                expression.GetLocation(),
                Semantics.Display(type!)));
    }

    private static bool IsNullTest(ExpressionSyntax expression) =>
        expression.Parent is ConstantPatternSyntax
            or BinaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.EqualsExpression
                    or (int)SyntaxKind.NotEqualsExpression,
            };

    private static bool TargetIsExplicitlyNullable(
        SyntaxNodeAnalysisContext context,
        SyntaxNode node)
    {
        for (SyntaxNode? current = node;
             current is not null;
             current = current.Parent)
        {
            switch (current)
            {
                case ArgumentSyntax argument:
                    return ParameterOf(context, argument)?.Type
                        .NullableAnnotation is NullableAnnotation.Annotated;
                case ArrowExpressionClauseSyntax arrow:
                    return DeclaredTypeOf(arrow.Parent) is NullableTypeSyntax;
                case ReturnStatementSyntax:
                    return DeclaredTypeOf(
                            current
                               .FirstAncestorOrSelf<MemberDeclarationSyntax>())
                        is NullableTypeSyntax;
                case EqualsValueClauseSyntax equals:
                    return DeclaredTypeOf(equals.Parent) is NullableTypeSyntax;
                case AnonymousFunctionExpressionSyntax:
                case MemberDeclarationSyntax:
                    return false;
            }
        }

        return false;
    }

    private static IParameterSymbol? ParameterOf(
        SyntaxNodeAnalysisContext context,
        ArgumentSyntax argument) =>
        (context.SemanticModel.GetOperation(argument, context.CancellationToken)
            as IArgumentOperation)?.Parameter;

    private static TypeSyntax? DeclaredTypeOf(SyntaxNode? declaration) =>
        declaration switch
        {
            MethodDeclarationSyntax method => method.ReturnType,
            PropertyDeclarationSyntax property => property.Type,
            ParameterSyntax parameter => parameter.Type,
            VariableDeclaratorSyntax
            {
                Parent: VariableDeclarationSyntax variable,
            } => variable.Type,
            _ => null,
        };
}
