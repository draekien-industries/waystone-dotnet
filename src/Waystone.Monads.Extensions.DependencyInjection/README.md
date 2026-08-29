# Waystone.Monads.Extensions.DependencyInjection

Authors the ambient `Waystone.Monads` configuration from a dependency injection
container.

`MonadOptions` is ambient by design — `Option` and `Result` read it statically, so
nothing is threaded through call sites and no monad gains a constructor
dependency. What this package changes is only how those options are *written*: by
the container at start-up rather than by a hand-written static call.

## Install and configure

```
dotnet add package Waystone.Monads.Extensions.DependencyInjection
```

```csharp
builder.Services.AddWaystoneMonads(
    options => options.UseFallbackErrorCode("Contoso"));

var app = builder.Build();

app.Services.UseWaystoneMonads();
```

Everything this package ships is in the
`Microsoft.Extensions.DependencyInjection` namespace, which a host application
already has in scope — no extra `using`, including for `ReadFromConfiguration`.

`AddWaystoneMonads` returns a `MonadServicesBuilder` rather than the collection.
Its `Services` property is the same collection you passed in, so carry on from
there:

```csharp
builder.Services.AddWaystoneMonads()
       .Services.AddSingleton<IClock, SystemClock>();
```

The builder exists so a companion package can offer a call that only makes sense
once registration has happened — `Waystone.Monads.Extensions.Hosting` hangs
`EnableInstallOnStart()` off it, so there is no way to ask for the install
without first asking for the registration.

Calling `AddWaystoneMonads` more than once accumulates rather than conflicts.
Each `configure` delegate is kept and they run in registration order at install
time, so a later call overrides an earlier one. Everything else it does is
idempotent, which makes it safe for a library to call during its own registration
without knowing whether the application already has.

## Two calls, and why

`AddWaystoneMonads` registers. `UseWaystoneMonads` installs. They are separate
because the configuration needs services the container has not built yet — an
`ILoggerFactory` does not exist while the collection is still being populated.

This is Serilog's bootstrap-logger split, with the expensive half left out.
Serilog buffers events written before the bind, because a log event emitted early
is lost forever. Nothing is lost here: options read between the two calls are
answered from the defaults, which are valid settings rather than a broken state.

**Forgetting the second call is the failure mode**, and it is silent — the library
keeps working on defaults. So it is instrumented instead. A read taken after
`AddWaystoneMonads` and before `UseWaystoneMonads` writes a
`Waystone.Monads.ConfigurationNotApplied` event to the `Waystone.Monads`
`DiagnosticListener`:

```csharp
// In a test suite, subscribe and throw to make the omission fatal.
listener.Subscribe(observer, name => name == "Waystone.Monads.ConfigurationNotApplied");
```

The signal is held rather than spent while nothing is subscribed, so a subscriber
attached at any point before the install still receives it.

`Waystone.Monads.Extensions.Hosting` removes the second call on a host by running
it from the host's own start-up sequence.

## What the container supplies

At install time, three things are applied in order, each overwriting the last:

1. The options already in effect, so an earlier `MonadOptions.Configure` call is
   carried forward rather than discarded.
2. `ErrorCodeFactory`, if the container holds one.
3. Every delegate passed to `AddWaystoneMonads`, in registration order.

So a delegate has the last word.

**`ErrorCodeFactory` is the only thing resolved for you.** Everything else a
companion package needs is wired by a delegate you pass. Take the overload that
hands you the built provider:

```csharp
builder.Services.AddWaystoneMonads((provider, options) =>
    options.UseFallbackErrorCode("Contoso")
           .UseLoggerFactoryFrom(provider));
```

`UseLoggerFactoryFrom` ships from `Waystone.Monads.Extensions.Logging`, so the
package you installed is the one you call. Installing this package does not drag
that one in, and installing that one does not silently change what this one does.

Resolve singletons only. The options are one process-wide snapshot, so a scoped
service captured here outlives the scope it came from.

**`ErrorCodeFactory` has no interface.** It is a public non-sealed class with
`virtual` members, so you override it by subclassing:

```csharp
builder.Services.AddSingleton<ErrorCodeFactory, ContosoErrorCodeFactory>();
```

Registered before `AddWaystoneMonads` or after, it wins either way: the default is
registered with `TryAddSingleton`.

**A scoped service cannot be held here.** The options are one process-wide
snapshot published once, so anything resolved into them outlives the scope it
came from. Register an `ErrorCodeFactory` as scoped and, depending on whether the
container validates scopes, the install either fails or quietly captures the root
instance and hands it to every request for the life of the process. Per-request
configuration is a separate problem and is not solved by this package.

## Reading from configuration

Configuration binding is opt-in, mirroring Serilog's `ReadFrom.Configuration()`.
`AddWaystoneMonads` never reaches for an `IConfiguration` on its own:

```csharp
builder.Services.AddWaystoneMonads(
    options => options.ReadFromConfiguration(builder.Configuration));
```

```json
{
  "WaystoneMonads": {
    "FallbackErrorCode": "Contoso",
    "FallbackErrorMessage": "Something went wrong.",
    "CatchesCancellation": false
  }
}
```

Every key is optional, and an absent one leaves its setting alone — a section with
one key changes one setting. Pass a second argument to read a section other than
`WaystoneMonads`.

**A key that is present but unusable throws**, which is the point of opting in:
an empty `FallbackErrorCode`, or a `CatchesCancellation` that is not `true` or
`false`, stops start-up where the mistake is written rather than degrading to a
default nobody chose.

`"CatchesCancellation"` is honoured either way round, so `false` puts the setting
back even where code earlier in the chain called `UseCancellationAsFailure()`.

Binding goes through the builder's `Use*` methods rather than the reflection
binder, because the settings have no public setters to bind to.

## Without a Microsoft container

Both services are resolved through `IServiceProvider.GetService` itself rather
than through any container-specific API, so `UseWaystoneMonads` works on a
provider produced by any conforming container. `AddWaystoneMonads` needs an
`IServiceCollection`; a container that populates itself from one — which most
do — is enough.
