# Upgrading

## v1.x..v2.x

### Renamed `Bind` to `Try`

The `Option.Bind` and `Result.Bind` factory methods have been renamed to `Try`
to better adhere to functional programing concepts.

`Bind` is often associated with `FlatMap`, a way of composing functions together
in a pipeline. This renaming removes the confusion.

### Introduced `MonadsGlobalConfig`

This configuration allows the setting of a global error logger that will be
invoked whenever an exception is caught and handled by the library.

### Removed local error handling

Removed the local handle error callback on the `Option.Try` methods in favour of
the `MonadsGlobalConfig`.
