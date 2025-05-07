# Match

The `Match` method is how you consume n `Option<T>` in Waystone.Monads. It's the equivalent of pattern matching in functional languages: it forces you to handle both the `Some` and `None` cases, explicitly and exhaustively.

Think of it like a switch statement, but safer - you can't forget to handle the empty case.

## Usage

```csharp
Option<string> maybeName = Option.Some("Matt");

string result = maybeName.Match(
    some: name => $"Hello, {name}!",
    none: () => "Hello stranger!"
);
```

If `maybeName` is `Some("Matt")`, the result will be `"Hello, Matt!"`. If it's `None`, the fallback function runs instead.

{% hint style="success" %}
`Match` forces exhaustiveness. There's no default or fallthrough logic like in a `switch`. This makes your intent clear and total.
{% endhint %}

## Example

Imagine you're retrieving the URL of a profile image stored in a database, and you want to display a default image if one cannot be found:

```csharp
Option<string> maybeProfileImage = GetProfileImage(userId);

string imageUrl = maybeProfileImage.Match(
    some: url => url,
    none: () => "/images/default-avatar.png"
);
```

No nulls, no if/else clutter, just direct expression of fallback logic.

## Summary

`Match` is how you safely extract and use values inside an `Option<T>`. It forces you to handle both possible states and supports both sync and async workflows. Use it when you want to use the value inside an `Option`, not just transform or inspect it.
