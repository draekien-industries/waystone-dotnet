---
icon: binary
---

# Result

## Overview

The `Result` type represents a success or failure. Every `Result` is either an

* `Ok` - contains a success value
* `Err` - contains a failure value

They provide a powerful alternative to C# exceptions:

* errors are explicit, instead of exceptional
* no exceptions for expected failures
* safe and composable error handling
* centralised error handling

{% hint style="info" %}
The `Result` type enables support for [recoverable failures](result.md#recoverable-failures)
{% endhint %}

***

### Recoverable Failures

{% hint style="success" %}
Sometimes things go wrong, but they don't need to crash the program. You just want to know what wwent wrong and handle it
{% endhint %}

You go to an ATM to withdraw cash. If you type the wrong PIN, the machine doesn't explode - it just tells you the PIN is incorrect, and you can try again. That's a _recoverable failure_.

{% hint style="warning" %}
Throwing exceptions for every error (like a wrong PIN) is overkill and can clutter your logic
{% endhint %}

{% hint style="success" %}
The `Result` monad lets you return either a successful result or an error in a controlled way
{% endhint %}

```csharp
public Result<decimal, string> Withdraw(Account account, decimal amount)
{
    if (amount > amount.Balance)
        return Result.Err<decimal, string>("Insufficient funds");
    
    account.Balance -= amount;
    return Result.Ok<decimal, string>(account.Balance);
}
```

Instead of catching exception or checking for `null`, the caller _must_ handle both the `Ok` and `Err` cases - making the failure part of the design, not a suprise.

```csharp
Result<decimal, string> withdrawResult = Withdraw(account, 100.00);
string response = withdrawResult.Match(
    ok => $"Withdraw successful: Your remaining balance is ${account.Balance}",
    err => $"Withdraw failed: {err}",
);
```

## Benefits

The `Result` type models computations that can succeed or fail in a type-safe, composable way. Instead of using exceptions for control flow or returning ambiguous values like `null`, `Result` allows you to explicitly handle success and failure as part of your function's signature.

### Errors are Explicit, not Exceptional

In traditional C#, functions throw exceptions when an error occurs:

```csharp
string ReadFile(string path) => File.ReadAllText(path); // may throw
```

{% hint style="warning" %}
The method signature does not indicate the potential failures. You need to rely on documentation, or get surprised at runtime
{% endhint %}

With `Result`, you now know that an error is possible

```csharp
record Error(string Message);

Result<string, Error> ReadFile(string path)
    => Result.Bind(() => File.ReadAllText(path), ex => new Error(ex.Message));
```

{% hint style="success" %}
The function now clearly communicates that it may fail, and what kind of error you can expect (`Error` in this case).
{% endhint %}

{% hint style="success" %}
This makes your code self-documenting and safer
{% endhint %}

### No Exceptions for Expected Failures

Exceptions are great for _unexpected_ errors, but when failure is expected (like validation errors) throwing them leads to:

* Verbose `try/catch` blocks
* Poor performance in hot paths
* Hidden control flow

The `Result` type gives you a clean, functional alternative:

```csharp
Result<Guid, Error> ParseGuid(string input)
{
    return Guid.TryParse(input, out var guid)
        ? Result.Ok<Guid, Error>(guid)
        : Result.Err<Guid, Error>(new Error($"Input '{input}' is not a Guid"));
}
```

{% hint style="success" %}
Failure is just another branch of logic, not an exceptional event
{% endhint %}

### Safe and Composable Error Handling

There is no need to manually check errors at each step when chaining operations. Methods such as `Map`, `Bind`, `Match`, etc. allow us to do chain operations elegantly

```csharp
Result<User, Error> GetUser(string id)
Result<Profile, Error> LoadProfile(User user)

Result<string, Error> getEmailResult =
    GetUser(id)                     // Result<User, Error>
    .Map(user => LoadProfile(user)) // Result<Result<Profile, Error>, Error>
    .Flatten()                      // Result<Profile, Error>
    .Map(profile => profile.Email); // Result<string, Error>
```

{% hint style="success" %}
If _any_ function fails, the entire chain short-circuits to an `Err`, safely and predictably
{% endhint %}

### Centralised Error Handling

With `Result`, you can centralise how you handle failures instead of sprinking `try/catch` everywhere

```csharp
class UserNotFoundError : Error;
class ProfileNotFoundError : Error;

Result<User, Error> GetUser(string id)
Result<Profile, Error> LoadProfile(User user)

IActionResult apiResponse =
    GetUser(id)                     // Result<User, Error>
    .Map(user => LoadProfile(user)) // Result<Result<Profile, Error>, Error>
    .Flatten()                      // Result<Profile, Error>
    .Map(profile => profile.Email)  // Result<string, Error>
    .Match(
        ok => Ok(new { Email = ok }),
        err => err switch {
            UserNotFoundError _ => NotFound("User not found"),
            ProfileNotFoundError _ => NotFound("Profile not found"),
            _ => ServerError("Something went wrong")
        }
    );
```

{% hint style="success" %}
Error handling is now declarative and deeply nested `if` or exception logic is avoided
{% endhint %}

### Improves Testability and Predictability

Because `Result<T, E>` is just data (not a control-flow mechanism like exceptions), it's easier to test, log, and inspect. No need for `Assert.Throw`

```csharp
Result<decimal, string> CalculateDiscountedPrice(decimal price, decimal discount)

var result = CalculateDiscountedPrice(100, 1.5m);

Assert.True(result.IsErr);
Assert.Equal("Invalid discount", result.UnwrapErr());
```

## Summary

Using the `Result` monad helps you:

* Make failures explicit
* Avoid hidden exceptions and runtime `null` values
* Build safer, more readable pipelines
* Model errors in a meaningful way
* Write predictable, testable code
