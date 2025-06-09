namespace Waystone.Monads.Options;

using System;
using System.Threading.Tasks;
using Extensions;
using NSubstitute;
using Shouldly;
using Xunit;

public sealed class MethodChainingTests
{
    [Fact]
    public async Task
        GivenSyncOption_AndTaskDelegates_ThenMethodsShouldAllChain()
    {
        Option<int> option = Option.Some(1);

        var inspect = Substitute.For<Func<int, Task>>();

        string result = await option.MapAsync(x => Task.FromResult(x + 1))
                                    .InspectAsync(inspect)
                                    .FlatMapAsync(x => Task.FromResult(
                                                      Option.Some(x + 1)))
                                    .FilterAsync(x => Task.FromResult(x > 0))
                                    .OrElseAsync(() => Task.FromResult(
                                                     Option.Some(0)))
                                    .MatchAsync(
                                         some => Task.FromResult(
                                             some.ToString()),
                                         () => Task.FromResult(string.Empty));

        result.ShouldBe("3");
        await inspect.Received(1).Invoke(2);
    }
}
