# Waystone.Monads.Extensions.Hosting

Installs the container-registered `Waystone.Monads` configuration from the host's
own start-up sequence.

`Waystone.Monads.Extensions.DependencyInjection` splits registration from
installation, because configuration registered on an `IServiceCollection` needs
services the container has not built yet. That leaves an application holding a
second call it has to remember:

```csharp
var app = builder.Build();

app.Services.UseWaystoneMonads();   // easy to forget
```

Forgetting it is silent — the library keeps working on its defaults. This package
removes the call rather than relying on anybody to remember it.

## Install and configure

```
dotnet add package Waystone.Monads.Extensions.Hosting
```

```csharp
builder.Services
       .AddWaystoneMonads(options => options.UseFallbackErrorCode("Contoso"))
       .InstallWaystoneMonadsOnStart();

var app = builder.Build();

// No second call.
```

`InstallWaystoneMonadsOnStart` registers the installer and nothing else, so
`AddWaystoneMonads` is still where configuration goes. Called on its own it
installs the defaults plus whatever the container supplies.

Registering the installer twice installs once — the registration is deduplicated
on the implementation type.

## Registration order does not matter

The install runs in `IHostedLifecycleService.StartingAsync`, which the host calls
on every hosted service before it calls `StartAsync` on any of them. So a
background service that reads `MonadOptions` in its own `StartAsync` sees the
installed configuration whether it was registered before `InstallWaystoneMonadsOnStart`
or after.

That is the whole reason this is a lifecycle service rather than a plain
`IHostedService`. A plain one would install in `StartAsync`, in registration
order, and a service registered ahead of it would read the defaults.

**Work done before the host starts is still too early.** A read taken while the
service collection is being populated, or between `Build()` and `Run()`, runs
ahead of every hosted service. Such a read is answered from the defaults and
reported through the `Waystone.Monads.ConfigurationNotApplied` diagnostic event,
exactly as it is without this package. Configuration is applied at host start, not
at container build.

## Without a host

Nothing here applies. Call `UseWaystoneMonads()` on the provider yourself —
`Waystone.Monads.Extensions.DependencyInjection` is all a console application, a
test, or a container built by hand needs.
