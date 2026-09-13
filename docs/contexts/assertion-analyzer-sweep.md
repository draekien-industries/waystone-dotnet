# Sweeping WMS2001 and WMS2002 across a test project

Read when migrating a test project's assertions to the `Waystone.Monads.Shouldly` forms.

`Waystone.Monads.Tests` imports the assertion analyzers, so WMS2001 and WMS2002 report on
it. They are `Info`, so nothing fails; MSBuild does not log them either, which is why the
build looks silent. To see them, run the fix:

```
dotnet format analyzers test/Waystone.Monads.Tests/Waystone.Monads.Tests.csproj \
  --diagnostics WMS2001 WMS2002 --severity info
```

**Run that until it stops changing files — one pass is not enough.** WMS2001 rewrites
`(await x).IsSome.ShouldBeTrue()` into `(await x).ShouldBeSome()`, which is then WMS2002's
input, and the batch fixer only lands non-overlapping fixes per pass. Sweeping this project
takes three passes to reach a fixed point.

**Always scope it with `--diagnostics`.** Without it, `dotnet format` applies every fix
available at `Info` across the project, and the sweep disappears into several hundred
unrelated edits.
