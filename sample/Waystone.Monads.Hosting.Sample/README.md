# Waystone.Monads hosting sample

Every way to author `MonadOptions` on a host, one scenario per run:

```
dotnet run --project sample/Waystone.Monads.Hosting.Sample -- host
```

Pass no argument and it lists the names. As in the configuration sample, one
scenario per process — `MonadOptions` is published once, process-wide.

Each host is started and immediately stopped, since the install happens at start.

## The scenarios

### `host` — the one call

```csharp
builder.AddWaystoneMonads(options => options.UseFallbackErrorCode("FromTheHost"));

var app = builder.Build();
```

`AddWaystoneMonads` on `IHostApplicationBuilder`, which both
`WebApplicationBuilder` and `Host.CreateApplicationBuilder` give you. No
`UseWaystoneMonads` on the built provider — the host runs it.

The run prints the options before the host starts and after, so you can see that
building the host is not what applies them. Starting it is.

### `legacy` — the older `IHostBuilder`

```csharp
new HostBuilder().ConfigureServices(
    (_, services) => services
                    .AddWaystoneMonads(
                         options => options.UseFallbackErrorCode("FromConfigureServices"))
                    .EnableInstallOnStart());
```

`IHostBuilder` has no `IHostApplicationBuilder`, so reach the same pair through
`ConfigureServices`. `EnableInstallOnStart` hangs off the builder
`AddWaystoneMonads` returns, so there is no way to ask for the install without
first asking for the registration.

### `config` — reading `appsettings.json`

```csharp
builder.AddWaystoneMonads(
    options => options.ReadFromConfiguration(builder.Configuration));
```

Opt-in, mirroring Serilog's `ReadFrom.Configuration()`. `AddWaystoneMonads` never
reaches for an `IConfiguration` by itself. The `WaystoneMonads` section of
`appsettings.json` supplies both fallbacks.

Every key is optional and an absent one leaves its setting alone.

### `section` — a section of another name

```csharp
options.ReadFromConfiguration(builder.Configuration, "Contoso");
```

The same file's `Contoso` section sets only `FallbackErrorCode`, so the message
stays at the default — the "absent key changes nothing" rule, visible.

### `invalid` — configuration that cannot be honoured

`CatchesCancellation` is set to `sometimes`. Start-up stops with an
`ArgumentException` naming the key and the value:

```
The value of 'WaystoneMonads:CatchesCancellation' must be true or false, but was 'sometimes'.
```

That is the point of opting in. A typo in a section you asked to be bound stops
the application where the mistake is written, rather than degrading silently to a
default nobody chose.

### `order` — registration order does not matter

`EarlyReader` is an `IHostedService` registered *before* `AddWaystoneMonads`, and
it reads the options in its own `StartAsync`. It still sees `FromTheHost`.

The install runs in `IHostedLifecycleService.StartingAsync`, which the host calls
on every hosted service before `StartAsync` on any of them. A plain
`IHostedService` installer would run in registration order and this scenario would
print the defaults.

## What is still too early

Work between `Build()` and `Run()` runs ahead of every hosted service. The `host`
scenario shows it: the read before starting reports `Unspecified`. Such a read is
answered from the defaults and reported through the
`Waystone.Monads.ConfigurationNotApplied` event, exactly as it would be without
this package. Configuration is applied at host start, not at container build.
