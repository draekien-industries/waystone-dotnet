---
icon: triangle-exclamation
---

# Intentional Absence

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
