# Installing from the host

`Waystone.Monads.Extensions.Hosting` exists to delete one call. Registering
configuration on an `IServiceCollection` needs a second call on the built
provider to install it, and forgetting that call is silent — the library keeps
working on its defaults. This package runs the install from the host's own
start-up sequence instead:

```csharp
builder.AddWaystoneMonads(options => options.UseFallbackErrorCode("Contoso"));

var app = builder.Build();

// No second call.
```

That is `AddWaystoneMonads` on `IHostApplicationBuilder`, which both
`WebApplicationBuilder` and the builder from `Host.CreateApplicationBuilder`
implement. An overload hands your delegate the built provider, which is how a
companion package is pointed at something the container holds:

```csharp
builder.AddWaystoneMonads((provider, options) =>
    options.UseFallbackErrorCode("Contoso")
           .UseLoggerFactoryFrom(provider));
```

On the older `IHostBuilder`, which has no such interface, reach the same pair
through `ConfigureServices` and `EnableInstallOnStart()`. That call hangs off the
`MonadServicesBuilder` that `AddWaystoneMonads` returns, so asking for the
install without first asking for the registration does not compile. It registers
the installer and nothing else, so `AddWaystoneMonads` is still where
configuration goes, and calling it twice installs once.

## Registration order does not matter, but the host's start does

The install runs in `IHostedLifecycleService.StartingAsync`, which the host calls
on *every* hosted service before it calls `StartAsync` on any of them. So a
background service reading `MonadOptions` in its own `StartAsync` sees the
installed configuration whether it was registered before `EnableInstallOnStart`
or after. A plain `IHostedService` would install in `StartAsync`, in registration
order, and a service registered ahead of it would read the defaults — which is
the whole reason this is a lifecycle service.

**Work done before the host starts is still too early.** A read taken while the
service collection is being populated, or between `Build()` and `Run()`, runs
ahead of every hosted service, is answered from the defaults, and is reported
through the `ConfigurationNotApplied` event exactly as it would be without this
package. Configuration is applied at host start, not at container build.

Without a host this package does nothing for you — call `UseWaystoneMonads()` on
the provider yourself.
