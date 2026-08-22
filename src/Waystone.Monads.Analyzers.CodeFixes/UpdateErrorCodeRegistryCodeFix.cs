namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Writes the error code registry the compilation implies.
/// </summary>
/// <remarks>
/// Not a <see cref="MonadCodeFix" />: that base resolves a syntax node and a semantic
/// model for the document the diagnostic sits in, and this fix edits an additional
/// document and needs neither.
/// <para>
/// One invocation writes the whole file, so a run started from any one missing code
/// also takes out every stale entry. That is what makes the absence of a fix-all
/// provider harmless — a fix-all would apply the same rewrite once per diagnostic.
/// </para>
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UpdateErrorCodeRegistryCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("WM2019");

    public override FixAllProvider? GetFixAllProvider() => null;

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        TextDocument? registry = context.Document.Project.AdditionalDocuments
           .FirstOrDefault(document => ErrorCodeRegistry.Matches(document.Name));

        if (registry is null) return Task.CompletedTask;

        DocumentId id = registry.Id;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Update " + ErrorCodeRegistry.FileName,
                token => UpdateAsync(context.Document.Project, id, token),
                nameof(UpdateErrorCodeRegistryCodeFix)),
            context.Diagnostics);

        return Task.CompletedTask;
    }

    private static async Task<Solution> UpdateAsync(
        Project project,
        DocumentId id,
        CancellationToken cancellationToken)
    {
        TextDocument? registry = project.GetAdditionalDocument(id);

        Compilation? compilation =
            await project.GetCompilationAsync(cancellationToken)
                         .ConfigureAwait(false);

        if (registry is null || compilation is null) return project.Solution;

        var symbols = MonadSymbols.TryCreate(compilation);

        if (symbols?.ErrorCodeCatalogAttribute is null) return project.Solution;

        SourceText existing = await registry.GetTextAsync(cancellationToken)
                                            .ConfigureAwait(false);

        return project.Solution.WithAdditionalDocumentText(
            id,
            SourceText.From(
                ErrorCodeRegistry.Render(
                    existing,
                    ErrorCodeCatalogs.CodesIn(compilation, symbols))));
    }
}
