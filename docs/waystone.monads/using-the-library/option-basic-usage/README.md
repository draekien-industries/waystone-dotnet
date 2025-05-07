# Option: Basic Usage

This page walks through how to actually use the `Option<T>` type provided by Waystone.Monads in your own C# code. If you're already familiar with the theoretical side (see [Core Concepts: Option](../../core-concepts/option.md)), this guide is all about getting things done.

## Creating

You can create an `Option<T>` using the static methods found in the `Option` class.

```csharp
Option<string> some = Option.Some("Caleb");
Option<string> none = Option.None<string>();
```

If you're working with code that might throw, wrap it with `Option.Try`.

```csharp
Option<string> maybeContent = Option.Try(() => File.ReadAllText("episode_1.txt"));
```

The `Try` method automatically logs exceptions (if configured) and returns `None` on failure.

{% hint style="info" %}
If you care about the exceptions that are thrown, reach for [`Result<T, E>`](../../result-less-than-tok-terr-greater-than.md) instead.
{% endhint %}

## Matching

You can pattern match on the contents of an `Option<T>` using `Match`.

```csharp
string result = maybeContent.Match(
    some: content => $"Read: {content}",
    none: () => "No content available"
);
```

{% hint style="success" %}
`Match` forces you to handle both cases.
{% endhint %}

## Mapping

Use `Map` to transform the value if it exists.

```csharp
Option<int> maybeLength = maybeContent.Map(s => s.Length);
```

If `maybeContent` is `None`, the result is still `None`. No exceptions, no nulls, just predictable flow.

## Flat Mapping (aka Bind)

Use `FlatMap` when the function you're calling already returns an `Option<T>`.

```csharp
Option<string> maybeDomain = maybeUser
    .FlatMap(user => user.Email)
    .FlatMap(ParseDomain);

Option<string> ParseDomain(string email) =>
    email.Contains("@")
        ? Option.Some(email.Split('@')[1])
        : Option.None<string>();
```

{% hint style="success" %}
This is how you build functional pipelines that short-circuit on missing data
{% endhint %}

## Inspecting

If you just want to peek inside for side effects (like logging or metrics), use `Inspect`.

```csharp
maybeDomain.Inspect(domain => Console.WriteLine($"Email domain: {domain}"));
```

This won't change the value, and it won't run if the option is `None`.

## Fallbacks

You can provide fallback values using methods like `UnwrapOr` and `OrElse`.

```csharp
Option<string> fallback = maybeContent.OrElse(() => Option.Some("default"));
string content = maybeContent.UnwrapOr("default value");
```

Use these when you're ok with a default value but still want to treat the absence as a first-class concern.

## Summary

If you're sick of `null`, `Option<T>` is your upgrade path. It lets you model data explicitly, write clean pipelines, and avoid defensive code. This page showed basic examples on how to create, transform, inspect, and consume options.

Continue reading to deep dive into each operation, starting with [Match](match.md).
