---
icon: option
---

# Option

## Overview

The `Option` type represents an optional value. Every `Option` is ether a

* `Some` - contains a value, or
* `None` - contains no value

It provides similar functionality to C# [nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references), but provides the following benefits:

* enforces null handling, preventing accidental null reference exceptions
* all [default values](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/default-values) are treated as `None`&#x20;

{% hint style="info" %}
The `Option` type enables support for [intentional absence](option.md#intentional-absence)
{% endhint %}

***

### Intentional Absence

{% hint style="success" %}
Sometimes, not having a value is perfectly valid and expected
{% endhint %}

Imagine asking someone for their middle name. Some people just don't have one - and that's ok. The absence of a middle name is not an error. It's simply an intentional lack of data.

{% hint style="warning" %}
Returning `null` can mean many things

* a mistake
* a missing value
* an uninitialized field
{% endhint %}

{% hint style="success" %}
The `Option<T>` monad makes the absence explicit and intentional
{% endhint %}

```csharp
class User
{
    public User(string firstName, string? middleName, string lastName)
    {
        FirstName = firstName;
        MiddleName = string.IsNullOrWhiteSpace(middleName)
            ? Option.Some(middleName)
            : Option.None<string>();
        LastName = lastName;
    }

    public string FirstName { get; }
    public Option<string> MiddleName { get; set; }
    public string LastName { get; }
}
```

Now, it's clear to anyone accessing the `MiddleName` property that the result may _intentionally_ be absent - and they must handle both cases. For instance if we want to build a `FullName` from a `User`:

```csharp
class User
{
    public string FullName => MiddleName.Match(
        some => $"{FirstName} {MiddleName} {LastName}",
        none => $"{FirstName} {LastName}"
    );
}
```

## Benefits

The `Option` type replaces the ambiguous uses of `null` with an explicit, type-safe alternative that makes the possibility of missing data part of the type system, not a hidden runtime hazard.

### Makes Absence Explicit

```csharp
string? GetMiddleName(User user)
```

{% hint style="warning" %}
The caller must guess if `null` means an error, a valid absence, or an accidental bug
{% endhint %}

With the `Option` type, it becomes clear that the function may or may not return a value, and the caller is required to handle both cases. This leads to more robust, self-documenting code.

```csharp
Option<string> GetMiddleName(User user)
{
    return string.IsNullOrWhiteSpace(user.MiddleName)
        ? Option.Some(user.MiddleName)
        : Option.None<string>();
}
```

### Avoid Null Reference Exceptions

By design, the `Option` type avoid the most common cause of bugs in C#: derefencing `null`

```csharp
Option<User> GetUser(string id);

Option<string> email = GetUser(id)
    .Map(u => u.Email)
    .Map(e => e.ToLowerInvariant());
```

{% hint style="success" %}
This chain of operations is safe: if any step results in `None`, the entire pipeline short-circuits and remains `None` . You get null safety _without_ null checks.
{% endhint %}

