---
icon: binary
---

# Result

## What is a Result?

The `Result<T, E>` type represents a computation that can either succeed with a value - `Ok<T, E>` - or fail with an error - `Err<T, E>`. Where [`Option<T>`](option.md) models absence, `Result<T, E>` models failure. It is how functional programming encodes errors into the type system itself, rather than throwing them into the runtime.

{% hint style="info" %}
`Result<T, E>` is equivalent to `Either<E, T>` in some functional languages, but it's flipped in C# to put the success case first for readability
{% endhint %}

## The Exception Problem

In traditional object-oriented code, exceptions are your go-to mechanism for error handling. But exceptions are:

* Invisible in function signatures
* Easy to forget to handle
* Hard to compose
* Catastrophic in chains

They're basically control flow with a bomb strapped to it. If a method throws, anything down the chain is at risk, and you can't tell which methods do or don't throw unless you read the source or the documentation.

{% hint style="success" %}
`Result<T, E>` bakes error handling into the type system
{% endhint %}

## Functional Error Handling

With `Result<T, E>`, every operation explicitly returns either sucecss or failure. That's not just more honest - it's more composable:

* You can `Map` over the success value without touching the error
* You can `AndThen` into further computations that also return `Result`
* &#x20;You can `Match` or `Inspect` the outcome at the edge of your program - not the middle

```csharp
Result<User, Error> GetUser(string id);
Result<Address, Error> GetAddress(User user);
Result<string, Error> FormatAddress(Address address);

var result = GetUser("Keyleth")
    .AndThen(GetAddress)
    .AndThen(FormatAddress);
```

If any step fails, the whole chain fails, and the error is carried forward untouched. No `try/catch`. No special cases. Just clean, predictable control flow.

{% hint style="info" %}
This behaviour is often described as railway-oriented programming. Your computation follows one of two tracks: success or failure.
{% endhint %}

## Intentional Errors

Unlike `Option<T>`, which represents uncertainty, `Result<T, E>` represents an expected failure mode. You're not just saying "this might not exist", you're saying "this might go wrong, and here's what it looks like if it does".

```csharp
Result<User, Error> TryCreateUser(string input);
```

This makes it ideal for parsing, validation, and domain logic. You're not just opting out of exceptions - you're describing your domain more accurately.

{% hint style="info" %}
If you're writing `try/catch` just to return a fallback value or log an error, you probably want `Result<T, E>` instead
{% endhint %}

## When to Use Result

Reach for `Result<T, E>` when:

* A function might fail and you want to make that explicit
* You care about the reason for the failure
* You want to chain operations but bail early on error
* You're validating, parsing, or transforming user input

And expecially when you want the caller to handle the failure case - not just log it and pray.

## TL;DR

`Result<T, E>` is a better model for failure than exceptions. It gives you structured, type-safe, and composable control flow. Instead of blowing up at runtime, your failures travel through your system like first-class citizens.
