# Waystone.Monads.SystemTextJson.AotVerification

A console harness that publishes with `PublishAot` and checks what the JSON
converters actually do under NativeAOT. It exists because the answer is not the
same for every type argument, and nothing else in the repository would notice if
it changed.

## Running it

```
dotnet publish test/Waystone.Monads.SystemTextJson.AotVerification -c Release
./test/Waystone.Monads.SystemTextJson.AotVerification/bin/Release/net10.0/win-x64/publish/Waystone.Monads.SystemTextJson.AotVerification.exe
```

It exits `0` when every check behaves as the package README documents, and
non-zero with a reason otherwise.

**On Windows the native link step needs `vswhere.exe` on `PATH`.** Without it the
publish fails inside `Microsoft.NETCore.Native.targets` with
`MSB3073 ... exited with code 123`, which reads like a compiler failure rather
than a missing tool. It lives in
`C:\Program Files (x86)\Microsoft Visual Studio\Installer`.

CI does not run this. The workflows run `dotnet test`, not `dotnet publish`, and
a NativeAOT publish needs a native toolchain the runners are not set up for.
Run it by hand when the converters or the factories change.

## What it found

| Path | Under NativeAOT |
| --- | --- |
| Reference-type argument through the factory | Works |
| Value-type argument through the factory | **Throws `NotSupportedException`** |
| Any argument through an explicitly registered converter | Works |

The failure is `'OptionJsonConverter\`1[System.Int32]' is missing native code or
metadata`. A generic instantiation over a value type needs its own compiled
code, and the compiler cannot see through `MakeGenericType` to know it will be
asked for one. Reference types all share a single compiled body, so they are
fine.

`[DynamicDependency]` on the factory does not rescue this. It keeps the open
generic definition and its constructors from being trimmed, which is a different
problem — it cannot conjure an instantiation that was never compiled.

The harness treats each row above as the expected outcome. A value-type check
that suddenly *succeeds* fails the run on purpose, because that would mean the
runtime changed and the package README's workaround is no longer needed.

## Why the value-type checks use different types from the explicit checks

The factory checks use `int`, `double` and `Guid`; the explicit checks use
`long`. That separation is load-bearing and not a stylistic accident.

Naming `new OptionJsonConverter<long>()` anywhere in the program makes
`OptionJsonConverter<long>` statically reachable, so the compiler emits it and
the factory would then find it at run time. An earlier version of this harness
used `int` on both sides and reported that every factory check passed — the
explicit registration was rooting the very instantiation the factory check was
supposed to prove was missing.

If you add a check, give the factory side a type argument that appears nowhere
else in the program.

## Reflection is deliberately left on

`PublishAot` turns off `System.Text.Json`'s reflection-based path, and the
csproj turns it back on with `JsonSerializerIsReflectionEnabledByDefault`,
suppressing `IL2026` and `IL3050`. That path is the subject: the question is
whether the converter factories can close their generics at run time, which only
arises when the serializer is resolving converters at run time.

A consumer who wants those warnings to stay meaningful uses a source-generated
`JsonSerializerContext` instead. That is a different configuration and this
harness does not cover it.
