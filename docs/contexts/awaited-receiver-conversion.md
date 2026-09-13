# Converting an extension family to generated awaited receivers

Read before applying `[GenerateAwaitedReceivers]` and `[GenerateAwaitedMember]` to an
extension class in `src/Waystone.Monads`. The generator's own contract is in
[src/Waystone.Internal.SourceGenerators/AGENTS.md](../../src/Waystone.Internal.SourceGenerators/AGENTS.md).

Read the baseline, do not estimate. Apply the attributes, build, and read the
RS0016/RS0017 pair: it names the exact parameter and type parameter drift between the
hand-written extension and the core member it forwards to. A family converts with an
untouched baseline only when the two already agree. A parameter rename is source-breaking
and waits for a major; a type parameter rename is not and lands as an ordinary
`refactor:`.

Run the experiment on the whole set at once rather than one family at a time: one build
reports every family's verdict, and the count of RS0017 rows per class is the verdict. Do
it before the core members change, so a failed conversion does not also hide fresh RS0016
rows.

## An apparent removal is a missing addition

**Drift is not the only blocker. When a conversion appears to remove overloads, check
which receiver the hand-written ones sit on before reaching for the generator.**

`FromExtensionBlocks` skips a member already on an awaited receiver, so a family whose
hand-written overloads all sit on one loses them. Converting `Option.MatchExtensions` in
that state removes six — the three async-delegate shapes on each of the `Task` and
`ValueTask` receivers. `Result.Match` loses nothing from the identical attempt, because
its async-delegate shapes sit on the synchronous `Result<TOk, TErr>` receiver and are
lifted. Both core types declare the same four-overload `Match` set, so the core surface is
never the difference.

The fix is to add the missing synchronous-receiver overloads first and then convert: for
`Option.Match` that measures 0 RS0017 and 38 RS0016. Reach for that check first.

That Option's dropped overloads all involve a parameterless `Func<Task<TOut>>` branch
where Result's take the contained value is a coincidence of which overloads Option ships,
not the cause. It reads like one.

`OkOrElseExtensions` is the other trap, and it is downstream of the same thing. It forwards
through `optionTask.MatchAsync(...)` with a value-returning `async` lambda in the `None`
branch. Convert without the synchronous overloads and resolution falls to the generated
`MatchAsync(Action<T>, Action)` shape, so the lambda becomes a void-returning conversion
and fails as `CS8030` in a file with nothing wrong in it. With the synchronous overloads
present it resolves correctly and needs no edit, so treat a `CS8030` there as a symptom of
the missing block rather than a call site to fix.

A class whose awaited shapes were hand-written before they were generated also has to pin
their existing names with `ReceiverParameterName`, or the conversion that changes nothing
else still moves every baseline row. The generator's `AGENTS.md`, linked above, has the
rule.
