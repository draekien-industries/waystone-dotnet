# Option\<T>

{% hint style="info" %}
Learn more about the `Option` type before you get started: [Core Concepts - Option](../../core-concepts/option.md)
{% endhint %}

## Creating an Option

You can create an `Option<T>` using the static methods `Some<T>()` and `None<T>()`

{% tabs %}
{% tab title="Option.Some<T>(T value)" %}
```csharp
Option<string> maybeHello = Option.Some("Hello, World!");
```

{% hint style="warning" %}
Passing `null` or `default(T)` to `Option.Some()` will throw an `InvalidOperationException`. To represent an absent value, use [`Option.None<T>()`](./#none)&#x20;
{% endhint %}
{% endtab %}

{% tab title="Option.None<T>()" %}
```csharp
Option<string> maybeHello = Option.None<string>();
```
{% endtab %}
{% endtabs %}

## Creating Options Safely

You can use `Bind` to safetly attempt the creation of an `Option<T>` by wrapping a potentially exception-throwing factory method

### How it works

```csharp
public static Option<T> Bind<T>(Func<T> factory, Action<Exception>? onError = null)
```

1. Bind invokes the provided factory method
2. If the factory returns successfully, the result is wrapped in a `Some`&#x20;
3. If the factory throws, the exception is caught, and a `None` is returned

{% hint style="info" %}
The optional `onError` action is invoked when an exception is thrown in the `Bind` method (if provided)
{% endhint %}

{% hint style="success" %}
This is ideal when dealing with code that may throw, but you want to handle it in a functional, null-safe way
{% endhint %}

### Example: Wrapping Unsafe Code

```csharp
var result = Option.Bind(() => File.ReadAllText("config.json")
                         ex => logger.LogError(ex, "Failed to read config file"));
```

* If the file is read successfully, `result` will be a `Some`
* If it throws (e.g. file not found), the `result` will be a `None` and the exception logged
