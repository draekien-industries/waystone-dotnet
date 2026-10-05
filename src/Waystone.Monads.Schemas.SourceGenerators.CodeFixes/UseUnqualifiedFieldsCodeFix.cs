namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>Rewrites <c>Schema.Fields(...)</c> as <c>Fields(...)</c>.</summary>
/// <remarks>
/// Deletes the receiver and nothing else. The generator emits the unqualified
/// method once it sees the rewritten call, so the fix adds no declaration of its
/// own.
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseUnqualifiedFieldsCodeFix : CodeFixProvider
{
    private const string Title = "Call Fields without the Schema qualifier";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.PreferUnqualifiedFields.Id);

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = (await context.Document
                                        .GetSyntaxRootAsync(context.CancellationToken)
                                        .ConfigureAwait(false))!;

        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            MemberAccessExpressionSyntax access =
                root.FindNode(diagnostic.Location.SourceSpan)
                    .FirstAncestorOrSelf<MemberAccessExpressionSyntax>()!;

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    _ => Task.FromResult(
                        context.Document.WithSyntaxRoot(
                            root.ReplaceNode(
                                access,
                                access.Name.WithTriviaFrom(access)))),
                    nameof(UseUnqualifiedFieldsCodeFix)),
                diagnostic);
        }
    }
}
