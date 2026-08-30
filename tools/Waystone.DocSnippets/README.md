# Waystone.DocSnippets

Fills the code blocks in the published documentation from the sample projects
that compile them, so a sample exists once rather than twice.

```
dotnet run --project tools/Waystone.DocSnippets            # write
dotnet run --project tools/Waystone.DocSnippets -- --check # report, write nothing
```

`pre-push` runs the second form. A page that no longer matches the sample it
quotes fails the push.

## Adding a snippet

Two edits, one in each repository.

Wrap the code in a named region in the sample project. Every `.cs` file under
`sample/` is scanned, so the runnable samples can be quoted as well as the
published ones. The name is lower case, digits and hyphens — anything else is
treated as an ordinary region and skipped without a word:

```csharp
#region configuration-the-usual-call
MonadOptions.Configure(options => options
    .UseFallbackErrorCode("Unknown"));
#endregion
```

Then put the slot on the page, in place of the fenced block it replaces:

```
<!-- snippet: configuration-the-usual-call -->
<!-- endSnippet -->
```

Run the tool, and the slot fills with the region, dedented, plus a comment
naming the file it came from. Everything between the two markers is generated:
edit the region, never the page.

A region no page uses is reported and allowed — writing one ahead of the page
that will quote it is a normal half-step. A slot naming a region nothing defines
is an error, because the page is already published and shows nothing.

A slot inside a fenced code block is left alone, so a page can show the markers
without the tool treating the example as a real slot. This one does it above.

## Where it expects the other repository

The pages are in [draekien-industries/docs](https://github.com/draekien-industries/docs),
which is a separate checkout. Five candidates are tried in order, and the first
that actually holds a `waystone.monads` directory wins:

1. `--docs <path>`
2. `$WAYSTONE_DOCS_PATH`
3. `git config waystone.docs-path`
4. A directory named `docs` beside this checkout
5. Any sibling of this checkout that looks like it

Finding none, it exits `3` rather than `1`, and the hook reads that as "nothing
to check against" and lets the push through.

`--repo <path>` names the checkout holding the samples, for when the tool is run
from somewhere else. The documentation repository's own `pre-push` passes it.

## Why not MarkdownSnippets

[MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets) does this job
and does it well, and it was the first choice.

It takes **one** directory and scans it for both the source and the markdown.
There is no separate source root. The samples cannot move into the documentation
repository either: every sample project takes the library by `ProjectReference`,
so it compiles against the working tree, which is the whole reason the samples
catch a break before it ships.

Bridging the two would have meant copying the samples into a gitignored folder
inside the documentation repository on every run — reintroducing the copy this
was written to remove, ephemeral or not. Reading two roots directly is less
machinery than that.

See
[docs/explorations/2026-08-31-single-sourcing-code-samples.md](https://github.com/draekien-industries/docs)
in that repository for the full comparison.

## It is written against Waystone.Monads

Deliberately. The sample projects prove a page's five lines compile; this proves
the same API is pleasant to hold across a whole program. Nothing here throws for
a failure it expects — `Locator`, `SnippetReader`, `SnippetInjector` and `Runner`
all return `Result<T, Error>`, `Option<T>` carries every value that may be
absent, and the error codes come from an `[ErrorCodeCatalog]` enum rather than
from strings written at the point they are raised.

That is also why `DocSnippetError.UnterminatedRegion` appears in the output when
something is wrong: it is the generated code, printed as it is.
