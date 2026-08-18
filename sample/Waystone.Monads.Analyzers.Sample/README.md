# Waystone.Monads analyzer sample

Every member in this project is deliberately wrong. The project exists so the
analyzer has something to report, and so a change to a rule shows up as a change
in build output rather than only in a unit test.

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

## Trying the code fixes

Open either file in an IDE and invoke the lightbulb. Thirteen of the twenty-five rules
offer a fix; the rest are reported without one, either because the correction is
ambiguous (`Ok` or `Err`?) or because it changes a signature and cascades to
callers the fix cannot see.
