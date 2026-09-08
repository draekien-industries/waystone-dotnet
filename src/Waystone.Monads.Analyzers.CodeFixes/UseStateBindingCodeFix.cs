namespace Waystone.Monads.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Operations;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseStateBindingCodeFix : MonadCodeFix
{
    private const string BindMemberName = "With";
    private const string StateParameterName = "state";

    private const string OptionExtensionsNamespace =
        "Waystone.Monads.Options.Extensions";

    private const string ResultExtensionsNamespace =
        "Waystone.Monads.Results.Extensions";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM2017");

    private protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (MemberInvocationAt(node) is not { } target
         || model.GetOperation(target.Invocation, context.CancellationToken)
                is not IInvocationOperation operation
         || symbols.BinderFor(operation.TargetMethod.ContainingType) is null
         || Lambdas(operation) is not { } lambdas)
        {
            return;
        }

        List<ISymbol> captured = CapturedBy(lambdas);
        var version = VersionOf(model);

        if (captured.Count == 0
         || !CanCarryState(captured, version)
         || ShadowsACapture(lambdas, captured)
         || FreshStateName(target.Invocation) is not { } state)
        {
            return;
        }

        var rewritten = Rewrite(
            lambdas,
            captured,
            state,
            version >= LanguageVersion.CSharp9);

        var replacement = target.Invocation
           .WithExpression(
                target.Access.WithExpression(
                    Bind(target.Access.Expression, captured)))
           .WithArgumentList(
                target.Invocation.ArgumentList.ReplaceNodes(
                    rewritten.Keys,
                    (original, _) => rewritten[original]));

        context.RegisterCodeFix(
            CodeAction.Create(
                "Bind the state with 'With'",
                token => ApplyAsync(
                    context.Document,
                    target.Invocation,
                    replacement,
                    ImportFor(operation, symbols),
                    token),
                nameof(UseStateBindingCodeFix)),
            diagnostic);
    }

    private static string? ImportFor(
        IInvocationOperation operation,
        MonadSymbols symbols) =>
        operation.TargetMethod.IsStatic
            ? null
            : symbols.IsOption(operation.TargetMethod.ContainingType)
                ? OptionExtensionsNamespace
                : ResultExtensionsNamespace;

    private static LanguageVersion VersionOf(SemanticModel model) =>
        model.SyntaxTree.Options is CSharpParseOptions options
            ? options.LanguageVersion.MapSpecifiedToEffectiveVersion()
            : LanguageVersion.Default;

    private static bool CanCarryState(
        List<ISymbol> captured,
        LanguageVersion version) =>
        captured.Count == 1
     || (version >= LanguageVersion.CSharp7_1
      && captured.All(symbol => !IsReservedTupleName(symbol.Name)));

    /// <remarks>
    /// Only reached with two or more captures, because a single one is passed as
    /// itself rather than as a tuple member. There is no rewrite for these: an
    /// explicit element name does not help, since <c>Rest</c> is CS8126 at any
    /// position and <c>Item2</c> is CS8125 anywhere but second, and naming the
    /// members anything other than the variables would put invented names in the
    /// consumer's source.
    /// </remarks>
    private static bool IsReservedTupleName(string name) =>
        string.Equals(name, "Rest", StringComparison.Ordinal)
     || (name.Length > 4
      && name.StartsWith("Item", StringComparison.Ordinal)
      && name.Substring(4).All(char.IsDigit));

    private static List<IAnonymousFunctionOperation>? Lambdas(
        IInvocationOperation invocation)
    {
        var lambdas = new List<IAnonymousFunctionOperation>();

        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter?.Type.TypeKind != TypeKind.Delegate)
            {
                continue;
            }

            if (Semantics.LambdaIn(argument.Value) is not
                    { Syntax: LambdaExpressionSyntax syntax } lambda
             || syntax.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return null;
            }

            lambdas.Add(lambda);
        }

        return lambdas.Count == 0 ? null : lambdas;
    }

    private static List<ISymbol> CapturedBy(
        List<IAnonymousFunctionOperation> lambdas)
    {
        var captured = new List<ISymbol>();

        foreach (var lambda in lambdas)
        {
            foreach (var symbol in ReferencedOutside(lambda))
            {
                if (!captured.Contains(symbol, SymbolEqualityComparer.Default))
                {
                    captured.Add(symbol);
                }
            }
        }

        return captured;
    }

    private static IEnumerable<ISymbol> ReferencedOutside(
        IAnonymousFunctionOperation lambda)
    {
        foreach (var descendant in lambda.Descendants())
        {
            if (Semantics.CapturableReference(descendant) is { } referenced
             && !Semantics.IsDeclaredWithin(referenced, lambda.Syntax))
            {
                yield return referenced;
            }
        }
    }

    private static bool ShadowsACapture(
        List<IAnonymousFunctionOperation> lambdas,
        List<ISymbol> captured)
    {
        var names = captured.Select(symbol => symbol.Name)
           .ToImmutableHashSet(StringComparer.Ordinal);

        foreach (var lambda in lambdas)
        {
            if (lambda.Symbol.Parameters.Any(
                    parameter => names.Contains(parameter.Name)))
            {
                return true;
            }

            foreach (var descendant in lambda.Descendants())
            {
                if (descendant is IVariableDeclaratorOperation declarator
                 && names.Contains(declarator.Symbol.Name))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string? FreshStateName(SyntaxNode invocation)
    {
        var scope = invocation.FirstAncestorOrSelf<MemberDeclarationSyntax>()
                 ?? invocation;

        var identifiers = scope.DescendantTokens()
           .Where(token => token.IsKind(SyntaxKind.IdentifierToken))
           .Select(token => token.ValueText);

        if (!identifiers.Contains(StateParameterName, StringComparer.Ordinal))
        {
            return StateParameterName;
        }

        var taken = identifiers.ToImmutableHashSet(StringComparer.Ordinal);

        for (int suffix = 1; suffix <= taken.Count; suffix++)
        {
            string candidate = StateParameterName
                             + suffix.ToString(
                                   System.Globalization.CultureInfo
                                      .InvariantCulture);

            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static ExpressionSyntax Bind(
        ExpressionSyntax receiver,
        List<ISymbol> captured) =>
        SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    receiver.WithoutTrivia(),
                    SyntaxFactory.IdentifierName(BindMemberName)))
           .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(StateArgument(captured)))));

    private static ExpressionSyntax StateArgument(List<ISymbol> captured) =>
        captured.Count == 1
            ? SyntaxFactory.IdentifierName(captured[0].Name)
            : SyntaxFactory.TupleExpression(
                SyntaxFactory.SeparatedList(
                    captured.Select(
                        symbol => SyntaxFactory.Argument(
                            SyntaxFactory.IdentifierName(symbol.Name)))));

    private static Dictionary<SyntaxNode, SyntaxNode> Rewrite(
        List<IAnonymousFunctionOperation> lambdas,
        List<ISymbol> captured,
        string state,
        bool markStatic)
    {
        var rewritten = new Dictionary<SyntaxNode, SyntaxNode>();

        foreach (var lambda in lambdas)
        {
            var syntax = (LambdaExpressionSyntax)lambda.Syntax;
            var references = References(lambda, captured, state);

            var read = references.Count == 0
                ? syntax
                : syntax.ReplaceNodes(
                    references.Keys,
                    (original, _) => references[original]);

            rewritten[syntax] = WithStateParameter(read, state, markStatic);
        }

        return rewritten;
    }

    private static Dictionary<SyntaxNode, SyntaxNode> References(
        IAnonymousFunctionOperation lambda,
        List<ISymbol> captured,
        string state)
    {
        var references = new Dictionary<SyntaxNode, SyntaxNode>();

        foreach (var descendant in lambda.Descendants())
        {
            if (Semantics.CapturableReference(descendant) is not { } referenced
             || descendant.Syntax is not IdentifierNameSyntax name
             || !captured.Contains(referenced, SymbolEqualityComparer.Default))
            {
                continue;
            }

            references[name] = Read(state, captured.Count, referenced.Name)
               .WithTriviaFrom(name);
        }

        return references;
    }

    private static ExpressionSyntax Read(
        string state,
        int captured,
        string name) =>
        captured == 1
            ? SyntaxFactory.IdentifierName(state)
            : SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(state),
                SyntaxFactory.IdentifierName(name));

    private static LambdaExpressionSyntax WithStateParameter(
        LambdaExpressionSyntax lambda,
        string state,
        bool markStatic)
    {
        var parameter =
            SyntaxFactory.Parameter(SyntaxFactory.Identifier(state));

        var parameters = lambda switch
        {
            SimpleLambdaExpressionSyntax simple => SyntaxFactory.SeparatedList(
                new[] { simple.Parameter.WithoutTrivia(), parameter }),
            ParenthesizedLambdaExpressionSyntax parenthesized =>
                parenthesized.ParameterList.AddParameters(parameter).Parameters,
            _ => SyntaxFactory.SingletonSeparatedList(parameter),
        };

        var modifiers = markStatic
                     && !lambda.Modifiers.Any(SyntaxKind.StaticKeyword)
            ? lambda.Modifiers.Insert(
                0,
                SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            : lambda.Modifiers;

        return SyntaxFactory
           .ParenthesizedLambdaExpression(
                SyntaxFactory.ParameterList(parameters),
                lambda.Body)
           .WithModifiers(modifiers)
           .WithTriviaFrom(lambda)
           .WithAdditionalAnnotations(Formatter.Annotation);
    }

    private static async Task<Document> ApplyAsync(
        Document document,
        SyntaxNode target,
        SyntaxNode replacement,
        string? import,
        CancellationToken cancellationToken)
    {
        var replaced = await ReplaceAsync(
                document,
                target,
                replacement,
                cancellationToken)
           .ConfigureAwait(false);

        if (import is null)
        {
            return replaced;
        }

        var root = await replaced.GetSyntaxRootAsync(cancellationToken)
           .ConfigureAwait(false);

        return root is CompilationUnitSyntax unit
            ? replaced.WithSyntaxRoot(Import(unit, import))
            : replaced;
    }

    private static CompilationUnitSyntax Import(
        CompilationUnitSyntax unit,
        string import)
    {
        string newLine = NewLineIn(unit);

        var declaration = unit.DescendantNodes()
           .OfType<BaseNamespaceDeclarationSyntax>()
           .FirstOrDefault();

        if (declaration is null || declaration.Usings.Count == 0)
        {
            return Imports(unit.Usings, import)
                ? unit
                : unit.WithUsings(Append(unit.Usings, import, newLine));
        }

        return Imports(declaration.Usings, import)
            ? unit
            : unit.ReplaceNode(
                declaration,
                declaration.WithUsings(
                    Append(declaration.Usings, import, newLine)));
    }

    /// <remarks>
    /// Read off the document rather than left to the formatter, which is the one
    /// place in this fix that does not defer to
    /// <see cref="Formatter.Annotation" />. An annotated directive is formatted
    /// with <c>Environment.NewLine</c>, so the first attempt at this wrote CRLF
    /// into a document whose every other line ended LF — caught only because the
    /// test then passed on Windows and would have failed on Linux CI.
    /// <para>
    /// The document's own trivia is the right source even where a formatting
    /// option disagrees with it. The option says what a new file should use; this
    /// answers what the file being edited already uses, and matching it is what
    /// keeps a fix from leaving one odd line ending behind. A document with no
    /// end-of-line trivia at all is a single line, so any answer is the first one
    /// and LF is the safer default in a repository that normalises on commit.
    /// </para>
    /// </remarks>
    private static string NewLineIn(SyntaxNode root)
    {
        foreach (var trivia in root.DescendantTrivia())
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return trivia.ToFullString();
            }
        }

        return "\n";
    }

    private static SyntaxList<UsingDirectiveSyntax> Append(
        SyntaxList<UsingDirectiveSyntax> usings,
        string import,
        string newLine)
    {
        var end = SyntaxFactory.EndOfLine(newLine);

        var directive =
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(import));

        if (usings.Count == 0)
        {
            return SyntaxFactory.SingletonList(
                directive.WithTrailingTrivia(end, end));
        }

        var last = usings[usings.Count - 1];

        return usings.Replace(last, last.WithTrailingTrivia(end))
           .Add(directive.WithTrailingTrivia(last.GetTrailingTrivia()));
    }

    private static bool Imports(
        SyntaxList<UsingDirectiveSyntax> usings,
        string import) =>
        usings.Any(
            directive => directive.Alias is null
                      && directive.StaticKeyword.IsKind(SyntaxKind.None)
                      && string.Equals(
                             directive.Name?.ToString(),
                             import,
                             StringComparison.Ordinal));
}
