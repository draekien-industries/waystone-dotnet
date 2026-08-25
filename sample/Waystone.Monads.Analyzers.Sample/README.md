# Waystone.Monads analyzer sample

Every member in `Misuse.cs` and `Idioms.cs` is deliberately wrong. Those two files
exist so the analyzer has something to report, and so a change to a rule shows up
as a change in build output rather than only in a unit test.

`Ordering.cs` and `Chains.cs` are the exceptions: both are code you would be happy
to ship. `Ordering.cs` is here for the same reason as the other two — it exercises
the source generator, so a change to the emitted shape shows up as a build error
here rather than only in a snapshot test. `Chains.cs` pins the composition rules
the `waystone-monads` skill teaches, so a change to the async surface that made a
chain stop composing would break this build rather than only contradicting a
document.

**Building it produces warnings, and that is the point.** `Misuse.cs` carries the
`WM1xxx` rules, which ship at warning severity because the code they mark throws
or silently misbehaves at runtime. Do not fix them.

`Idioms.cs` carries the `WM2xxx` rules, which ship at info severity — they appear
in an IDE as suggestions and stay out of build output. Open the file in an IDE to
see them, or raise one in `.editorconfig` to bring it into the build.

The `.editorconfig` here enables the two `WM3xxx` migration rules, which ship
disabled. It is the opt-in a team adopting Option and Result would add while
converting a codebase.

## Why two files

`Misuse.cs` leaves nullable reference types **disabled**, because that is the
consumer the compiler helps least: assigning `null` to an `Option<int>` there
produces no compiler diagnostic at all, only `WM1002`. `Idioms.cs` enables
nullable, which is what `WM1005` needs to see that a value may be null.

## The generated error codes

`Ordering.cs` is one pass of an ordering pipeline: find the order, check it has not
shipped, check it can be delivered, reserve the stock, take the payment, dispatch it.
Every step returns a `Result<T, Error>` and they compose with `AndThen`, so the first
failure short-circuits the rest and the caller gets one `Error`.

The point is where those errors come from. `OrderErrorCode` is marked
`[ErrorCodeCatalog]`, and the generator turns it into everything the pipeline needs:

- `OrderErrorCodeCatalog.Errors.NotFound(message)` builds the failed step's `Error` with
  the right code already attached, so no step has to name a code as a string.
- `refusal.Value.ToError(message)` in `Reserve` handles the other direction. The
  warehouse hands back a bare `OrderErrorCode`, and the extension attaches a message
  at the boundary where one is worth writing.
- `StatusCodeFor` switches on the code to pick an HTTP status. **This is the method
  the feature exists for.** A `case` label needs a compile-time constant, so it can be
  written against `OrderErrorCodeCatalog.Names` and cannot be written against
  `ErrorCode.FromEnum` at all — that call works its string out at run time.

The enum also declares a format, `order.{member:kebab}`, so the codes are
`order.not-found` rather than `OrderErrorCode.NotFound`. That is the shape you would
actually put on a wire, and it is reached declaratively — no `ErrorCodeFactory`
subclass, and nothing to install at startup. The format is evaluated at build time, so
`StatusCodeFor` still switches on constants.

`ErrorCodes.txt` lists every code the project publishes, and the `AdditionalFiles` item
in the csproj is what opts in. `WM2019` reports a generated code the file does not list
and `WM2020` an entry nothing generates, so a rename cannot change a wire contract
without showing up as a line in a diff. The `.globalconfig` raises both to warnings —
and it has to be a global config rather than an `.editorconfig` section, because
`WM2020` is reported against `ErrorCodes.txt`, which has no syntax tree for a
path-matched section to apply to.

Two details are pinned here on purpose, so a change to either breaks this build rather
than only a snapshot test:

- The enum is `OrderErrorCode` and the generated class is `OrderErrorCodeCatalog`.
  The generator appends `Catalog` to the enum's name and trims nothing off it, so the
  repetition in the name here is the enum's to fix, not the generator's to hide.
- The `case` labels are the generated constants, so a change to the code scheme stops
  this file compiling, and a change to the casing rules changes what they compare
  against.

This project imports `Waystone.Monads.SourceGenerators.props` to load the generator,
the same way it imports the analyzer props. A consumer installing the NuGet package
gets both without doing either.

## Composable chains

`Chains.cs` is the reusable half of `Ordering.cs`. That file shows one finished
chain; this one shows how steps have to be shaped for a chain to be possible, and
what it takes for a chain to be reused instead of retyped.

Every member obeys one rule — **one parameter in, one monad out** — and the
consequences are what the file exists to pin:

- `Validated` composes three guard steps, each `Order → Result<Order, Error>`. A
  guard step answers with the value it was handed, so it fits anywhere an `Order`
  flows. Returning `bool` would fit only `Filter`, which keeps no reason for the
  refusal.
- The conditions themselves are named — `IsPresent`, `IsPositive` — which puts the
  reusable unit one layer below the step. A predicate is shared by every guard
  asking the same question; the guard is the layer that attaches the reason the
  predicate cannot carry. That is what leaves all three guards structurally
  identical, differing only in the question and the error code.
- `Validated` is itself `Order → Result<Order, Error>`, so `Bill` reuses it as a
  method group. `Bill` is then a step in turn, and `BillRounded` chains onto it
  without knowing how many steps it contains. **That is the whole of chain reuse.**
- The pricing step varies by caller, so it is held in a field rather than taken as
  a second parameter. A second parameter would read more directly and would cost
  every caller the ability to chain onto `Bill`.
- `BillAll` and `BillEach` run the same chain per element and gather it two
  different ways, which is why gathering stays at the call site — `Collect` stops
  at the first failure and `Partition` reports all of them.

`BillAsync` is the boundary worth reading closely. Async **steps** compose: an
async step is `T → Task<Result<U, Error>>`, which is what an I/O method returns
anyway, so `AndThenAsync` takes `ReserveAsync` by name — and the synchronous
`Validated` drops into the same chain untouched, because each `*Async` member
accepts a synchronous delegate too.

Async **chains** do not compose. The chain hands back a `ValueTask`, and no
`*Async` member accepts a `ValueTask`-returning delegate, so `BillAsync` cannot
itself be a step. Handing it to `AndThenAsync` fails as `CS0411` — "the type
arguments cannot be inferred from the usage" — reported against the call site and
naming neither `ValueTask` nor the step, so it reads like a generics problem
rather than the design constraint it is. Reuse the async steps, not the async
chain; `.AsTask()` would buy composability with an allocation on every call.

## Trying the code fixes

Open `Misuse.cs` or `Idioms.cs` in an IDE and invoke the lightbulb. Thirteen of the
twenty-six rules offer a fix; the rest are reported without one, either because the correction is
ambiguous (`Ok` or `Err`?) or because it changes a signature and cascades to
callers the fix cannot see.
