# FlatMap

`FlatMap` (otherwise known as `Bind` in classic functional programming) lets you chain together operations that themselves return `Option<T>`. It prevents nested `Option<Option<T>>` results and keeps the pipeline flat and clean.

If `Map` is about transforming a value, `FlatMap` is about transforming the entire computation. It only proceeds if the source option is `Some`, and expectes the mapping function to return another `Option<U>`.

## Usage

Use `FlatMap` when your transformation returns an `Option`

```csharp
Option<string> TryExtractDomain(string email);

Option<string> maybeEmail = GetEmail(userId);
Option<string> maybeDomain = maybeEmail.FlatMap(TryExtractDomain);
```

Here, `TryExtractDomain` returns an `Option<string>`. If the input is `None`, the entire expression short-circuits to `None`.

{% hint style="info" %}
Without `FlatMap`, you'd get `Option<Option<string>>`, which is awkward to unwrap and reason about.
{% endhint %}

## Example

Let's say you're buliding a user setting loader that depends on optional user authentication and optional user profile data.

```csharp
Option<Guid> maybeUserId = TryGetAuthenticatedUserId();

Option<UserSettings> maybeSettings = 
    maybeUserId.FlatMap(userId => LoadUserProfile(userId)
               .FlatMap(profile => FetchUserSettings(profile.SettingsId)));
```

Each function in the chain returns an `Option`, and `FlatMap` composes them without blowing up the nesting. You don't need to manually check for presence at each step - the monad takes care of it.

Compare that to imperative C#:

```csharp
if (TryGetAuthenticatedUserId() is { } userId)
{
    var profile = LoadUserProfile(userId);
    if (profile is not null)
    {
        return FetchUserSettings(profile.SettingsId);
    }
}

return null;
```

{% hint style="success" %}
`FlatMap` abstracts all of this branching away
{% endhint %}

## Summary

Reach for `FlatMap` when you have a chain of computations that can each fail or return nothing. It avoids nested options, eliminates boilerplate, and makes failure propagation automatic.
