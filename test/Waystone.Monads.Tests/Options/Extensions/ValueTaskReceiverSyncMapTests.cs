namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Covers the two shapes that await a <see cref="ValueTask{TResult}" /> receiver
/// and then apply a <em>synchronous</em> map. The <c>Task</c> receiver's equivalents
/// were already exercised; these two were reachable public API with no test touching
/// them at all. The pair is easy to miss because the two receivers declare
/// identically named overloads, so a test written against <c>Task</c> compiles and
/// passes while leaving these unrun.
/// </summary>
[TestSubject(typeof(MapExtensions))]
[TestSubject(typeof(MapOrExtensions))]
public sealed class ValueTaskReceiverSyncMapTests
{
    [Fact]
    public async Task GivenSome_WhenMapAsyncWithASyncMap_ThenReturnSomeOfTheMapped()
    {
        ValueTask<Option<int>> some = new ValueTask<Option<int>>(Option.Some(1));

        Option<string> result = await some.MapAsync(value => $"n{value}");

        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe("n1");
    }

    [Fact]
    public async Task GivenNone_WhenMapAsyncWithASyncMap_ThenReturnNoneAndSkipTheMap()
    {
        ValueTask<Option<int>> none = new ValueTask<Option<int>>(Option.None<int>());
        var invoked = false;

        Option<string> result = await none.MapAsync(
            value =>
            {
                invoked = true;
                return $"n{value}";
            });

        result.IsNone.ShouldBeTrue();
        invoked.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenSome_WhenMapOrAsyncWithASyncMap_ThenReturnTheMappedValue()
    {
        ValueTask<Option<int>> some = new ValueTask<Option<int>>(Option.Some(1));

        int result = await some.MapOrAsync(10, value => value + 1);

        result.ShouldBe(2);
    }

    [Fact]
    public async Task GivenNone_WhenMapOrAsyncWithASyncMap_ThenReturnTheDefault()
    {
        ValueTask<Option<int>> none = new ValueTask<Option<int>>(Option.None<int>());
        var invoked = false;

        int result = await none.MapOrAsync(
            10,
            value =>
            {
                invoked = true;
                return value + 1;
            });

        result.ShouldBe(10);
        invoked.ShouldBeFalse();
    }
}
