namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseNoneCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM1001", "WM1002", "WM1003");

    private protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (node is not ExpressionSyntax expression)
        {
            return;
        }

        var target = diagnostic.Id is "WM1001"
            ? expression.FirstAncestorOrSelf<InvocationExpressionSyntax>()
            : expression;

        if (target is null)
        {
            return;
        }

        var info = model.GetTypeInfo(target, context.CancellationToken);

        var option = info.Type ?? info.ConvertedType;

        var arguments = symbols.TypeArgumentsOf(option);

        if (!symbols.IsOption(option) || arguments.Length != 1)
        {
            return;
        }

        var value = arguments[0];

        var replacement = FactoryCall(
            symbols.OptionFactory,
            "None",
            ImmutableArray.Create(value),
            model,
            target.SpanStart);

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use Option.None<" + Semantics.Display(value) + ">()",
                token => ReplaceAsync(
                    context.Document,
                    target,
                    replacement,
                    token),
                nameof(UseNoneCodeFix)),
            diagnostic);
    }
}
