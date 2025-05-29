# Configuration

Configuration for the library is done via the `MonadsGlobalConfig` static class. Invoke this configuration _once_ in the lifecycle of your project.

## Logging

There are several places in this library where exceptions are silently handled and transformed into non-throwing types. You can configure a custom logging action to inspect these exceptions as they are handled by the library.

```csharp
MonadsGlobalConfig.UseExceptionLogger((ex) => {
    Console.WriteLine(ex); // replace with your logger's log method
});
```
