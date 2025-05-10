---
description: >-
  This page walks through the common behaviours shared by the Option and Result
  types and how to use them in your C# code.
---

# Shared Functionality

While `Option<T>` and `Result<T, E>` serve different purposes, they share a common operational model. The shared methods outlined below work the same way across both types, making them reliable tools for building expressive and robust control flows.

## Configuring

Configuration for the library is done via the `MonadsGlobalConfig` static class. It currently provides the option for configuring a global exception logger which will be invoked whenever an exception is caught and handled by the library.

Invoke this configuration _once_ in the startup of your project.

```csharp
MonadsGlobalConfig.UseExceptionLogger((ex) => {
    Console.WriteLine(ex); // replace with your logger's log method, e.g. serilog
});
```

## Creating

### Try

{% hint style="info" %}
Use `Try` to safely capture potential exception throwing logic inside a monadic wrapper
{% endhint %}

{% tabs %}
{% tab title="Option" %}
```csharp
Option<User> maybeUser = Option.Try(LoadUserFromDisk);
```

A `None` will be returned in the case of an exception. If the global exception logger is configured, this exception will be surfaced there.
{% endtab %}

{% tab title="Result" %}
```csharp
Result<User, Error> result = Result.Try(
    factory: () => LoadUserFromDisk(),
    onErr: ex => new Error(ex.Message)
);
```

When trying to create a result, an `onErr` delegate must be provided. This delegate gives you a way to transform the caught exception into an error payload of your choice.
{% endtab %}
{% endtabs %}

## Transforming

### Map

{% hint style="info" %}
Use `Map` to apply a transformation to the contained value in the case of a `Some` or `Ok`
{% endhint %}

{% tabs %}
{% tab title="Option" %}
```csharp
Option<string> maybeName = Option.Some("Laura");
Option<int> maybeLength = maybeName.Map(name => name.Length);
//          ^? Some<int>(5)
```
{% endtab %}

{% tab title="Result" %}
```csharp
Result<string, string> nameResult = Result.Ok<string, string>("Bailey");
Result<int, string> lengthResult = nameResult.Map(name => name.Length);
//                  ^? Ok<int, string>(6)
```
{% endtab %}
{% endtabs %}

Both behave identically: `Map` only applies the function if there's a value inside, and propagates `None` or `Err` otherwise.

{% hint style="info" %}
`Map` returns the same container type. `Option<T>` stays an `Option`. `Result<T, E>` stays a `Result`.
{% endhint %}

## Consuming

### Match



### Unwrap



### UnwrapOr



### UnwrapOrElse
