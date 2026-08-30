# Configuring from a container

`MonadOptions` is ambient by design — the monads read it statically, so nothing
is threaded through call sites and no monad gains a constructor dependency.
`Waystone.Monads.Extensions.DependencyInjection` changes only how the options are
*written*: by the container at start-up rather than by a hand-written static
call.

```csharp
builder.Services.AddWaystoneMonads(
    options => options.UseFallbackErrorCode("Contoso"));

var app = builder.Build();

app.Services.UseWaystoneMonads();
```

Everything ships in `Microsoft.Extensions.DependencyInjection`, which a host
application already has in scope.

`AddWaystoneMonads` returns a `MonadServicesBuilder` rather than the collection;
its `Services` property is the same collection, so carry on from there. Calling
it more than once accumulates rather than conflicts — each delegate is kept and
they run in registration order, so a later one overrides an earlier one, and
everything else it does is idempotent. That makes it safe for a library to call
during its own registration without knowing whether the application already has.

## Register and install are two calls, and forgetting the second is silent

`AddWaystoneMonads` registers; `UseWaystoneMonads` installs. They are separate
because the configuration needs services the container has not built yet — an
`ILoggerFactory` does not exist while the collection is still being populated.

Nothing is lost in between: options read before the install are answered from the
defaults, which are valid settings rather than a broken state. **That is exactly
why omitting the install is the failure mode** — the library keeps working and
nothing complains. It is instrumented instead: a read taken after the register
and before the install writes the `ConfigurationNotApplied` event. The signal is
held rather than spent while nothing is subscribed, so a subscriber attached any
time before the install still receives it.

```csharp
// Make the omission fatal in a test suite by throwing from the subscriber.
using IDisposable watching =
    MonadDiagnostics.ConfigurationNotAppliedEvent.Subscribe(_ => throw new InvalidOperationException());
```

**Install before the application accepts work.** A request handler or background
service that runs ahead of the install reads the defaults for the whole of that
flow, not just once, and the event is the only trace of it.
`Waystone.Monads.Extensions.Hosting` removes both the second call and the
ordering risk rather than relying on anyone to remember them.

## Only ErrorCodeFactory is resolved for you

At install time three things are applied in order, each overwriting the last: the
options already in effect, so an earlier `MonadOptions.Configure` is carried
forward rather than discarded; an `ErrorCodeFactory` if the container holds one;
and then every delegate passed to `AddWaystoneMonads`. So a delegate has the last
word.

Everything else a companion package needs is wired by a delegate you pass. Take
the overload handed the built provider:

```csharp
builder.Services.AddWaystoneMonads((provider, options) =>
    options.UseFallbackErrorCode("Contoso")
           .UseLoggerFactoryFrom(provider));
```

`UseLoggerFactoryFrom` ships from `Waystone.Monads.Extensions.Logging`, so the
package you installed is the one you call — installing one of these does not drag
in or silently reconfigure the other.

`ErrorCodeFactory` has no interface. It is a public non-sealed class with
`virtual` members, so override it by subclassing and registering the subclass.
The default is registered with `TryAddSingleton`, so yours wins whether it is
registered before `AddWaystoneMonads` or after.

**Resolve singletons only.** The options are one process-wide snapshot published
once, so anything resolved into them outlives the scope it came from. Register a
scoped `ErrorCodeFactory` and the install either fails validation or quietly
captures the root instance and hands it to every request for the life of the
process. Per-request configuration is a different problem and this package does
not solve it.

## Binding configuration is opt-in

`AddWaystoneMonads` never reaches for an `IConfiguration` on its own:

```csharp
builder.Services.AddWaystoneMonads(
    options => options.ReadFromConfiguration(builder.Configuration));
```

It reads `FallbackErrorCode`, `FallbackErrorMessage` and `CatchesCancellation`
from a `WaystoneMonads` section, or from a section named in a second argument.
Every key is optional and an absent one leaves its setting alone.

**A key that is present but unusable throws,** which is the point of opting in —
an empty `FallbackErrorCode`, or a `CatchesCancellation` that is neither `true`
nor `false`, stops start-up where the mistake is written rather than degrading to
a default nobody chose. `CatchesCancellation` is honoured either way round, so
`false` puts the setting back even where earlier code called
`UseCancellationAsFailure()`.

Both services are resolved through `IServiceProvider.GetService` rather than any
container-specific API, so the install works on a provider from any conforming
container.
