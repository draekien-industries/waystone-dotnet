---
description: Unwrap with intent
---

# Expect

The `Expect` method is used to extract the value from an `Option<T>` when you're certain it contains a value. If the option is actually `None`, it will throw a meaningful exception with your provided message.

## Summary

Returns the contained `Some<T>` value, consuming the `Option<T>`. If the `Option<T>` is a `None<T>`, it throws an `UnmetExpectationException` with the provided custom message.

{% tabs %}
{% tab title="Option<T>" %}
```csharp
public abstract T Expect(string message);
```
{% endtab %}

{% tab title="Some<T>" %}
```csharp
public override T Expect(string message) => Value;
```
{% endtab %}

{% tab title="None<T>" %}
```csharp
public override T Expect(string message) 
    => throw new UnmetExpectationException(message);
```
{% endtab %}
{% endtabs %}

## When to Use

Use `Expect()` when you:

* are confident the `Option<T>` is a `Some<T>`
* want to fail fast and clearly when the expectation isn't met
* need to provide a contextual error message to help debug failure

This method is useful in scenarios where an absent value indicates a logic error or misuse of the API - not a runtime condition to recover from.

{% hint style="warning" %}
Throws `UnmetExpectationException` when the `Option<T>` is `None`. The exception includes the message passed to `Expect()`
{% endhint %}

## Example

```csharp
Option<string> username = GetCurrentUsername();
string name = username.Expect("Expected a username, but none was found.");
```

* If `username` is `Some("alice")`, then `name` is `"alice"`
* If `username` is `None<string>` , an `UnmetExpectationException` is thrown with the message `"Expected a username, but none was found."`

## Use with Care

`Expect()` is a powerful but non-defensive operation. If you're handling real-world data where absence is normal (e.g. user input, file reads, etc) consider using:

* [`Match()`](match.md): to handle both `Some` and `None` explicitly
* [`UnwrapOr()`](unwrap.md): to provide a fallback value
* `IsSome/IsNone`: to inspect before unwrapping
