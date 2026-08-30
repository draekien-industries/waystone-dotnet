# The documentation samples

Every code sample published in the
[`waystone.monads` GitBook space](https://github.com/draekien-industries/docs)
lives here as compiling C#. Build it:

```
dotnet build sample/Waystone.Monads.Docs/Waystone.Monads.Docs.Samples.slnx
```

**Nothing here runs.** Every project is a library with no entry point. The build
succeeding *is* the test, so there is no output to read and no scenario to pick.
That is the difference between this folder and the rest of `sample/`, where each
project is a runnable demonstration you are meant to look at.

Every project is also in `Waystone.Net.slnx`, which is what CI builds. The
`Waystone.Monads.Docs.Samples.slnx` above is a convenience for working on one
page without building the test matrix; it is not the thing that guards the
branch. **A project added here has to go in both**, or it compiles on your
machine and nowhere else.

## Why it exists

`docs/AGENTS.md` in the documentation repository carries the rule: compile
samples rather than reading them. Both samples on the configuration page failed
against 6.x — one used a delegate arity that never existed, the other assigned
internal properties — and neither failure was visible to inspection. A sample
that is subtly wrong reads exactly like one that is right.

`TreatWarningsAsErrors` is on, so an obsolete API in a sample fails the build.
That is the second half of the job: a deprecated call compiles fine and its
warning scrolls past in a log nobody reads.

## The page quotes these files rather than copying them

A block wrapped in a named `#region` is published. `Waystone.DocSnippets` reads
the region and writes it into the matching `<!-- snippet: ... -->` slot on the
page, and `pre-push` fails when the two have drifted apart.

```csharp
#region configuration-the-usual-call
MonadOptions.Configure(options => options.UseFallbackErrorCode("Unknown"));
#endregion
```

So a region name is part of the published page, not a private label: renaming one
breaks the slot that quotes it. Name it `<page>-<what-it-shows>`.

`Guides/Configuration.cs` is the file to copy the shape from. The rest of this
folder is still compiled-only, and moves onto regions as each page is next
touched — see [tools/Waystone.DocSnippets/README.md](../../tools/Waystone.DocSnippets/README.md).

## One project per install list

There is a project per companion package, not one project referencing
everything. That is deliberate, and it is the part most likely to look like
over-engineering.

A page tells a reader which package to install. If every sample compiled in one
project that referenced all nine, a page could quietly depend on a package its
install section never mentions, and the build would stay green. Splitting them
means each project references exactly what its page tells you to install, so a
missing package is a build error rather than a support question.

It already caught one. `companion-packages/fluentvalidation.md` chains
`UseValidationErrorCode` after `UseLogger`, but `UseLogger` ships in
`Waystone.Monads.Extensions.Logging` and the page's install section names only
`Waystone.Monads.FluentValidation`. See the comment in `FluentValidation.cs`.

## One file per page

A file is named for the page it comes from, and says so in a `<summary>` on the
type. When a build breaks, the file name is the page to go and fix.

The core project mirrors the page tree in folders — `StartHere/`, `Guides/`,
`Reference/Option/`, `Reference/Result/` — because that project covers most of
the space and a flat list of thirty files would stop being navigable.

`Reference/StateOverloads.cs` sits at the top of `Reference/` rather than under
either type, because `reference/state-overloads.md` covers both.

## The samples are D&D-themed

Every type and value in here comes from tabletop fantasy — characters, quests,
spells, rituals, dice. The published pages already lean that way (`Grog`,
`Keyleth`, `Thordak`, `Pike`, `Vex'ahlia`, `The Raven Queen`), and this folder
finishes the job.

It is not decoration. `Order`, `UserInput` and `Person` are the shapes every
sample in every library uses, so a reader skims them without reading. A `Quest`
with a `GoldReward`, or a `SpellInput` with a `Range`, forces them to look at
what the sample actually does — and `Range` is a real constraint on a real
spell, which `UserInput.Range` never was.

Keep it consistent when you add a page: reuse the cast rather than inventing a
new one, and pick a domain noun that carries the constraint the sample is
demonstrating.

## What is deliberately not here

**Analyzer rule samples.** Every one is wrong on purpose, so it cannot live in a
project that treats warnings as errors.
[`sample/Waystone.Monads.Analyzers.Sample`](../Waystone.Monads.Analyzers.Sample/README.md)
covers those, and this project does not import the analyzers at all — a
documentation page shows `Unwrap` and `Expect` on purpose, and the `WM1xxx`
rules exist to report exactly that.

**Upgrade guide samples.** They are written against v5 and v6 on purpose. The
break inventory in
[`sample/Waystone.Monads.PreviousMajor.Sample`](../Waystone.Monads.PreviousMajor.Sample/README.md)
is where old API is compiled against the working tree.

**Configuration, hosting and observability.** Those three pages already have
runnable sample projects of their own under `sample/`, and a second copy here
would drift from the first.
