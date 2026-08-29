# Waystone.Monads configuration sample

Every way to author `MonadOptions` without a host, one scenario per run:

```
dotnet run --project sample/Waystone.Monads.Configuration.Sample -- static
```

Pass no argument and it lists the names.

**One scenario per process, deliberately.** `MonadOptions` is process-wide and
publishing is a one-way swap, so running these back to back in one process would
have each inherit the last and the output would stop meaning anything. That is a
property of the library, not an awkwardness of the sample — it is why the install
happens once at start-up.

Every scenario reports what a caller would actually observe — the fallbacks a
blank `ErrorCode` and `Error` resolve to, and the code an exception produces —
rather than reading configuration back out. `MonadOptions.Current` is internal, so
this is the same view a consumer has.

## The scenarios

### `static` — no container at all

```csharp
MonadOptions.Configure(
    options => options.UseFallbackErrorCode("Contoso")
                      .UseFallbackErrorMessage("Something went wrong."));
```

A console application, a test, a worker built by hand. Needs no package beyond
`Waystone.Monads` itself, and takes effect on the call.

### `container` — a container, installed by hand

```csharp
services.AddWaystoneMonads(options => options.UseFallbackErrorCode("Contoso"));

using ServiceProvider provider = services.BuildServiceProvider();

provider.UseWaystoneMonads();
```

Two calls, because the configuration needs services the container has not built
yet. The run prints the options both before and after the install, so the gap is
visible: registration alone changes nothing.

### `factory` — a custom `ErrorCodeFactory`

```csharp
services.AddSingleton<ErrorCodeFactory, ShoutingErrorCodeFactory>();
services.AddWaystoneMonads();
```

No interface to implement — `ErrorCodeFactory` is a non-sealed class with
`virtual` members, so `ShoutingErrorCodeFactory` overrides `FromException` and
calls `base`. Registering it before `AddWaystoneMonads` wins, because the default
goes in with `TryAddSingleton`.

`TimeoutException` gives `TIMEOUT` instead of `Timeout`.

### `additive` — several registrations

```csharp
services.AddWaystoneMonads(
    options => options.UseFallbackErrorCode("FromTheLibrary")
                      .UseFallbackErrorMessage("Set by the library."));

services.AddWaystoneMonads(
    options => options.UseFallbackErrorCode("FromTheApplication"));
```

Both delegates run, in registration order, over one shared builder. The code ends
up `FromTheApplication` and the message stays `Set by the library.` — a later
call overrides what it sets and inherits what it does not. This is what makes it
safe for a library to call `AddWaystoneMonads` during its own registration.

### `logging` — logging wired from the container

```csharp
services.AddLogging(logging => logging.AddSimpleConsole());
services.AddWaystoneMonads();
```

Nothing here mentions Waystone logging. The install resolves whatever
`ILoggerFactory` the container holds and points
`Waystone.Monads.Extensions.Logging` at it. The scenario then swallows a
`TimeoutException` through `Option.Try` and the exception turns up in the console
log, carrying the call site the compiler recorded.

A container with no `ILoggerFactory` leaves logging unconfigured rather than
failing.

### `scope` — one flow overridden

```csharp
using (MonadOptions.BeginScope(options => options.UseFallbackErrorCode("Scoped")))
{
    // Scoped in here.
}
// Global again out here.
```

The only route that does not touch the process-wide options. It is confined to
the current asynchronous flow.

### `forgotten` — the failure this is all designed around

Registers configuration, builds the provider, and never installs it. The library
keeps working on defaults, which is exactly why the mistake is otherwise
invisible — so it writes a `Waystone.Monads.ConfigurationNotApplied` event
instead. `ForgottenInstallWatcher` subscribes and counts it.

The signal is held rather than spent while nothing is subscribed, so a subscriber
attached at any point before the install still receives it. Subscribe and throw
to make the omission fatal in a test suite.
