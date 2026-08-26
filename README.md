# Waystone.Net

[![Release](https://github.com/draekien-industries/waystone-dotnet/actions/workflows/release.yml/badge.svg)](https://github.com/draekien-industries/waystone-dotnet/actions/workflows/release.yml)
[![codecov](https://codecov.io/gh/draekien-industries/waystone-dotnet/graph/badge.svg?token=jrDIJLZrhD)](https://codecov.io/gh/draekien-industries/waystone-dotnet)

## Documentation

- [Draekien-Industries](https://draekien-industries.wpei.me/) - the published
  documentation, including the Deprecations page listing API that the next major
  release removes
- [Contributing](CONTRIBUTING.md) - how to build, test, and release this
  repository

## Getting Started

1. Read the documentation linked above
2. Clone the repository and have a look around
3. Have a read of the [contributing guide](CONTRIBUTING.md)
4. Code up a storm

## Installation

Waystone.Net is a collection of C# class libraries published to NuGet.org. You can install them via the NuGet package manager by searching for `Waystone` packages. The following packages are currently available:

- Waystone.Monads
- Waystone.Monads.Extensions.Logging
- Waystone.Monads.FluentValidation
- Waystone.WideLogEvents
- Serilog.Enrichers.Waystone.WideLogEvents
- Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore

## Agent tooling

This repository also ships a Claude Code plugin for developers writing code
against these packages. Its `waystone-monads` skill teaches an agent how to
compose `Option<T>` and `Result<TOk, TErr>`, the traps that defeat both types,
and the `WM` analyzer diagnostics that catch those traps.

You do not need the plugin to use the NuGet packages. Install it if you want
your agent to write idiomatic code against them without being told how each
time.

### Install the plugin

Add the marketplace, then install from it:

```
/plugin marketplace add draekien-industries/waystone-dotnet
/plugin install waystone-dotnet@waystone-dotnet
```

Both commands run inside Claude Code. The plugin name and the marketplace name
are the same, which is why `waystone-dotnet` appears twice.

### Install the skill on its own

Use [`npx skills`](https://agentskills.io) if you want the skill without the
plugin, or if you use an agent other than Claude Code:

```
npx skills add draekien-industries/waystone-dotnet --skill waystone-monads
```

That installs into the current project. Add `-g` to install for every project
instead. To see what else the repository offers, run the same command with
`--list` and no `--skill`.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for setup, the test matrix, commit
message conventions, and how releases work.
