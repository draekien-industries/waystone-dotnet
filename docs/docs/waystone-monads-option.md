# The Option Type

An `Option` can be in either a `Some` state or a `None` state.

## Creating an `Option`

To create an `Option`, use one of the available factory methods located under
the `Option` static class.

> [!NOTE]
>
> The type param is required when creating a `None` option because it needs to
> know what it's `Some` value type would have been.

> [!WARNING]
>
> Creating a `Some` with a "default" value (such as `null` for reference types
> and `default` for value types) will cause an `InvalidOperationException` to be
> thrown as a `Some` option can never contain a `default` value.

#### Examples

```csharp
Option<string> some = Option.Some("I have a value");
Option<string> none = Option.None<string>();
```

### Bind

The `Bind` method allows you to convert the return value of a function into an
`Option` type. It will execute the factory you provide inside a `try catch`
block, and provides a callback function parameter where you can handle any
exceptions thrown by the factory.

#### Examples

```csharp
Option<string> some = Option.Bind(() => "hello world!");
Option<string> none = Option.Bind(
    () => throw new ExampleException(), 
    ex => Console.WriteLine("Error thrown"));
```

## Accessing the value of an `Option`

### Match

The `Match` method allows you to handle both states of an `Option` by providing
callbacks for each available branch - an `onSome` callback and an `onNone`
callback. This forces you to handle both scenarios in your code and therefore
provides better safety than the built int nullable types in C#.

#### Examples

```csharp
Option<int> option = Option.Some(1);

int result = option.Match(x => x + 1, () => 0);

Debug.Assert(result == 2);
```

```csharp
Option<int> option = Option.None<int>();

int result = option.Match(x => x + 1, () => 0);

Debug.Assert(result == 0);
```

### Unwrap

The `Unwrap` method allows you to strip the `Option` from the value without
handling the possible `None` scenario.

> [!WARNING]
>
> Unwrapping a `None` will throw an `UnwrapException` as there is no value to
> return

It is recommended to use one of its alternatives in order to handle the `None`
scenario:

- `UnwrapOr`
- `UnwrapOrDefault`
- `UnwrapOrElse`

#### Examples

```csharp
Option<int> option = Option.Some(1);
Debug.Assert(option.Unwrap() == 1);
```

```csharp
Option<int> option = Option.None<int>();
option.Unwrap(); // throws UnwrapException
```

### UnwrapOr

Similar to `Unwrap`, but allows a fallback value to be provided. This fallback
value will be returned for a `None` instead of throwing an exception.

#### Examples

```csharp
Option<int> option = Option.Some(1);
Debug.Assert(option.UnwrapOr(10) == 1);
```

```csharp
Option<int> option = Option.None<int>();
Debug.Assert(option.UnwrapOr(10) == 10);
```

### UnwrapOrDefault

Similar to `UnwrapOr`, but returns the `default` value of the option's value
type instead of throwing an exception.

> [!NOTE]
>
> For reference types the `default` is `null` and for value types like `int` the
> default is `0`

#### Examples

```csharp
Option<int> option = Option.None<int>();
Debug.Assert(option.UnwrapOrDefault() == default(int));
```

### UnwrapOrElse

Similar to `UnwrapOr`, but allows a fallback value factory to be provided. This
value returned by the fallback value factory will be returned for a `None`
instead of throwing an exception.

#### Examples

```csharp
Option<int> option = Option.None<int>();
Debug.Assert(option.UnwrapOrElse(() => 10) == 10);
```

### Expect

Similar to `Unwrap`, but allows for a custom exception message to be provided to
the exception that is thrown for a `None`.

> [!WARNING]
>
> Expecting a `None` will throw an `UnmetExpectationException` with the provided
> message.

#### Examples

```csharp
Option<int> option = Option.None<int>();
option.Expect("Value should exist"); // throws UnmetExpectationException with message "Value should exist"
```

### Map

The `Map` method allows you to apply a function to the contained value in order
to transform its value or its type. Does nothing for a `None`.

#### Examples

```csharp
Option<int> number = Option.Some(1);
Option<string> numberAsString = number.Map(x => x.ToString());
```

### MapOr

Similar to `Map`, but allows you to provide a fallback value for when the option
is a `None`. Consumes the `Option` when invoked.

#### Examples

```csharp
Option<int> number = Option.None<int>();
string numberAsString = number.MapOr("no value", x => x.ToString());
Debug.Assert(numberAsString == "no value");
```

### MapOrElse

Similar to `MapOr`, but allows you to provide a fallback value factory. Consumes
the `Option` when invoked.

#### Examples

```csharp
Option<int> number = Option.None<int>();
string numberAsString = number.MapOrElse(() => "no value", x => x.ToString());
Debug.Assert(numberAsString == "no value");
```

### Inspect

Provides access to the value of a `Some` in order to run a function against it.
Does nothing when `None`.

#### Examples

```csharp
Option<int> number = Option.Some(1);
number.Inspect(x => Console.WriteLine($"My number is {x}"));
```

### Filter

Apply a predicate to the value of a `Some`, transforming the option to a `None`
if the predicate fails.

#### Examples

```csharp
Option<int> number = Option.Some(1);
Option<int> some = number.Filter(x => x > 0); // returns Some
Option<int> none = number.Filter(x => x < 0); // returns None
```

## Performing Logical Operators

### IsSomeAnd

Evaluates whether an option is `Some` and its value passes a predicate.

#### Examples

```csharp
Option<int> option = Option.Some(1);
Debug.Assert(option.IsSomeAnd(x => x > 0) == true);
Debug.Assert(option.IsSomeAnd(x => x < 0) == false);
```

```csharp
Option<int> option = Option.None<int>();
Debug.Assert(option.IsSomeAnd(x => x > 0) == false);
```

### IsNoneOr

Evaluates whether an option is `None` or its value passes a predicate.

#### Examples

```csharp
Option<int> option = Option.Some(1);
Debug.Assert(option.IsSomeAnd(x => x > 0) == true);
Debug.Assert(option.IsSomeAnd(x => x < 0) == false);
```

```csharp
Option<int> option = Option.None<int>();
Debug.Assert(option.IsSomeAnd(x => x > 0) == true);
Debug.Assert(option.IsSomeAnd(x => x < 0) == true);
```

### Or

Performs a logical `Or` comparison between the current option and another
option. Returns the current option if it is `Some`, otherwise returns the other
option.

#### Examples

```csharp
Option<int> option = Option.Some(1);
var result = option.Or(Option.Some(2));
Debug.Assert(result == Option.Some(1));
```

```csharp
Option<int> option = Option.None<int>();
var result = option.Or(Option.Some(2));
Debug.Assert(result == Option.Some(2));
```

```csharp
Option<int> option = Option.None<int>();
var result = option.Or(Option.None<int>());
Debug.Assert(result == Option.None<int>());
```

### OrElse

Similar to `Or`, but allows a comparison with a delegate that produces the other
option.

#### Examples

```csharp
Option<int> option = Option.Some(1);
var result = option.OrElse(() => Option.Some(2));
Debug.Assert(result == Option.Some(1));
```

```csharp
Option<int> option = Option.None<int>();
var result = option.OrElse(() => Option.Some(2));
Debug.Assert(result == Option.Some(2));
```

```csharp
Option<int> option = Option.None<int>();
var result = option.OrElse(() => Option.None<int>());
Debug.Assert(result == Option.None<int>());
```

### Xor

Performs a logical `Xor` (exclusive or) operator between the current option and
another option. Returns `Some` if exactly one option is `Some`, otherwise
returns `None`.

#### Examples

```csharp
Option<int> option = Option.Some(1);
Debug.Assert(option.Xor(Option.Some(2)) == Option.None<int>());
Debug.Assert(option.Xor(Option.None<int>()) == Option.Some(1));
```

```csharp
Option<int> option = Option.None<int>();
Debug.Assert(option.Xor(Option.Some(2)) == Option.Some(2));
Debug.Assert(option.Xor(Option.None<int>()) == Option.None<int>());
```

## Other Utilities

### Zip

Combines two options into one, combining their values into a `Tuple`. Will
return `None` if either option being zipped is `None`.

#### Examples

```csharp
Option<int> option1 = Option.Some(1);
Option<string> option2 = Option.Some("value");
Option<(int, string)> zipped = option1.Zip(option2);
Debug.Assert(zipped == Option.Some((1, "value"));
```

### Unzip

The reverse of `Zip`.

#### Examples

```csharp
Option<int> option = Option.Some((1, "value"));
(Option<int>, Option<string>) unzipped = option.Unzip();
Debug.Assert(unzipped.Item1 == Option.Some(1));
Debug.Assert(unzipped.Item2 == Option.Some("value"));
```

### Flatten

Useful for handling nested options. Removes one layer at a time.

#### Examples

```csharp
Option<int> option = Option.Some();
Option<Option<int>> optionOfOption = option.Map(x => Option.Some(x + 1));
Option<int> flattened = optionOfOption.Flatten();
```

### Transpose

Converts an `Option` of `Result` into a `Result` of `Option`.

#### Examples

```csharp
Option<IResult<int, string>> optionOfResult = Option.Some(Result.Ok<int, string>(1));
IResult<Option<int>, string> resultOfOption = optionOfResult.Transpose();
Debug.Assert(resultOfOption == Result.Ok<Option<int>, string>(Option.Some(1));
```
