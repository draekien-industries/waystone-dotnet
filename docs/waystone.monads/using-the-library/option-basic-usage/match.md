# Match

The `Match` method is how you consume n `Option<T>` in Waystone.Monads. It's the equivalent of pattern matching in functional languages: it forces you to handle both the `Some` and `None` cases, explicitly and exhaustively.

Think of it like a switch statement, but safer - you can't forget to handle the empty case.

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

## Real-World Example

Imagine you're retrieving the URL of a profile image stored in a database, and you want to display a default image if one cannot be found:

```csharp
Option<string> maybeProfileImage = GetProfileImage(userId);

string imageUrl = maybeProfileImage.Match(
    some: url => url,
    none: () => "/images/default-avatar.png"
);
```

No nulls, no if/else clutter, just direct expression of fallback logic.

## Async Overloads

`Match` also has async-friendly versions that support delegates that return a `Task` or `ValueTask` for scenarios where your fallback or transform logic is asynchronous.

```csharp
Option<string> maybeToken = await TryGetSessionTokenAsync(cancellationToken);

string token = await maybeToken.Match(
    some: t => DecryptTokenAsync(t, cancellationToken),
    none: () => FetchFallbackTokenAsync(cancellationToken)
);
```

This lets you stay entirely in async land without awkward blocking or state juggling.

{% hint style="warning" %}
Do not use the `async/await` keywords in your lamdas. This stops the compiler from figuring out the correct return type and you'll have to specify the type params for `Match` yourself.
{% endhint %}

{% hint style="danger" %}
Be careful with `async` lambdas. If you forget to `await` the `Match`, you'll just get a `Task` or `ValueTask` back.
{% endhint %}

## Summary

`Match` is how you safely extract and use values inside an `Option<T>`. It forces you to handle both possible states and supports both sync and async workflows. Use it when you want to use the value inside an `Option`, not just transform or inspect it.

Next up: [Map](map.md) - how to transform an `Option<T>` without consuming it.
