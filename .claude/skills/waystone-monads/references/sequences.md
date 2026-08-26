# Sequences of monads

| Need | Use |
| --- | --- |
| All succeed, or the first failure | `Collect` |
| Every failure, not just the first | `Partition` |
| Drop the absent and keep the rest | `Flatten` / `FlattenErr` |
| Transform or narrow every element in place | `Map` / `Filter` |
| First or last present element | `FirstOrNone` / `LastOrNone` |
| Split `Option<(T1, T2)>` into a pair | `Unzip` |

## Collect fails fast; Partition reports everything

`Collect` is the port of Rust's `collect::<Result<Vec<_>, E>>()` and
`collect::<Option<Vec<_>>>()`, and it short-circuits the same way: enumeration
stops at the first `None` or `Err`, so the tail is never visited, later failures
are never seen, and a side-effecting source is left partly consumed. The
successes gathered before the failure are discarded — a `Result` collect keeps
the **first** error only.

`Partition` is the counterpart for a caller that must report every failure —
validation, a batch import, a bulk API. It enumerates the whole source and
returns both lists, each present and possibly empty, never null. It exists only
for `Result`; there is no `Option` equivalent, because absences carry nothing to
report.

Choosing wrongly loses data silently in either direction: `Collect` where
`Partition` was meant drops every failure after the first, and `Partition` where
`Collect` was meant carries on past a failure that should have stopped the work.

Both enumerate **when called** rather than when the result is read, so neither
may be handed an unbounded sequence.

`CollectAsync` is the `IAsyncEnumerable` form, short-circuiting the same way on
a pull-based source. It returned `Task` up to 6.x and returns `ValueTask` from
7.0.0, like the rest of the async surface.

## Behaviours that invert the obvious guess

Check against this rather than assuming:

| Looks like | Actually |
| --- | --- |
| `Collect` on an empty sequence fails | Succeeds with an empty list — `Some([])` or `Ok([])` |
| Collection-level `Filter` drops elements | Preserves length and position, turning failing `Some`s into `None`s in place; chain `Flatten` to compact |
| Collection-level `Map` may change length | Preserves length and position too |
| `LastOrNone` costs what `FirstOrNone` costs | Must enumerate the whole sequence; only `FirstOrNone` short-circuits |

`Flatten` and the collection-level `Map` and `Filter` are deferred and
re-evaluated on each enumeration, unlike `Collect` and `Partition`. Because
`Flatten` changes the sequence length, its output can no longer be lined up with
the source by position.

## The two-operand combinators disagree with each other

The same inverted-expectation trap runs through the members that take a second
monad:

| Member | Present + Present | Present + Absent | Absent + Absent |
| --- | --- | --- | --- |
| `Zip` / `ZipWith` | Combined | **Absent** | Absent |
| `Reduce` | Combined | **The present one survives** | Absent |
| `Xor` | **Absent** | The present one | Absent |

`Zip` is strict, `Reduce` is a merge, and `Xor` is exclusive. Reaching for
`Reduce` while expecting `Zip`'s all-or-nothing is the common error, and it
fails silently because both return the same type.

`And` is stricter than it looks in a different way: it takes its argument
eagerly and returns it **as-is** when the receiver is present, rather than
deriving it from the contained value. `AndThen` is the value-dependent, lazy
one.

## FirstOrNone and its unwrapping siblings

`FirstOrNone` and `LastOrNone` search a sequence of options, skip the absent
elements, and return `None` rather than throwing when nothing matches.
`FirstOr`, `FirstOrElse`, `LastOr` and `LastOrElse` do the same search and
collapse in one call, taking an eager value and a lazy factory respectively —
the same eager/lazy split as `UnwrapOr` and `UnwrapOrElse`, and the same reason
to prefer the lazy form when the fallback is not free.
