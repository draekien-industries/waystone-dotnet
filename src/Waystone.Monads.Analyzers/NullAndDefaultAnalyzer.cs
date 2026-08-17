namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullAndDefaultAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.NullAssignedToMonad,
            Rules.DefaultOfMonad,
            Rules.DefaultValueConvertsToNone);

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

        context.RegisterOperationAction(
            operation => AnalyzeConversion(operation, symbols),
            OperationKind.Conversion);
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

    private static void AnalyzeConversion(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var conversion = (IConversionOperation)context.Operation;

        if (conversion.OperatorMethod is not { Name: "op_Implicit" } operatorMethod
         || !SymbolEqualityComparer.Default.Equals(
                operatorMethod.ContainingType.OriginalDefinition,
                symbols.Option))
        {
            return;
        }

        var operand = conversion.Operand;

        if (operand.ConstantValue is { HasValue: true, Value: null }
         || !Semantics.IsDefaultValue(operand))
        {
            return;
        }

        var valueType = operatorMethod.ContainingType.TypeArguments[0];

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.DefaultValueConvertsToNone,
                operand.Syntax.GetLocation(),
                valueType.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat),
                operand.Syntax.ToString()));
    }

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
