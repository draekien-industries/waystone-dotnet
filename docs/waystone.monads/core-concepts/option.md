---
icon: option
---

# Option

`Option` represents a value that might exist or might not. If the value exists, it's wrapped in `Some`, otherwise it is `None`.

{% hint style="info" %}
This concept comes from functional programming, where nulls are considered unsafe and unreliable
{% endhint %}

{% hint style="success" %}
`Option` is a structured, explicit replacement for nulls and nullable types
{% endhint %}

## Why not just use "T?" ?

C# has nullable reference types (`string?`, `object?`, etc.) and value types (`int?`, `bool?`, etc.) so it's fair to ask: "why bother with `Option`?"

Let's compare them:

| Feature                     | Nullable Types                        | Option\<T> |
| --------------------------- | ------------------------------------- | ---------- |
| Exists at runtime?          | No (only annotations and value types) | Yes        |
| Can be pattern-matched?     | No                                    | Yes        |
| Can chain operations?       | No                                    | Yes        |
| Can accidentally be null?   | Yes                                   | No         |
| Can safely enforce absence? | No                                    | Yes        |

{% hint style="warning" %}
Nullable reference types are just compile-time metadata. At runtime, `string?` is still just a `string`. You can dereference it without a warning if the compiler fails to catch the null. You can also pass `null` around silently.
{% endhint %}

{% hint style="success" %}
With `Option` , you are forced to be explicit. You can't accidentally forget to check a value because you literally can't access it unless you unwrap it.
{% endhint %}

## Anatomy of an Option

