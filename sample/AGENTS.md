# sample

## Purpose

Every project here compiles against the working tree, not a published package. That
is the whole point: a sample that stops building is a break found before release
rather than by a consumer.

Two groups sit here, and the split is about who the code is for.

- `Waystone.Monads.Docs/*` exists to be *published*. Its code appears on GitBook.
- Everything else exists to be *run* — a reader clones it and starts it. The
  analyzer sample additionally exists to be *wrong*: it carries code that trips the
  rules on purpose, so warnings there are the output, not a defect.

## Published code lives here once

**A fenced C# block on a GitBook page is quoted out of a file here.** It is not
copied. `tools/Waystone.DocSnippets` lifts named `#region` blocks out of these
projects and writes them into the pages, and `pre-push` fails if a page has drifted.

A snippet region looks like this:

```csharp
#region option-unwrap-or
int port = configured.UnwrapOr(8080);
#endregion
```

The rules that are easy to get wrong:

1. The region name is the snippet key and it is **published**. Lower case, digits
   and hyphens only — anything else is treated as an ordinary `#region` and ignored
   silently. Name it `<page-slug>-<what-it-shows>`.
2. A key names exactly one block across the whole tree. A duplicate fails the run.
3. Regions do not nest. A region opening inside an open snippet fails the run.
4. Put the scaffolding — the declarations, the `Console.WriteLine`, the method
   signature — *outside* the region. Only the lines a reader needs go between the
   markers.
5. The tool reads all of `sample/`, so the runnable projects can be quoted too. The
   `PreviousMajor` ones are excluded from the root build, so nothing there should be
   quoted — a break in it would not be caught.

## Converting a page is part of editing it

**Touching a GitBook page that still holds a hand-written C# block means moving that
block here first.** The space is being converted one page at a time as each is next
edited, deliberately rather than in a sweep. Most pages still hold hand-written
copies, and only `guides/configuration.md` is done.

The `diff`, `ini`, `jsonc`, `xml` and shell blocks stay hand-written, and so do the
`upgrading/` pages — their samples are older majors that no longer compile here.

After adding a region, build the project and run:

```
dotnet run --project tools/Waystone.DocSnippets
```

Then read the diff in the documentation checkout. Only the two comment lines and the
fence should be new. Anything else means the sample and the page disagreed; the
compiling sample wins, but look before you accept it.

See [Waystone.Monads.Docs/README.md](Waystone.Monads.Docs/README.md) for the project
layout and [../tools/Waystone.DocSnippets/README.md](../tools/Waystone.DocSnippets/README.md)
for how the documentation checkout is found.
