namespace Waystone.Monads.Options.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// The four cases are split across the two receiver shapes rather than repeated on
/// each, so the exclusivity matrix and both receivers are covered without eight
/// near-identical tests. Xor is the only member here with no lazy sibling, because
/// it cannot short-circuit: it has to look at both sides to know whether exactly
/// one is a <see cref="Some{T}" />.
/// </remarks>
[TestSubject(typeof(XorExtensions))]
public sealed class XorExtensionsTests
{
    [Fact]
    public async Task GivenSomeTaskAndNone_WhenXorAsync_ThenKeepTheSome()
    {
        Option<int> result = await Task.FromResult(Option.Some(1))
                                       .XorAsync(Option.None<int>());

        result.ShouldBeSomeValue(1);
    }

    [Fact]
    public async Task GivenSomeTaskAndSome_WhenXorAsync_ThenReturnNone()
    {
        Option<int> result = await Task.FromResult(Option.Some(1))
                                       .XorAsync(Option.Some(2));

        result.ShouldBeNone();
    }

    [Fact]
    public async Task
        GivenNoneValueTaskAndSome_WhenXorAsync_ThenReturnTheOtherOption()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .XorAsync(Option.Some(2));

        result.ShouldBeSomeValue(2);
    }

    [Fact]
    public async Task GivenNoneValueTaskAndNone_WhenXorAsync_ThenReturnNone()
    {
        Option<int> result =
            await new ValueTask<Option<int>>(Option.None<int>())
               .XorAsync(Option.None<int>());

        result.ShouldBeNone();
    }
}
