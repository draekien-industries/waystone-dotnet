# Map

`Map` and its siblings (`MapOr`, `MapOrElse`) are the core transformation tools in the `Option<T>` toolkit. They let you apply logic only if there is a value and avoid touching anything if the option is `None`.

Think of them as the monadic version of `Select`, but with strict null safety and predictable fallback behaviour.

## Usage

Use `Map` when you want to apply a function to the inner value if it exists and get back a new `Option<U>`

```csharp
Option<string> maybeName = user.MiddleName;
Option<int> maybeLength = maybeName.Map(name => name.Length);
```

If the original option is `None`, `Map` returns `None<U>` automatically.

{% hint style="info" %}
This is the bread-and-butter of functional transformation. You are not unwrapping or mutating - just safely mapping values across a pipeline.
{% endhint %}

## Summary

Use `Map` when you're building transformation pipelines that depend on values being present.&#x20;

