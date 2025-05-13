# Configuration

Configuration for the library is done via the `MonadsGlobalConfig` static class. It currently provides the option for configuring a global exception logger which will be invoked whenever an exception is caught and handled by the library.

Invoke this configuration _once_ in the startup of your project.

```csharp
MonadsGlobalConfig.UseExceptionLogger((ex) => {
    Console.WriteLine(ex); // replace with your logger's log method, e.g. serilog
});
```
