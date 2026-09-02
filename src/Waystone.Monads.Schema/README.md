# Waystone.Monads.Schema

Composable schema parsing for [Waystone.Monads](https://www.nuget.org/packages/Waystone.Monads).

Declare a check once, compose it into a schema, and parse an untrusted input into a
validated domain type. The result is a `Result<TOut, SchemaViolation>`, and every
failure is reported at once rather than one at a time.

```csharp
public partial class OrderSchema : Schema<OrderDto, Order>
{
    static readonly Schema<string, EmailAddress> Email =
        Schema.Text.Trim().NotEmpty().Transform(EmailAddress.Create);

    protected override Result<Order, SchemaViolation> Configure(OrderDto subject) =>
        Schema.Fields(
                  Schema.Required(subject.Email, Email),
                  Schema.Optional(subject.Nickname, Schema.Text.MaxLength(40)))
              .Into((email, nickname) => new Order { Email = email, Nickname = nickname });
}

Result<Order, SchemaViolation> order = OrderSchema.Instance.Parse(dto);
```

This parses rather than validates: what comes out is a type the caller could not
have constructed without passing.

The namespace is `Waystone.Monads.Schemas`, plural, so that the bare `Schema` type
does not lose name resolution to the namespace itself.

See the [documentation](https://draekien-industries.wpei.me) for the full surface.
