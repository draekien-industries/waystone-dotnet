# The previous-major sample

Code written against **v6** and compiled against the working tree. Its build output
is the v7 break inventory: run `capture-breaks.ps1` and read `breaks.txt`.

Nothing here is a demonstration. Every member exists because some v7 issue claims
a break, or claims there is none, and a claim about what the compiler does is
cheaper to settle by compiling than to argue.

## Running it

```
pwsh sample/Waystone.Monads.PreviousMajor.Sample/capture-breaks.ps1
```

It builds both projects, normalises the diagnostics to repo-relative paths, and
rewrites `breaks.txt` as a count-by-code table and a row per diagnostic. It exits
`0` even when the build fails — a failing build is the measurement.

**Commit `breaks.txt` on the layer that changed the surface.** The diff is then
the attribution: this break arrived with this change. Rebuilt only at the end of
the stack, it proves the final state and attributes nothing.

**A missed re-capture is worse than no file, because the rows stay plausible.**
The layer collapsing the per-family extension classes did not re-run this script,
and the inventory went on naming `AndThenExtensions` and `IsSomeAndExtensions` for
a further nineteen layers — five rows attributed to `CS0411` and `CS1739` that had
become `CS0103` and `CS0234`, all of them still reading like real measurements.
[DRA-126](https://linear.app/draekien-industries/issue/DRA-126) scoped a fixer
against `CS0246` off the back of it, which is a diagnostic this break never emits.
Re-run the script whenever a type or member leaves the public surface, and read the
count table rather than trusting that the row you care about is current.

## Why it is not in the build

The project is listed in `Waystone.Net.slnx` with `<Build Project="false" />`, so
`dotnet build` at the root skips it while an IDE still loads it and builds it on
demand. That covers CI, which builds the same solution.

If you re-add it with `dotnet sln add`, put that element back — the command does
not, and the first breaking layer would then fail every workflow in the
repository.

`TreatWarningsAsErrors` lives in `src/Directory.Build.props` and there is no
`Directory.Build.props` at the repository root, so nothing outside `src/**`
inherits it. The `TreatWarningsAsErrors=false` in both csproj files is belt and
braces, not a workaround.

## Why there are two projects

**A declaration-phase error masks every body-phase error in the same
compilation.** Measured: a bad `using static` reports its own `CS0234` and the
`CS0029` in a method body two files away is never reported at all. The compiler
does not get as far as binding bodies.

That matters because two v7 changes break declarations rather than call sites —
[DRA-111](https://linear.app/draekien-industries/issue/DRA-111) deletes a type a
`using static` names, and
[DRA-129](https://linear.app/draekien-industries/issue/DRA-129) removes a virtual
a consumer overrides. Left in one project, either one would silently reduce the
whole inventory to a single row.

So `Waystone.Monads.PreviousMajor.Declarations.Sample` holds exactly the
declaration-phase call sites and nothing else, and this project holds the rest.
Neither references the other, so a break in one cannot suppress the other.

**Anything added here that breaks at declaration phase belongs in the other
project.** A `using` directive, a base type, a member signature, an `override`.

## What each file measures

| File | Issue | The claim it settles |
| --- | --- | --- |
| `Chains.cs` | [DRA-115](https://linear.app/draekien-industries/issue/DRA-115) | Whether the delegate return-type change breaks a caller who named `Task`-returning steps as method groups. `BillAsyncLambda` is the control: a lambda converts rather than matches, so it should not move. |
| `Conversions.cs` | [DRA-119](https://linear.app/draekien-industries/issue/DRA-119) | The implicit conversions, in return and argument position, which bind by different rules. |
| `NamedArguments.cs` | [DRA-110](https://linear.app/draekien-industries/issue/DRA-110) | Named arguments are what make a parameter rename a break. The calls on the core members are the control; the calls on the extensions are the inventory. |
| `StaticForm.cs` | [DRA-111](https://linear.app/draekien-industries/issue/DRA-111) | Qualified calls that name an extension class rather than reducing to a receiver. |
| `RunTimeErrorCodes.cs` | [DRA-129](https://linear.app/draekien-industries/issue/DRA-129) | The obsoleted members, called deliberately. `NoWarn=CS0618` keeps the floor build quiet while they wait to break. |
| `Configuration.cs` | [DRA-123](https://linear.app/draekien-industries/issue/DRA-123), [DRA-129](https://linear.app/draekien-industries/issue/DRA-129) | Startup configuration as it is written today, and the only caller of `UseExceptionLogger`. |
| `ConsumerExtensions.cs` | [DRA-120](https://linear.app/draekien-industries/issue/DRA-120), [DRA-121](https://linear.app/draekien-industries/issue/DRA-121) | **Both issues carry no break inventory.** Adding an extension method is non-breaking only while nobody already has that name in scope, and `Select` and `Where` are the two most likely to be there. This file declares a consumer's own and calls them, so the collision count stops being zero by assumption. |
| `../Waystone.Monads.PreviousMajor.Declarations.Sample/UsingStaticExtension.cs` | [DRA-111](https://linear.app/draekien-industries/issue/DRA-111) | A load-bearing `using static` on an extension class — the file imports no extension namespace, so the call below binds only through it. There are zero `using static` occurrences elsewhere in this repository, which is why a deliberate one is here. |
| `../Waystone.Monads.PreviousMajor.Declarations.Sample/ErrorCodeFactoryOverride.cs` | [DRA-129](https://linear.app/draekien-industries/issue/DRA-129) | A consumer subclass overriding the removed virtual, which breaks at the `override` keyword rather than at a call site. |

## What it cannot measure

`DRA-125` changes what `MonadOptionsScope` does when disposed out of order. That
is a behaviour change with no diagnostic, so no sample can count it and the
migration guide has to say so in prose.

`DRA-122` ships editorconfig presets and `DRA-81` adds a package; neither touches
an existing surface.

## Not this project

`sample/Waystone.Monads.Analyzers.Sample` pins current behaviour as compiling
code and is expected to build. It has a different job. `Chains.cs` there is the
positive case; `Chains.cs` here is the same idioms written to be broken.
