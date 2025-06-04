# Option\<T>

## Control Flow

### IsSome / IsNone

Use `IsSome` and `IsNone` when you want to check the state of the monad and don't need to access it's value just quite yet.

```csharp
Option<string> maybeName = Option.Some("John");

maybeName.IsSome; // true
maybeName.IsNone; // false
```

{% hint style="info" %}
These are ideal for short-circuiting logic or quick guards, but avoid using them for full branching. Reach for [`Match`](../core-functionality.md#match) when both branches matter.
{% endhint %}

### IsSomeAnd

Use `IsSomeAnd` when you want to check if the `Option` is a `Some` and that the value contained inside the `Some` matches a predicate.

```csharp
Option<string> maybeName = Option.Some("John");
maybeName.IsSomeAnd(name => name.Length > 0); // true
```

### IsNoneOr

Use `IsNoneOr` when you want to check if the `Option` is a `None` or the value contained inside the `Some` matches a predicate.

```csharp
Option<string> maybeName = Option.Some("John");
maybeName.IsNoneOr(name => name.Length > 0); // true
maybeName.IsNoneOr(name => string.IsNullOrWhiteSpace(name)); // false
```

## Transform

### FlatMap

Use `FlatMap` to compose monadic pipelines where each operation might fail and return an `Option<T>` itself.  It prevents nested `Option<Option<T>>` results and keeps the pipeline flat and clean.

```csharp
Option<string> TryExtractDomain(string email);

Option<string> maybeEmail = GetEmail(userId);
Option<string> maybeDomain = maybeEmail.FlatMap(TryExtractDomain);
```

Without `FlatMap`, you'd need to `Map` and then flatten manually, or deal with nested options.

{% hint style="info" %}
`FlatMap` short-circuits: if one of the options in the chain is `None`, the subsequent functions are skipped.
{% endhint %}

### Filter

Use `Filter` to retain only the values that pass a predicate. If the value doesn't match, the result becomes a `None`.

```csharp
Option<string> maybeName = Option.Some("John");

Option<string> nonEmpty = maybeName.Filter(name => name.Length > 0); // Some("John")
Option<string> blank = maybeName.Filter(name => name.Length == 0);   //  None
```

{% hint style="info" %}
This is the clean alternative to `if`-guards. It keeps the monadic flow and makes intent obvious.
{% endhint %}

### Zip

Use `Zip` to combine two options into a single option containing both values as a tuple.

```csharp
Option<string> a = Option.Some("a");
Option<string> b = Option.Some("b");
Option<string> c = Option.None<string>();

Option<(string, string)> ab = a.Zip(b); // Some(("a", "b"))
Option<(string, string)> ac = a.Zip(c); // None
```

{% hint style="info" %}
If either `Option` being zipped is a `None`, then a `None` will be returned.
{% endhint %}

### Unzip

Reverses a `Zip`, splitting the tuple into two options.

```csharp
Option<(string, string)> some = Option.Some(("a", "b"));
Option<(string, string)> none = Option.None<(string, string)>();

(Option<string>, Option<string>) unzippedSome = some.Unzip(); // (Some("a"), Some("b"))
(Option<string>, Option<string>) unzippedNone = none.Unzip(); // (None, None)
```

## Logical Operators

Sometimes you want to combine two `Option` values using logical operators without leaving the monadic model.

### Or

`Or` returns the first `Some` value encountered in the chain.

```csharp
Option<string> first = Option.Some("John");
Option<string> second = Option.None<string>();
Option<string> fallback = Option.Some("Default");

Option<string> result = first.Or(second).Or(fallback); // Some("John")
```

{% hint style="info" %}
Use `Or` when you want a prioritized fallback chain
{% endhint %}

### OrElse

Like `Or`, but lazily evaluated. The fallback is only invoked if the preceding is `None`.

```csharp
Option<string> first = Option.None<string>();

Option<string> CreateSecond() => Option.None<string>();
Option<string> CreateFallback() => Option.Some("Default");

Option<string> result = first
    .OrElse(() => CreateSecond())
    .OrElse(() => CreateFallback()); // Some("Default")
```

### Xor

`Xor` is an exclusive-or. It returns the first `Some` value encountered in the chain if exactly one of the options is `Some`.

```csharp
Option<string> first = Option.Some("John");
Option<string> second = Option.None<string>();
Option<string> third = Option.Some("Smith");

Option<string> result = first
    .Xor(second) // Some("John")
    .Xor(third); // None
```

{% hint style="info" %}
`Xor` is niche, but useful when you're checking mutually exclusive conditions
{% endhint %}
