namespace Waystone.Monads.Shouldly.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

public abstract class AssertionCodeFix : CodeFixProvider
{
    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(
        CodeFixContext context)
    {
        var root = await context.Document
           .GetSyntaxRootAsync(context.CancellationToken)
           .ConfigureAwait(false);

        if (root is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            if (!diagnostic.Properties.TryGetValue(
                    MonadAssertion.ReplacementKey,
                    out string? replacement)
             || string.IsNullOrEmpty(replacement))
            {
                continue;
            }

            var assertion = root
               .FindNode(
                    diagnostic.Location.SourceSpan,
                    getInnermostNodeForTie: true)
               .FirstAncestorOrSelf<InvocationExpressionSyntax>();

            if (assertion?.Expression is not MemberAccessExpressionSyntax
                access)
            {
                continue;
            }

            Register(context, diagnostic, assertion, access, replacement!);
        }
    }

    protected abstract void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        InvocationExpressionSyntax assertion,
        MemberAccessExpressionSyntax access,
        string replacement);

    /// <summary>
    /// Replaces one expression with another, keeping the trivia of the expression that
    /// was there.
    /// </summary>
    /// <remarks>
    /// Deliberately carries neither the formatter nor the simplifier annotation, unlike
    /// the equivalent in Waystone.Monads.Analyzers. Both fixes here swap one expression
    /// for another in the same position and introduce no type name that could be
    /// shortened and no line that could need indenting — so the formatter has nothing
    /// to do but reindent the replaced statement to its own canonical depth, which
    /// rewrites code the fix was not asked to touch. Across a suite-wide batch that is
    /// the difference between a reviewable diff and an unreadable one.
    /// </remarks>
    protected static async Task<Document> ReplaceAsync(
        Document document,
        SyntaxNode target,
        SyntaxNode replacement,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
           .ConfigureAwait(false);

        var updated = root!.ReplaceNode(
            target,
            replacement.WithTriviaFrom(target));

        return document.WithSyntaxRoot(updated);
    }
}
