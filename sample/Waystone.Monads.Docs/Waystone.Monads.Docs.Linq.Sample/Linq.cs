using Waystone.Monads.Linq;
using Waystone.Monads.Options;

namespace Waystone.Monads.Docs.Linq.Sample;

/// <summary>packages/linq.md</summary>
internal static class LinqPage
{
    internal sealed record Customer(Option<string> PostalAddress);

    internal sealed record Quote(decimal Amount);

    internal static Option<Quote> QuerySyntax(int id) =>
        from customer in FindCustomer(id)
        from address in customer.PostalAddress
        from rate in RateFor(address)
        select Price(customer, rate);

    internal static Option<Quote> TheSameChainByHand(int id) =>
        FindCustomer(id)
            .AndThen(customer => customer.PostalAddress
                .AndThen(address => RateFor(address)
                    .Map(rate => Price(customer, rate))));

    // The page also shows a `where` clause on a Result, to say it does not
    // compile. There is nothing to pin here: a sample that fails to build is
    // the claim itself, and this project would stop building if it were added.

    private static Option<Customer> FindCustomer(int id) =>
        Option.Some(new Customer(Option.Some("1 Example St")));

    private static Option<decimal> RateFor(string address) => Option.Some(1.5m);

    private static Quote Price(Customer customer, decimal rate) => new(rate);
}
