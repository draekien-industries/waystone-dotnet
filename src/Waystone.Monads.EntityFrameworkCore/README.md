# Waystone.Monads.EntityFrameworkCore

Maps an `Option<T>` property onto a single nullable column. `Some(value)` stores
the value, `None` stores `NULL`.

## Querying an option

Storing an option needs one call. Querying one needs a second, and without it
every query below throws.

```csharp
services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(connectionString)
                      .UseWaystoneOptionQueries());
```

With it, these translate:

| You write | SQL |
| --- | --- |
| `Where(p => p.Age.IsSome)` | `WHERE "Age" IS NOT NULL` |
| `Where(p => p.Age.IsNone)` | `WHERE "Age" IS NULL` |
| `Where(p => p.Age == Option.Some(31))` | `WHERE "Age" = 31` |
| `Where(p => p.Age == Option.None<int>())` | `WHERE "Age" IS NULL` |
| `Where(p => p.Age != Option.None<int>())` | `WHERE "Age" IS NOT NULL` |

Write the option inline, as above.

### A captured option throws

This is the one form that does not work, and it throws rather than answering:

```csharp
Option<int> wanted = Option.None<int>();

// InvalidOperationException
context.People.Where(person => person.Age == wanted);
```

Entity Framework Core turns a captured value into a SQL parameter before any
rewrite can run, so one compiled query has to serve both a `Some` and a `None`.
A `None` would become `WHERE "Age" = NULL`, which matches no row — including the
rows that hold `None`. That is a silent wrong answer, so the package raises an
exception naming the fix instead.

Inline the option, or query the column:

```csharp
context.People.Where(person => person.Age == Option.None<int>());
context.People.Where(person => EF.Property<int?>(person, "Age") == null);
```

### Anything else on the option still throws

`Match`, `Unwrap` and the rest do not translate. Entity Framework Core cannot
push an arbitrary call on `Option<T>` into SQL, and the rewrite covers the state
checks and comparisons rather than pretending to cover everything.

Every row of the table above, and the throw, is covered by a test in this
repository.

## Install it

```
dotnet add package Waystone.Monads.EntityFrameworkCore --prerelease
```

Targets `net8.0` and `net10.0`. It supports
`Microsoft.EntityFrameworkCore >= 8.0.11 && < 11.0.0` — bring your own version
inside that range. There is no `netstandard2.0` asset, because EF Core has none
either.

## Use it

One call in `OnModelCreating` maps every `Option<T>` property in the model,
whatever the type held:

```csharp
using Microsoft.EntityFrameworkCore;
using Waystone.Monads.Options;

public sealed class AppDbContext : DbContext
{
    public DbSet<Person> People => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseWaystoneOptionConversions();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseWaystoneOptionQueries();
    }
}

public sealed class Person
{
    public int Id { get; set; }
    public Option<string> Nickname { get; set; } = Option.None<string>();
    public Option<int> Age { get; set; } = Option.None<int>();
}
```

That produces two nullable columns of the held types:

```sql
CREATE TABLE "People" (
    "Id"       INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "Nickname" TEXT    NULL,
    "Age"      INTEGER NULL
);
```

Call the sweep last. It reads the entity types already in the model, so anything
you configure by hand should come first. A property you have already given a
converter is left alone.

## Where the types live

The package shadows Entity Framework Core's own namespaces, so its types sit
where you already look for them.

| Member | Namespace |
| --- | --- |
| `UseWaystoneOptionConversions`, `UseWaystoneOptionQueries` | `Microsoft.EntityFrameworkCore` |
| `ReferenceTypeOptionConverter<T>`, `ValueTypeOptionConverter<T>` | `Microsoft.EntityFrameworkCore.Storage.ValueConversion` |
| `OptionValueComparer<T>` | `Microsoft.EntityFrameworkCore.ChangeTracking` |

The package and assembly are still called
`Waystone.Monads.EntityFrameworkCore`. Only the namespaces shadow.

## Registering one property by hand

Use this when a property needs a column name or a provider type the sweep would
not give it:

```csharp
modelBuilder.Entity<Person>()
            .Property(person => person.Nickname)
            .HasConversion(
                new ReferenceTypeOptionConverter<string>(),
                new OptionValueComparer<string>())
            .IsRequired(false)
            .HasColumnName("nick");

modelBuilder.Entity<Person>()
            .Property(person => person.Age)
            .HasConversion(
                new ValueTypeOptionConverter<int>(),
                new OptionValueComparer<int>())
            .IsRequired(false);
```

Three things the sweep does for you and a hand-written registration must not
forget.

**Pick the converter by the held type.** A reference type takes
`ReferenceTypeOptionConverter<T>`, a value type takes
`ValueTypeOptionConverter<T>`. They are separate classes because C# resolves
`T?` under a `notnull` constraint to `T` rather than to `Nullable<T>`, so one
class would hand `Option<int>` a non-nullable column and write `None` as `0`.
Picking the wrong one is a compile error, so this is a nuisance rather than a
trap.

**Call `IsRequired(false)`.** The model property is a non-nullable reference
type, so without it the provider emits `NOT NULL` and saving a `None` fails at
the database.

**Pass the comparer.** Without it EF Core builds its own, which is not guaranteed
to reach the record equality `Option<T>` already has — a property reassigned from
`Some(1)` to `Some(2)` can then go unnoticed and never reach the database. One
comparer class covers both reference and value types.

## `Some(0)` and `Some("")` are not `None`

A default value is stored and read back as a `Some`:

| Property value | Column | Reads back as |
| --- | --- | --- |
| `Option.Some(31)` | `31` | `Option.Some(31)` |
| `Option.Some(0)` | `0` | `Option.Some(0)` |
| `Option.Some("")` | `''` | `Option.Some("")` |
| `Option.None<int>()` | `NULL` | `Option.None<int>()` |
| `null` | `NULL` | `Option.None<int>()` |

The last row is a convenience. A property left uninitialised is `null`, not
`None`, and rather than throwing on it the converters treat it as a `None`.
Initialise your properties anyway — the CLR default of `Option<T>` is `null`, and
only the database round trip repairs it.

## A nullable option is not supported

`Option<T>?` has no representation here. The column has one `NULL` and the
property wants two absent states, and there is no way to tell "no row value"
from `None`. If you have that shape, do not use `Option<T>` for the property.

## `Result<TOk, TErr>` is out of scope

There is no converter for `Result<TOk, TErr>` and there is not going to be one.
`TOk` and `TErr` are different, usually unrelated types, so a single column
cannot hold both — it would need a discriminator plus two nullable columns, which
is a complex-type mapping with per-entity decisions, not a value converter.

A result describes the outcome of an operation rather than persisted state. If
you want to store one, map explicit columns and build the `Result` in your own
code.
