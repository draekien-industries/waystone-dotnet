# Waystone.Monads.Shouldly.Analyzers

Also covers `Waystone.Monads.Shouldly.Analyzers.CodeFixes`. Both are `IsPackable=false`
and ship inside the `Waystone.Monads.Shouldly` package, not the `Waystone.Monads` one.

## Constraints

**These rules must not move into `Waystone.Monads.Analyzers`.** That assembly ships
inside the core package, so every consumer of `Waystone.Monads` would receive a
diagnostic telling them to call `ShouldBeSome` — a method they do not reference — with
a code fix producing source they cannot compile. The two-package split is the whole
reason this area exists, and it is not a structural preference that can be tidied away
later.

**Never reference `Waystone.Monads` or `Waystone.Monads.Shouldly`.** The assertions
package loads these assemblies as analyzers, so a project reference back is a build
cycle. Both libraries' types are resolved by metadata name through
`AssertionSymbols.TryCreate`, which doubles as the gate: a project without
`Shouldly.OptionAssertions` gets no diagnostics at all. That gate is a test, not an
implementation detail — `GivenTheAssertionsPackageIsAbsent_ThenReportNothing` exists in
both analyzer test classes.

**The `WMS` prefix is deliberate and the tier digit carries over.** `WM` is at `WM2021`
and is validated end to end by `RulesTests` in `Waystone.Monads.Analyzers.Tests`, whose
`EveryRuleIsSupportedByAnAnalyzer` reflects over that assembly's own types — so a `WM`
id defined here would either fail that test or force it to be weakened. Keeping the `2`
means the severity policy in
[Waystone.Monads.Analyzers](../Waystone.Monads.Analyzers/AGENTS.md) transfers unchanged
rather than having to be restated. There is no `WMS1` tier: every rule here fires on
code that works, so none may ship above `Info`.

**Write and change descriptors through the `writing-diagnostic-descriptors` skill**, as
in the core analyzer area. The paired obligations there apply here too, against this
directory's own `AnalyzerReleases.{Shipped,Unshipped}.md` and this area's own
`RulesTests`.

**A new rule needs a row in both severity presets**, under
`src/Waystone.Monads.Shouldly/build/`. This area's `PresetTests` mirrors the core
one's and fails on a descriptor with no entry. The presets read the *same*
`WaystoneMonadsRuleset` property the core package reads, so a consumer sets one
posture for every Waystone package they installed — which means the two packages'
files have to agree about what each tier name means. `recommended` changes nothing
here, because there is no misuse tier to promote into; `strict` raises both rules to
warning. Read [Waystone.Monads.Analyzers](../Waystone.Monads.Analyzers/AGENTS.md) for
why the files are global configs and why `global_level` is negative.

## Gotchas

**`IInvocationOperation.TargetMethod` arrives unreduced for an extension method.** Its
parameter 0 is the receiver, so a check that skips one parameter to reach past the
expected value lands on `expected` instead and silently rejects every overload. This
cost a full round of red tests on `Shouldly.ShouldBe`. Normalise with
`method.ReducedFrom ?? method` — the same idiom `MonadSymbols.IsMonadMethod` uses — and
count from the static signature.

**The assertion names are string literals and nothing in the build checks them.**
Renaming an assertion in `Waystone.Monads.Shouldly` leaves the rule reporting and the
fix applying against a method that no longer exists, and the result does not compile.
`MonadAssertionTests` in the test project is the only place the two sides can be
compared, because it is the only assembly that references both. It checks both
directions: every name the analyzers write exists, and every assertion the package
ships is named by something.

**A code fix here carries neither the formatter nor the simplifier annotation**, unlike
`MonadCodeFix`. Both fixes swap one expression for another in place, so the formatter
has nothing to do except reindent the replaced statement to its own canonical depth —
which rewrites lines the fix was not asked to touch and, across a suite-wide batch,
turns a reviewable diff into an unreadable one. Adding the annotation also breaks the
fix tests, whose sources are not canonically indented.

**WMS2002 whitelists the positions it will rewrite.** The fix moves `await` outward and
every postfix operator binds tighter than `await`, so a chained member access would
rewrite to code that reads the member off the task and does not compile. Only an
expression statement and a variable initialiser are reported. Widening that set means
proving the new position binds looser than `await`, not just that it looks harmless —
and an exclusion list in its place would have to be exhaustive to be correct.

**WMS2001 overlaps `WM2001` on the `Unwrap` shape, deliberately.** The spans differ,
the messages say different things, and applying this fix resolves both because the
rewrite is what removes the `Unwrap`. Do not suppress either to silence the pair: a
consumer without this package would then get no signal at all.

**`ShouldBeOfType<Some<T>>()` is excluded rather than handled.** Those sites usually
test the closed hierarchy itself, and nothing in the syntax separates that from an
incidental type check, so rewriting them would delete the only coverage of it. The
exclusion is a test in both analyzer test classes, not a comment.

## Tests

**This namespace shadows the global `Shouldly`.** A file declaring
`namespace Waystone.Monads.Shouldly.Analyzers` resolves a plain `using Shouldly;` to the
enclosing `Waystone.Monads.Shouldly`, which holds no types, and every assertion in the
file stops compiling. Write `using global::Shouldly;`. It is the same resolution rule
the package's own README describes, met from the other side.

The harness, framework pinning and force-enabled-diagnostic caveats are the same as
[Waystone.Monads.Analyzers](../Waystone.Monads.Analyzers/AGENTS.md); read that file's
Tests section as well.
