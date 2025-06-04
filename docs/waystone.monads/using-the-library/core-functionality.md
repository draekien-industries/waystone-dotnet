# Core Functionality

## Introduction

While `Option<T>` and `Result<T, E>` serve different purposes (absence vs. success/failure), they share a common operational model. If you are fluent with either the `Option<T>` or `Result<T, E>` already, you're 90% of the way to mastering the other.

The core APIs defined below work the same across both types, differing only in terms of semantics.

## Creation

Both `Option` and `Result` types have factory methods that allow you to create an instance of the monad in one of it's two states.

{% tabs %}
{% tab title="Option" %}
```csharp
Option<string> some = Option.Some("Hello world!");
Option<string> none = Option.None<string>();
```
{% endtab %}

{% tab title="Result" %}
```csharp
Result<int, string> ok = Result.Ok<int, string>(1);
Result<int, string> err = Result.Err<int, string>("Something went wrong...");
```
{% endtab %}
{% endtabs %}

Use `Try` to safely capture potentially exception-throwing logic inside a monadic wrapper. This is useful if you want to begin a monadic chain from a method you do not have control over.

{% tabs %}
{% tab title="Option" %}
```csharp
Option<User> maybeUser = Option.Try(() => GetCurrentUser());
```

If the `GetCurrentUser` call throws, the exception is caught and logged via your [configured exception logger](configuration.md), and you get back a `None<User>` instance.
{% endtab %}

{% tab title="Result" %}
```csharp
Result<User, string> result = Result.Try(
    onOk: () => GetCurrentUser(),
    onErr: ex => ex.Message
);
```

If the `GetCurrentUser` call throws, the exception is caught and logged via your configured exception logger, and the `onErr` delegate you provide is invoked.&#x20;

{% hint style="info" %}
The `onErr` delegate gives you a way to transform the caught exception into an error payload.
{% endhint %}
{% endtab %}
{% endtabs %}

## Transform

### Map

Use `Map` to apply a transformation to the contained value, if it is present or successful.

{% tabs %}
{% tab title="Option" %}
```csharp
Option<string> maybeName = Option.Some("John");
Option<int> maybeLength = maybeName.Map(name => name.Length);
```
{% endtab %}

{% tab title="Result" %}
```csharp
Result<string, string> nameResult = Result.Ok<string, string>("John");
Result<int, string> lengthResult =  nameResult.Map(name => name.Length);
```
{% endtab %}
{% endtabs %}

{% hint style="info" %}
`Map` returns the same monadic wrapper type - `Option<T>` stays an `Option`, and `Result<T, E>` stays a `Result`.
{% endhint %}

### Flatten

Removes a level of nesting from the monadic wrapper.

{% tabs %}
{% tab title="Option" %}
Removes one level of nesting from an `Option<Option<T>>`

```csharp
Option<Option<string>> some = Option.Some(Option.Some("John"));
Option<string> result = some.Flatten();
```
{% endtab %}

{% tab title="Result" %}
Removes one level of nesting from an `Result<Result<T, E>, E>`&#x20;

```csharp
Result<int, string> DoWork(string source);
Result<string, string> start = Result.Ok<string, string>("Hello world!");
Result<Result<int, string, string>> output= start.Map(x => DoWork(x));
Result<int, string> flattened = output.Flatten();
```
{% endtab %}
{% endtabs %}

## Consume

Use the methods defined below when you need to escape from a monadic wrapper to access a concrete output.

### Match

Use `Match` to consume the monadic wrapper when you are uncertain of the wrapper's current state. It enables you to pattern match on the outcome and apply branching logic.

{% tabs %}
{% tab title="Option" %}
```csharp
Option<string> maybeName = Option.Some("John");

int length = maybeName.Match(
    name => name.Length,
    () => 0
);
```

{% hint style="info" %}
`length` will be `0` if `maybeName` is a `None<string>`
{% endhint %}
{% endtab %}

{% tab title="Result" %}
```csharp
Result<string, string> nameResult = Result.Ok<string, string>("John");

int length = nameResult.Match(
    name => name.Length,
    _ => 0
);
```

{% hint style="info" %}
`length` will be `0` if `nameResult` is an `Err<string, string>`
{% endhint %}
{% endtab %}
{% endtabs %}

### Unwrap

Use `Unwrap` to consume the monadic wrapper when you are certain the monadic wrapper holds a value, or if you want to fail loudly if it doesn't.

{% hint style="info" %}
Avoid `Unwrap` unless you've validated the presence of a value upstream. It's an intentional point of failure, like `First` on an empty sequence.
{% endhint %}

{% tabs %}
{% tab title="Option" %}
```csharp
Option<string> maybeName = Option.Some("John");
string name = maybeName.Unwrap();
```
{% endtab %}

{% tab title="Result" %}
```csharp
Result<string, string> nameResult = Result.Ok<string, string>("John");
string name = nameResult.Unwrap();
```
{% endtab %}
{% endtabs %}

{% hint style="warning" %}
An `UnwrapException` will be thrown when the value is absent or failure.
{% endhint %}

### Expect

A sibling to `Unwrap`, but allows you to provide a meaningful error message when an exception is thrown. Use `Expect` to consume the monadic wrapper when you expect it to be in a `Some` or `Ok` state, and you want to fail loudly if it isn't.

{% hint style="info" %}
This method is useful in scenarios where an absent value indicates a logic error or misuse of the API - not a runtime condition to recover from.
{% endhint %}

{% tabs %}
{% tab title="Option" %}
```csharp
Option<string> maybeName = Option.Some("John");
string name = maybeName.Expect("Expected a name, but got nothing.");
```
{% endtab %}

{% tab title="Result" %}
```csharp
Result<string, string> nameResult = Result.Ok<string, string>("John");
string name = nameResult.Expect("Expected a name, but got an error");
```
{% endtab %}
{% endtabs %}

{% hint style="warning" %}
An `UnmetExpectationException` with your provided message will be thrown when the value is absent or failure.
{% endhint %}

## Side-Effect

Side effects allow you to conditionally access the value when it is `Some` or `Ok` and run some logic against the value without having to handle the other branch.

### Inspect

Use `Inspect` when you want to run some logic against the value inside the monadic wrapper when it is in it's `Some` or `Ok` state without transforming the value inside. The most common use case for `Inspect` is to inspect the value inside the wrapper and log it's value.

{% hint style="info" %}
Reach for `Map` instead if you need to transform the contained value
{% endhint %}

{% tabs %}
{% tab title="Option" %}
```csharp
Option<string> maybeName = Option.Some("John");
maybeName.Inspect(name => Console.WriteLine(name.Length));
```
{% endtab %}

{% tab title="Result" %}
```csharp
Result<string, string> nameResult = Result.Ok<string, string>("John");
nameResult.Inspect(name => Console.WriteLine(name.Length));
```
{% endtab %}
{% endtabs %}

## Conversion

### Transpose
