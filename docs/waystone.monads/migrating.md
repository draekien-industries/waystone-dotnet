# Migrating

## v1.x - v2.x

### Renamed `Bind` to `Try`

The `Option.Bind` and `Result.Bind` factory methods have been renamed to `Try`to better adhere to functional programming concepts.

`Bind` is often associated with `FlatMap`, a way of composing functions together in a pipeline. This renaming removes the confusion.

```diff
-Option.Bind(() => CreateSome(), ex => Console.WriteLine(ex));
+Option.Try(() => CreateSome());

-Result.Bind(() => CreateOk(), ex => HandleEx(ex));
+Result.Try(() => CreateOk(), ex => HandleEx(ex));
```

### Introduced `MonadsGlobalConfig`

This configuration allows the setting of a global error logger that will be invoked whenever an exception is caught and handled by the library.

```csharp
MonadsGlobalConfig.UseExceptionLogger((ex) => {
    Console.WriteLine(ex); // replace with your logger's log method, e.g. serilog
});
```

### Removed local error handling

Removed the local handle error callback on the `Option.Try` methods in favour of the `MonadsGlobalConfig`.

```diff
// program.cs
+MonadsGlobalConfig.UseExceptionLogger((ex) => {
+    Console.WriteLine(ex); // replace with your logger's log method, e.g. serilog
+});

// usage
-Option.Bind(() => CreateSome(), ex => Console.WriteLine(ex));
+Option.Try(() => CreateSome());
```
