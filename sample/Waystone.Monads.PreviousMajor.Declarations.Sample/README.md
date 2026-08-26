# The declaration-phase half of the previous-major sample

Read
[`../Waystone.Monads.PreviousMajor.Sample/README.md`](../Waystone.Monads.PreviousMajor.Sample/README.md)
first. This project exists only because a declaration-phase error masks every
body-phase error in the same compilation, so the two call sites that break at
declaration phase — a `using static` on an extension class, and an `override` of
a removed virtual — are compiled apart from the rest of the inventory.

Add a file here only if it breaks before the compiler binds method bodies: a
`using` directive, a base type, a member signature, an `override`. Everything else
belongs in the other project.
