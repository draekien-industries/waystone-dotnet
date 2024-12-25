# Linq Adjacent Extensions for Options Collections

Some extension methods are provided in order to simplify working with options
collections.

> [!NOTE]
>
> Some alternative names were chosen in order to avoid conflicting with actual
> `System.Linq` methods.

## Filter

Apply a predicate to a collection of option types. Any option that does not
match the predicate will be transformed into a `None<T>`.

> [!WARNING]
>
> Unlike Linq's `Where` method, applying a `Filter` does not remove any elements
> that fail to pass the predicate

### Examples

```csharp
List<Option<int>> options = [
        Option.Some(1),
        Option.Some(2),
        Option.Some(3),
        Option.Some(4)
    ];

var filteredOptions = options.Filter(x => x > 2);

Debug.Assert(filteredOptions == [
        Option.None<int>(),
        Option.None<int>(),
        Option.Some(3),
        Option.Some(4)
    ]);
```

## First

Use one of the below methods to get the first element matching a predicate

### FirstOrNone

Gets the first `Some<T>` matching the predicate. Returns a `None<T>` if there
are no matches.

#### Examples

```csharp
List<Option<int>> options = [
        Option.Some(1),
        Option.Some(2),
        Option.Some(3),
        Option.Some(4)
    ];

Option<int> result = options.FirstOrNone(x => x > 2);
Debug.Assert(result == Option.Some(3));

Option<int> result2 = options.FirstOrNone(x => x > 4);
Debug.Assert(result2 == Option.None<int>());
```

### FirstOr

Gets the first element matching the predicate. Returns the provided default
value if there are no matches.

#### Examples

```csharp
List<Option<int>> options = [
        Option.Some(1),
        Option.Some(2),
        Option.Some(3),
        Option.Some(4)
    ];

int result = options.FirstOr(x => x > 2, 5);
Debug.Assert(result == 3);

int result2 = options.FirstOr(x => x > 4, 5);
Debug.Assert(result2 == 5);
```

### FirstOrElse

Gets the first element matching the predicate. Executes the provided delegate to
create and return a default value if there are no matches.

#### Examples

```csharp
List<Option<int>> options = [
        Option.Some(1),
        Option.Some(2),
        Option.Some(3),
        Option.Some(4)
    ];

int result = options.FirstOrElse(x => x > 2, () => 5);
Debug.Assert(result == 3);

int result2 = options.FirstOrElse(x => x > 4, () => 5);
Debug.Assert(result2 == 5);
```

## Last

Use one of the below methods to get the last element matching a predicate

### LastOrNone

Gets the last `Some<T>` matching the predicate. Returns a `None<T>` if there
are no matches.

#### Examples

```csharp
List<Option<int>> options = [
        Option.Some(1),
        Option.Some(2),
        Option.Some(3),
        Option.Some(4)
    ];

Option<int> result = options.LastOrNone(x => x > 2);
Debug.Assert(result == Option.Some(4));

Option<int> result2 = options.LastOrNone(x => x > 4);
Debug.Assert(result2 == Option.None<int>());
```

### LastOr

Gets the last element matching the predicate. Returns the provided default
value if there are no matches.

#### Examples

```csharp
List<Option<int>> options = [
        Option.Some(1),
        Option.Some(2),
        Option.Some(3),
        Option.Some(4)
    ];

int result = options.LastOr(x => x > 2, 5);
Debug.Assert(result == 4);

int result2 = options.LastOr(x => x > 4, 5);
Debug.Assert(result2 == 5);
```

### LastOrElse

Gets the last element matching the predicate. Executes the provided delegate to
create and return a default value if there are no matches.

#### Examples

```csharp
List<Option<int>> options = [
        Option.Some(1),
        Option.Some(2),
        Option.Some(3),
        Option.Some(4)
    ];

int result = options.LastOrElse(x => x > 2, () => 5);
Debug.Assert(result == 3);

int result2 = options.LastOrElse(x => x > 4, () => 5);
Debug.Assert(result2 == 5);
```
