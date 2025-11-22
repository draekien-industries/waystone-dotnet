namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(AsyncControlFlowExtensions))]
public sealed class AsyncControlFlowExtensionsTests
{
    [Fact]
    public async Task GivenSome_WhenIsSomeAndWithTruePredicate_ThenReturnTrue()
    {
        Option<int> some = Option.Some(10);

        bool result = await some.IsSomeAnd(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task
        GivenSome_WhenIsSomeAndWithFalsePredicate_ThenReturnFalse()
    {
        Option<int> some = Option.Some(10);

        bool result = await some.IsSomeAnd(async value =>
        {
            await Task.Yield();

            return value < 5;
        });

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenNone_WhenIsSomeAnd_ThenReturnFalse()
    {
        Option<int> none = Option.None<int>();

        bool result = await none.IsSomeAnd(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenSome_WhenIsNoneOrWithTruePredicate_ThenReturnTrue()
    {
        Option<int> some = Option.Some(10);

        bool result = await some.IsNoneOr(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenNone_WhenIsNoneOrWithFalsePredicate_ThenReturnTrue()
    {
        Option<int> none = Option.None<int>();

        bool result = await none.IsNoneOr(async value =>
        {
            await Task.Yield();

            return value < 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenNone_WhenIsNoneOr_ThenReturnTrue()
    {
        Option<int> none = Option.None<int>();

        bool result = await none.IsNoneOr(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task
        GivenTaskSome_WhenIsSomeAndWithTruePredicate_ThenReturnTrue()
    {
        Task<Option<int>> some = Task.FromResult(Option.Some(10));

        bool result = await some.IsSomeAnd(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task
        GivenTaskSome_WhenIsSomeAndWithFalsePredicate_ThenReturnFalse()
    {
        Task<Option<int>> some = Task.FromResult(Option.Some(10));

        bool result = await some.IsSomeAnd(async value =>
        {
            await Task.Yield();

            return value < 5;
        });

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenTaskNone_WhenIsSomeAnd_ThenReturnFalse()
    {
        Task<Option<int>> none = Task.FromResult(Option.None<int>());

        bool result = await none.IsSomeAnd(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task
        GivenTaskSome_WhenIsNoneOrWithTruePredicate_ThenReturnTrue()
    {
        Task<Option<int>> some = Task.FromResult(Option.Some(10));

        bool result = await some.IsNoneOr(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task
        GivenTaskNone_WhenIsNoneOrWithFalsePredicate_ThenReturnTrue()
    {
        Task<Option<int>> none = Task.FromResult(Option.None<int>());

        bool result = await none.IsNoneOr(async value =>
        {
            await Task.Yield();

            return value < 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenTaskNone_WhenIsNoneOr_ThenReturnTrue()
    {
        Task<Option<int>> none = Task.FromResult(Option.None<int>());

        bool result = await none.IsNoneOr(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task
        GivenValueTaskSome_WhenIsSomeAndWithTruePredicate_ThenReturnTrue()
    {
        ValueTask<Option<int>> some = new(Option.Some(10));

        bool result = await some.IsSomeAnd(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task
        GivenValueTaskSome_WhenIsSomeAndWithFalsePredicate_ThenReturnFalse()
    {
        ValueTask<Option<int>> some = new(Option.Some(10));

        bool result = await some.IsSomeAnd(async value =>
        {
            await Task.Yield();

            return value < 5;
        });

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenValueTaskNone_WhenIsSomeAnd_ThenReturnFalse()
    {
        ValueTask<Option<int>> none = new(Option.None<int>());

        bool result = await none.IsSomeAnd(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task
        GivenValueTaskSome_WhenIsNoneOrWithTruePredicate_ThenReturnTrue()
    {
        ValueTask<Option<int>> some = new(Option.Some(10));

        bool result = await some.IsNoneOr(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task
        GivenValueTaskNone_WhenIsNoneOrWithFalsePredicate_ThenReturnTrue()
    {
        ValueTask<Option<int>> none = new(Option.None<int>());

        bool result = await none.IsNoneOr(async value =>
        {
            await Task.Yield();

            return value < 5;
        });

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenValueTaskNone_WhenIsNoneOr_ThenReturnTrue()
    {
        ValueTask<Option<int>> none = new(Option.None<int>());

        bool result = await none.IsNoneOr(async value =>
        {
            await Task.Yield();

            return value > 5;
        });

        result.ShouldBeTrue();
    }
}
