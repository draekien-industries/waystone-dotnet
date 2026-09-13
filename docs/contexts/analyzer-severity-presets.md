# The shipped severity presets

Read before adding a `WM` rule, and before editing anything under
`src/Waystone.Monads/build/`.

`src/Waystone.Monads/build/` carries `recommended.globalconfig` and
`strict.globalconfig`, packed into the nupkg's `build/` folder beside a
`Waystone.Monads.props` that NuGet auto-imports. A consumer opts in with one property,
`WaystoneMonadsRuleset`, and gets nothing unless they set it. The shipped defaults stay
quiet; the tiers are an opt-in rather than a change to `Rules.cs`.
`Waystone.Monads.Shouldly` ships a parallel pair reading the *same* property, so a posture
is set once rather than per package.

**Add a rule and you add two preset rows.** `PresetTests` reads every descriptor out of
`Rules.cs` and fails when one has no entry, fails when an entry names an id no descriptor
declares, and fails when a tier's entries do not all carry that tier's severity. It
resolves them through Roslyn's own `AnalyzerConfigSet` rather than by matching the text,
so what it pins is the severity a compiler would apply.

**They are global configs, and must be.** `WM2020` is reported against `ErrorCodes.txt`,
which has no syntax tree, so a path-matched `.editorconfig` section cannot set its severity
at all — an `.editorconfig` fragment would ship a preset with one rule silently missing
from it. Two consequences to keep: `global_level` stays negative, because a tie with a
consumer's own global config is resolved by *unsetting* the option rather than by reporting
a conflict; and a consumer's path-matched `.editorconfig` beats the preset, which is the
override route the docs promise. `sample/Waystone.Monads.Analyzers.Sample` applies `strict`
and holds the `WM1` rules back down to warning in its `.editorconfig` — that is the only
executable statement of the precedence anywhere, and what keeps a project full of
deliberate misuse building.

**Do not read that sample as evidence that a codebase can adopt `strict` as shipped.** Most
of the rules `strict` moves are overridden straight back down in that `.editorconfig`, so
what the sample validates is the `EditorConfigFiles` plumbing, the override precedence, and
the `WM2` tier's raise from suggestion to warning. A preset's effect on real code is not
testable here: the only consumer in the tree is a fixture built to report.

**Do not change a shipped default to make a preset tidier.** The presets are additive by
construction; a default that moves needs a `### Changed Rules` row in
`AnalyzerReleases.Unshipped.md` and breaks the build of a consumer who only wanted a
version bump.
