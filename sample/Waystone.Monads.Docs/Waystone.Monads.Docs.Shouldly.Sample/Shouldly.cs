using Shouldly;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Shouldly.Sample;

/// <summary>packages/shouldly.md</summary>
internal static class ShouldlyPage
{
    internal sealed record Quest(string Name, int GoldReward);

    internal static void WithoutThePackage(Result<Quest, Error> result)
    {
        result.IsOk.ShouldBeTrue();
    }

    internal static void WithThePackage(Result<Quest, Error> result)
    {
        result.ShouldBeOk();
    }

    internal static void UnwrapAndAssert(Result<Quest, Error> result)
    {
        Quest quest = result.ShouldBeOk();
        quest.GoldReward.ShouldBe(42);
    }

    internal static void Chained(Result<Quest, Error> result)
    {
        result.ShouldBeOk().GoldReward.ShouldBe(42);
    }

    internal static void WithACustomMessage(Result<Quest, Error> result)
    {
        result.ShouldBeOk("the quest board should have been stocked");
    }
}
