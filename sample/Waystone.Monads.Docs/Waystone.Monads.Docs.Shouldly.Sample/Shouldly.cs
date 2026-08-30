using Shouldly;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Shouldly.Sample;

/// <summary>packages/shouldly.md</summary>
internal static class ShouldlyPage
{
    internal sealed record Order(int Total);

    internal static void WithoutThePackage(Result<Order, Error> result)
    {
        result.IsOk.ShouldBeTrue();
    }

    internal static void WithThePackage(Result<Order, Error> result)
    {
        result.ShouldBeOk();
    }

    internal static void UnwrapAndAssert(Result<Order, Error> result)
    {
        Order order = result.ShouldBeOk();
        order.Total.ShouldBe(42);
    }

    internal static void Chained(Result<Order, Error> result)
    {
        result.ShouldBeOk().Total.ShouldBe(42);
    }

    internal static void WithACustomMessage(Result<Order, Error> result)
    {
        result.ShouldBeOk("the seed data should have loaded");
    }
}
