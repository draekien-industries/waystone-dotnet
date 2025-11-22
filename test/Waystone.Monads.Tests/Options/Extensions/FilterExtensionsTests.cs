namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(FilterExtensions))]
public sealed class FilterExtensionsTests
{
    [Fact]
    public async Task GivenSome_WhenFilterWithTruePredicate_ThenReturnSome()
    {
        Option<int> some = Option.Some(10);

        Option<int> result = await some.Filter(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe(10);
    }

    [Fact]
    public async Task GivenSome_WhenFilterWithFalsePredicate_ThenReturnNone()
    {
        Option<int> some = Option.Some(10);

        Option<int> result = await some.Filter(async value =>
        {
            await Task.Yield();

            return value < 5;
        });

        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenNone_WhenFilter_ThenReturnNone()
    {
        Option<int> none = Option.None<int>();

        Option<int> result = await none.Filter(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public async Task
        GivenAsyncSome_WhenFilterWithTruePredicate_ThenReturnSome()
    {
        Option<int> some = Option.Some(10);

        Option<int> result = await Task.FromResult(some)
           .Filter(async value =>
            {
                await Task.Yield();

                return value > 5;
            });

        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe(10);
    }

    [Fact]
    public async Task
        GivenAsyncSome_WhenFilterWithFalsePredicate_ThenReturnNone()
    {
        Option<int> some = Option.Some(10);

        Option<int> result = await Task.FromResult(some)
           .Filter(async value =>
            {
                await Task.Yield();

                return value < 5;
            });

        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenAsyncNone_WhenFilter_ThenReturnNone()
    {
        Option<int> none = Option.None<int>();

        Option<int> result = await Task.FromResult(none)
           .Filter(async value =>
            {
                await Task.Yield();

                return value > 5;
            });

        result.IsNone.ShouldBeTrue();
    }

    [Fact]
    public async Task
        GivenValueTaskAsyncSome_WhenFilterWithTruePredicate_ThenReturnSome()
    {
        Option<int> some = Option.Some(10);

        Option<int> result = await new ValueTask<Option<int>>(some)
           .Filter(async value =>
            {
                await Task.Yield();

                return value > 5;
            });

        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe(10);
    }

    [Fact]
    public async Task
        GivenValueTaskAsyncSome_WhenFilterWithFalsePredicate_ThenReturnNone()
    {
        Option<int> some = Option.Some(10);

        Option<int> result = await new ValueTask<Option<int>>(some)
           .Filter(async value =>
            {
                await Task.Yield();

                return value < 5;
            });

        result.IsNone.ShouldBeTrue();
    }
}
