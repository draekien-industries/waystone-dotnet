# Tests

## Running them

`dotnet test` with no `--framework` runs every target framework — five for
`Waystone.Monads.Tests` — and takes about twelve seconds incrementally. The
`pre-push` hook runs exactly that, because CI pins `--framework net8.0` for
coverage collection and would let a net472, net481, net9.0 or net10.0 break
through.

## Gotchas

**`MonadOptions.Global` is a process-wide mutable singleton.** A test that mutates
it and then asserts on it will flake against tests in other xUnit collections
running in parallel. Use `MonadOptions.BeginScope` so the override is confined to
the current asynchronous flow.

**Reqnroll binds step definitions across the whole test assembly.** The
`Specs/Options/Steps` and `Specs/Results/Steps` folders scope nothing, so step text
has to be unique across the project and a step class binds happily from the wrong
folder. When a step is not found, the folder is never the reason.

**A step that switches on a string argument needs a `default` that throws.**
Without one an unmatched value runs no assertion and the scenario passes. This hid
three no-op assertions in the Result specs.

**`ClosedHierarchyTests` lives in the analyzer test project, not here.**
`Waystone.Monads.Tests` has `InternalsVisibleTo`, so it would compile an
out-of-assembly derived type happily and prove nothing.
