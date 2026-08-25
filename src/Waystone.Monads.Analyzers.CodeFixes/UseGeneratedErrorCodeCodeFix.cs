namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;

/// <summary>
/// Rewrites a call to one of the obsolete enum error factories as the equivalent
/// call on the members <c>[ErrorCodeCatalog]</c> generates.
/// </summary>
/// <remarks>
/// Registered against <c>CS0618</c> rather than a rule of this library's own. The
/// compiler already reports these call sites once the members carry
/// <c>[Obsolete]</c>, and a rule reporting them again would double-report the way
/// <c>WM1002</c> once did alongside <c>WM2008</c>.
/// <para>
/// Where the enum member is named at the call site the fix produces the catalog
/// member — <c>OrderErrorCatalog.Errors.NotFound(message)</c> — because that reads
/// the baked constant and needs no lookup. Where the value is only known at run
/// time it produces the generated extension instead.
/// </para>
/// <para>
/// Offered only when the argument's enum type carries <c>[ErrorCodeCatalog]</c>,
/// since neither replacement exists otherwise. On any other <c>CS0618</c>,
/// including a consumer's own obsolete API, this provider offers nothing.
/// </para>
/// </remarks>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(UseGeneratedErrorCodeCodeFix))]
[Shared]
public sealed class UseGeneratedErrorCodeCodeFix : MonadCodeFix
{
    private const string ObsoleteMemberUsed = "CS0618";

    private const string ErrorCodeTypeName =
        "Waystone.Monads.Results.Errors.ErrorCode";

    private const string ErrorTypeName = "Waystone.Monads.Results.Errors.Error";

    private const string ResultTypeName = "Waystone.Monads.Results.Result";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(ObsoleteMemberUsed);

    protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (symbols.ErrorCodeCatalogAttribute is null) return;

        if (MemberInvocationAt(node) is not { } target) return;

        if (model.GetSymbolInfo(target.Invocation).Symbol is not IMethodSymbol
            method)
        {
            return;
        }

        SeparatedSyntaxList<ArgumentSyntax> arguments =
            target.Invocation.ArgumentList.Arguments;

        if (arguments.Count == 0) return;

        ExpressionSyntax enumExpression = arguments[0].Expression;

        if (EnumTypeOf(enumExpression, model, symbols) is not { } enumType)
        {
            return;
        }

        string? declaredMember = DeclaredMemberOf(enumExpression, model);

        SyntaxNode? replacement = Replacement(
            method,
            arguments,
            enumExpression,
            enumType,
            declaredMember);

        if (replacement is null) return;

        SyntaxNode replaced = method.ContainingType.ToDisplayString()
                           == ResultTypeName
            ? target.Invocation.ArgumentList
            : target.Invocation;

        string title = method.Name == "FromEnum" && arguments.Count == 1
            ? "Use the generated error code"
            : "Use the generated error factory";

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                token => ReplaceAsync(
                    context.Document,
                    replaced,
                    replacement,
                    token),
                title),
            diagnostic);
    }

    private static SyntaxNode? Replacement(
        IMethodSymbol method,
        SeparatedSyntaxList<ArgumentSyntax> arguments,
        ExpressionSyntax enumExpression,
        INamedTypeSymbol enumType,
        string? declaredMember)
    {
        string containing = method.ContainingType.ToDisplayString();

        if (containing == ErrorCodeTypeName
         && method.Name == "FromEnum"
         && arguments.Count == 1)
        {
            return declaredMember is null
                ? Call(Receiver(enumExpression), "ToErrorCode")
                : CatalogAccess(enumType, "Codes", declaredMember);
        }

        if (containing == ErrorTypeName
         && method.Name == "FromEnum"
         && arguments.Count == 2)
        {
            return ErrorFactory(
                enumExpression,
                enumType,
                declaredMember,
                arguments[1]);
        }

        if (containing == ResultTypeName
         && method.Name == "Err"
         && arguments.Count == 2)
        {
            return SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(
                        ErrorFactory(
                            enumExpression,
                            enumType,
                            declaredMember,
                            arguments[1]))));
        }

        return null;
    }

    private static ExpressionSyntax ErrorFactory(
        ExpressionSyntax enumExpression,
        INamedTypeSymbol enumType,
        string? declaredMember,
        ArgumentSyntax message) =>
        declaredMember is null
            ? Call(Receiver(enumExpression), "ToError", message)
            : SyntaxFactory.InvocationExpression(
                    CatalogAccess(enumType, "Errors", declaredMember))
               .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(message)));

    private static ExpressionSyntax CatalogAccess(
        INamedTypeSymbol enumType,
        string nested,
        string member)
    {
        string @namespace = enumType.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : enumType.ContainingNamespace.ToDisplayString() + ".";

        return SyntaxFactory.ParseExpression(
            $"{@namespace}{enumType.Name}Catalog.{nested}.{member}");
    }

    private static INamedTypeSymbol? EnumTypeOf(
        ExpressionSyntax expression,
        SemanticModel model,
        MonadSymbols symbols) =>
        model.GetTypeInfo(expression).Type is INamedTypeSymbol
            {
                TypeKind: TypeKind.Enum,
            } type
     && type.GetAttributes()
            .Any(
                attribute => SymbolEqualityComparer.Default.Equals(
                    attribute.AttributeClass,
                    symbols.ErrorCodeCatalogAttribute))
            ? type
            : null;

    private static string? DeclaredMemberOf(
        ExpressionSyntax expression,
        SemanticModel model) =>
        model.GetSymbolInfo(expression).Symbol is IFieldSymbol
        {
            IsConst: true,
            ContainingType.TypeKind: TypeKind.Enum,
        } field
            ? field.Name
            : null;

    private static ExpressionSyntax Call(
        ExpressionSyntax receiver,
        string name,
        params ArgumentSyntax[] arguments) =>
        SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    receiver,
                    SyntaxFactory.IdentifierName(name)))
           .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(arguments)));

    private static ExpressionSyntax Receiver(ExpressionSyntax expression) =>
        expression is IdentifierNameSyntax
            or MemberAccessExpressionSyntax
            or InvocationExpressionSyntax
            or ElementAccessExpressionSyntax
            or ParenthesizedExpressionSyntax
            ? expression
            : SyntaxFactory.ParenthesizedExpression(expression);
}
