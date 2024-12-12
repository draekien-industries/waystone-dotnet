# The Result Type

A `Result` can be in either an `Ok` state or an `Err` state.

## Creating a `Result`

To create a `Result`, use one of the available factory methods located under the
`Result` static class.

> [!NOTE]
>
> You must specify both the `Ok` and `Err` value type params when creating a
> result because it needs to know what its opposing value type would have been.

#### Examples

```csharp
Result<int, string> ok = Result.Ok<int, string>(1);
Result<int, string> err = Result.Err<int, string>("error");
```

### Bind

The `Bind` method allows you to convert the return value of a function into an
`Result` type. It will execute the factory you provide inside a `try catch`
block, and provides a callback function parameter where you can map any
exception thrown into an error value of your choice.

#### Examples

```csharp
Result<int, string> ok = Result.Bind<int, string>(() => 10, ex => "error");
Debug.Assert(ok == Result.Ok<int, string>(10));

Result<int, string> err = Result.Bind<int, string>(() => throw new ExampleException(), ex => "error");
Debug.Assert(err == Result.Err<int, string>("error");
```

## Accessing the value of a `Result`

### Match

The `Match` method allows you to handle both states of a `Result` by providing
callbacks for each available branch - an `onOk` callback and an `onErr`
callback. This forces you to handle both scenarios in your code.

#### Examples

```csharp
Result<int, string> ok = Result.Ok<int, string>(1);

int result = ok.Match(x => x + 1, e => e.Length());

Debug.Assert(result == 2);
```

```csharp
Result<int, string> err = Result.Err<int, string>("value");

int result = err.Match(x => x + 1, e => e.Length());

Debug.Assert(result == "value".Length());
```

### Unwrap

The `Unwrap` method allows you to strip the `Result` from the value without
handling the possible `Err` scenario.

> [!WARNING]
>
> Unwrapping an `Err` will throw an `UnwrapException`

It is recommended to use one of its alternatives in order to handle the `Err`
scenario:

- `UnwrapOr`
- `UnwrapOrDefault`
- `UnwrapOrElse`

#### Examples

```csharp
Result<int, string> result = Result.Ok<int, string>(1);
Debug.Assert(result.Unwrap() == 1);
```

```csharp
Result<int, string> result = Result.Err<int, string>("error");
result.Unwrap(); // throws UnwrapException
```

### UnwrapOr

Similar to `Unwrap`, but allows a fallback value to be provided. This fallback
value will be returned for an `Err` instead of throwing an exception.

#### Examples

```csharp
Result<int, string> result = Result.Ok<int, string>(1);
Debug.Assert(result.UnwrapOr(10) == 1);
```

```csharp
Result<int, string> result = Result.Err<int, string>("value");
Debug.Assert(result.UnwrapOr(10) == 10);
```

### UnwrapOrDefault

Similar to `UnwrapOr`, but returns the `default` value of the `Ok`'s value type
instead of throwing an exception.

#### Examples

```csharp
Result<int, string> result = Result.Ok<int, string>(1);
Debug.Assert(result.UnwrapOrDefault() == 1);
```

```csharp
Result<int, string> result = Result.Err<int, string>("value");
Debug.Assert(result.UnwrapOrDefault() == default(int));
```

### UnwrapOrElse

Similar to `UnwrapOr`, but allows a fallback value factory to be provided. The
value returned by the fallback value factory will be returned for `Err` instead
of throwing an exception.

#### Examples

```csharp
Result<int, string> result = Result.Ok<int, string>(1);
Debug.Assert(result.UnwrapOrElse(() => 10) == 1);
```

```csharp
Result<int, string> result = Result.Err<int, string>("value");
Debug.Assert(result.UnwrapOrElse(() => 10) == 10);
```

### UnwrapErr

Similar to `Unwrap`, but for an `Err` instead of an `Ok`. Will throw an
exception if the result is not an `Err`.

#### Examples

```csharp
Result<int, string> result = Result.Ok<int, string>(1);
result.UnwrapErr(); // throws UnwrapException
```

```csharp
Result<int, string> result = Result.Err<int, string>("error");
Debug.Assert(result.UnwrapErr() == "error");
```

### Expect

Similar to `Unwrap`, but allows for a custom exception message to be provided to
the exception that is thrown for an `Err`.

> [!WARNING]
>
> Expecting a `Err` will throw an `UnmetExpectationException` with the provided
> message

#### Examples

```csharp
Result<int, string> result = Result.Err<int, string>("error");
result.Expect("Should not be error"); // throws UnmetExpectationException
```

### ExpectErr

Similar to `ExpectErr`, but allows for a custom exception message to be provided
to the exception that is thrown for an `Ok`.

> [!WARNING]
>
> Expecting a `Ok` will throw an `UnmetExpectationException` with the provided
> message

#### Examples

```csharp
Result<int, string> result = Result.Ok<int, string>(1);
result.ExpectErr("Should be error"); // throws UnmetExpectationException
```

### Inspect

Provides access to the value of `Ok` in order to run a function against it. Does
nothing when `Err`.

#### Examples

```csharp
Result<int, string> result = Result.Ok<int, string>(1);
result.Inspect(x => Console.WriteLine($"My number is {x}"));
```

### InspectErr

Provides access to the value of `Err` in order to run a function against it.
Does nothing when `Ok`.

#### Examples

```csharp
Result<int, string> result = Result.Err<int, string>("error");
result.InspectErr(x => Console.WriteLine($"My error is {x}"));
```

### Map

Maps the value of an `Ok` by applying a function to its contained value.

#### Examples

```csharp
Result<int, string> result = Result.Ok<int, string>(1);
Debug.Assert(result.Map(x => x.ToString()) == Result.Ok<string, string>("1"));
```

### MapOr

Applies a function to the value of an `Ok` and returning the output. Returns the
provided default if the result was an `Err`.

#### Examples

```csharp
Result<int, string> result = Result.Err<int, string>("error");
Debug.Assert(result.MapOr("0", x => x.ToString()) == "0");
```

### MapOrElse

Similar to `MapOr`, but accepts a fallback value factory. Returns the output of
the factory if the result was an `Err`.

#### Examples

```csharp
Result<int, string> result = Result.Err<int, string>("error");
Debug.Assert(result.MapOr(error => "0", x => x.ToString()) == "0");
```

### MapError

Same as `Map`, but for an `Err` instead of an `Ok`.

#### Examples

```csharp
Result<int, string> result = Result.Err<int, string>("error");
Debug.Assert(result.MapErr(x => x.Length()) == Result.Err<int, int>(5)); 
```

## Converting to `Option`

### GetOk

Converts an `Ok` into an `Option`. The option will be a `Some` if the result was
`Ok`, otherwise `None`.

#### Example

```csharp
Debug.Assert(Result.Ok<int, string>(1).GetOk() == Option.Some(1));
Debug.Assert(Result.Err<int, string>("error").GetOk() == Option.None<int>());
```

### GetErr

Converts an `Err` into an `Option`. The option will be a `None` if the result
was `Ok`, otherwise `Some`.

#### Example

```csharp
Debug.Assert(Result.Ok<int, string>(1).GetErr() == Option.None<string>());
Debug.Assert(Result.Err<int, string>("error").GetErr() == Option.Some("error"));
```

## Performing logical operators

### IsOkAnd

Checks to see if the current result is `Ok` and its value matches a predicate.

### IsErrAnd

Checks to see if the current result is `Err` and its value matches a predicate.

### And

Evaluates the current result against another result using an `And` logical
operator.

| X    | Y    | Result | 
|------|------|--------|
| Ok1  | Ok2  | Ok2    |
| Ok   | Err  | Err    |
| Err  | Ok   | Err    |
| Err1 | Err2 | Err1   |

#### Examples

```csharp
var x = Result.Ok<int, string>(1);
var y = Result.Err<int, string>("late error");
Debug.Assert(x.And(y) == Result.Err<int, string>("late error"));
```

```csharp
var x = Result.Err<int, string>("early error");
var y = Result.Ok<int, string>(1);
Debug.Assert(x.And(y) == Result.Err<int, string>("early error"));
```

```csharp
var x = Result.Err<int, string>("early error");
var y = Result.Err<int, string>("late error");
Debug.Assert(x.And(y) == Result.Err<int, string>("early error"));
```

```csharp
var x = Result.Ok<int, string>(1);
var y = Result.Ok<int, string>(2);
Debug.Assert(x.And(y) == Result.Ok<int, string>(2));
```

### AndThen

Same as `And`, but accepts a result factory in place of a result value as the
source of the comparison.

#### Examples

```csharp
var x = Result.Ok<int, string>(1);
var y = Result.Err<int, string>("late error");
Debug.Assert(x.AndThen(x => y) == Result.Err<int, string>("late error"));
```

```csharp
var x = Result.Err<int, string>("early error");
var y = Result.Ok<int, string>(1);
Debug.Assert(x.AndThen(x => y) == Result.Err<int, string>("early error"));
```

```csharp
var x = Result.Err<int, string>("early error");
var y = Result.Err<int, string>("late error");
Debug.Assert(x.AndThen(x => y) == Result.Err<int, string>("early error"));
```

```csharp
var x = Result.Ok<int, string>(1);
var y = Result.Ok<int, string>(2);
Debug.Assert(x.AndThen(x => y) == Result.Ok<int, string>(2));
```

### Or

Evaluates the current result against another result, returning the first one
that is an `Ok`, otherwise returns the last `Err.

#### Examples

```csharp
var x = Result.Ok<int, string>(1);
var y = Result.Err<int, string>("error");
Debug.Assert(x.Or(y) == Result.Ok<int, string>(1));
```

```csharp
var x = Result.Err<int, string>("error");
var y = Result.Ok<int, string>(1);
Debug.Assert(x.Or(y) == Result.Ok<int, string>(1));
```

```csharp
var x = Result.Err<int, string>("error 1");
var y = Result.Err<int, string>("error 2");
Debug.Assert(x.Or(y) == Result.Err<int, string>("error 2"));
```

### OrElse

Same as `Or`, but accepts a factory in place of a result value as the source of
the comparison.

#### Examples

```csharp
var x = Result.Ok<int, string>(1);
var y = Result.Err<int, string>("error");
Debug.Assert(x.OrElse(e => y) == Result.Ok<int, string>(1));
```

```csharp
var x = Result.Err<int, string>("error");
var y = Result.Ok<int, string>(1);
Debug.Assert(x.OrElse(e => y) == Result.Ok<int, string>(1));
```

```csharp
var x = Result.Err<int, string>("error 1");
var y = Result.Err<int, string>("error 2");
Debug.Assert(x.OrElse(e => y) == Result.Err<int, string>("error 2"));
```

## Additional utilities

### Flatten

Removes a layer of `Result` from a nested `Result` value. Removes one layer at a
time.

#### Examples

```csharp
Result<int, string> result = Result.Ok<int, string>(1);
Result<Result<int, string>, string> resultOfResult = result.Map(x => Result.Ok<int, string>(x + 1));
Debug.Assert(resultOfResult.Flatten() == Result.Ok<int, string>(2));
```

### Transpose

Converts a `Result` of `Option` into a `Option` of `Result`

#### Examples

```csharp
Result<IOption<int>, string> resultOfOption = Result.Ok<IOption<int>, string>(Option.Some(1));
IOption<Result<int, string>> optionOfResult = resultOfOption.Transpose();
Debug.Assert(optionOfResult == Option.Some(Result.Ok<int, string>(1));
```
