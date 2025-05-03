---
icon: seal-exclamation
---

# Recoverable Failures

{% hint style="success" %}
Sometimes things go wrong, but they don't need to crash the program. You just want to know what wwent wrong and handle it
{% endhint %}

You go to an ATM ot withdraw cash. If you type the wrong PIN, the machine doesn't explode - it just tells you the PIN is incorrect, and you can try again. That's a _recoverable failure_.

{% hint style="warning" %}
Throwing exceptions for every error (like a wrong PIN) is overkill and can clutter your logic
{% endhint %}

{% hint style="success" %}
The `Result<T,E>` monad lets you return either a successful result or an error in a controlled way
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
