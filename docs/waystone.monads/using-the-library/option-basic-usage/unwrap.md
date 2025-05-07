# Unwrap\*

## Summary

The `Unwrap*` methods let you extract the value from an `Option<T>` in different ways, depending on whether you're willing to provide a fallback, compute one lazily, or accept an exception if the value is absent.

## Unwrap

Returns the contained `Some<T>` value, consuming the `Option<T>`. If the option is a `None`, throws an `UnwrapException`.

{% tabs %}
{% tab title="Option<T>" %}
```csharp
public abstract T Unwrap();
```
{% endtab %}

{% tab title="Some<T>" %}
```csharp
public override T Unwrap() => Value;
```
{% endtab %}

{% tab title="None<T>" %}
```csharp
public override T Unwrap()
    => throw new UnwrapException("Unwrap called for a `None` value.")
```
{% endtab %}
{% endtabs %}

### When to Use

* Only when you're confident the option contains a value
* Useful in tests or controlled contexts where failure is a bug

{% hint style="warning" %}
Throws an `UnwrapException` when the option is a `None`
{% endhint %}

### Example

```csharp
Option<string> maybeName = Option.Some("alice");
string name = maybeName.Unwrap(); // "alice"
```

If `name` is `None`, then an `UnwrapException` is thrown.

## UnwrapOr

Returns the value if `Some`, or the provided `defaultValue` if `None`.

{% tabs %}
{% tab title="Option<T>" %}
```csharp
public abstract T UnwrapOr(T defaultValue);
```
{% endtab %}

{% tab title="Some<T>" %}
```csharp
public override T UnwrapOr(T defaultValue) => Value;
```
{% endtab %}

{% tab title="None<T>" %}
```csharp
public override T UnwrapOr(T defaultValue) => defaultValue;
```
{% endtab %}
{% endtabs %}

### When to Use

* You have a simple fallback value ready

### Example

```csharp
Option<string> maybeName = Option.None<string>();
string name = maybeName.UnwrapOr("Unknown"); // "Unknown"
```

## UnwrapOrDefault

Returns the contained value, or the default of type `T` if `None`.

{% tabs %}
{% tab title="Option<T>" %}
```csharp
public abstract T? UnwrapOrDefault();
```
{% endtab %}

{% tab title="Some<T>" %}
```csharp
public override T? UnwrapOrDefault() => Value;
```
{% endtab %}

{% tab title="None<T>" %}
```csharp
public override T? UnwrapOrDefault() => default(T);
```
{% endtab %}
{% endtabs %}

### When to Use

* You want to resume using nullable reference types
* You want quick fallback without passing a value

### Example

```csharp
Option<int> number = Option.None<int>();
int value = number.UnwrapOrDefault(); //  0
```

## UnwrapOrElse

Returns the value if present, or computes a fallback using the provided function.

{% tabs %}
{% tab title="Option<T>" %}
```csharp
public abstract T UnwrapOrElse(Func<T> createElse);
```
{% endtab %}

{% tab title="Some<T>" %}
```csharp
public override T UnwrapOrElse(Func<T> createElse) => Value;
```
{% endtab %}

{% tab title="None<T>" %}
```csharp
public override T UnwrapOrElse(Func<T> createElse) => createElse.Invoke();
```
{% endtab %}
{% endtabs %}

### When to Use

* The fallback is expensive or based on runtime logic
* You want to avoid evaluating it unless necessary

### Example

```csharp
Option<string> title = Option.None<string>();
string fallback = title.UnwrapOrElse(() => GenerateTitle());
```

## Async Variants

Returns the value if present, or asynchronously computes it if absent.

```csharp
public abstract Task<T> UnwrapOrElse(Func<Task<T>> createElse);
public abstract ValueTask<T> UnwrapOrElse(Func<ValueTask<T>> createElse);
```

### When to Use

* Fallback logic involves I/O or async computation

### Example

```csharp
Option<User> user = Option.None<User>();
User result = await user.UnwrapOrElse(() => FetchGuestUserFromApi());
```

## Summary

<table><thead><tr><th width="200">Method</th><th>Use Case</th></tr></thead><tbody><tr><td><code>Unwrap</code></td><td>Panic if no value - avoid unless sure</td></tr><tr><td><code>UnwrapOr</code></td><td>Simple, static fallback</td></tr><tr><td><code>UnwrapOrDefault</code></td><td>Use <code>default(T)</code> fallback</td></tr><tr><td><code>UnwrapOrElse</code></td><td>Compute fallback only if needed</td></tr><tr><td><code>UnwrapOrElse(async)</code></td><td>Async fallback from a task or value task</td></tr></tbody></table>
