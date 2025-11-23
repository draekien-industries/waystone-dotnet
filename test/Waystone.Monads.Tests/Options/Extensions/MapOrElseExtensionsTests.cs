namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(MapOrElseExtensions))]
public sealed class MapOrElseExtensionsTests
{
    [Fact]
    public async Task GivenTaskSome_WhenMapOrElse_ThenReturnMappedValue()
    {
        Task<Option<int>> some = Task.FromResult(Option.Some(10));

        int result = await some.MapOrElse(
            async () =>
            {
                await Task.Yield();

                return 0;
            },
            async value =>
            {
                await Task.Yield();

                return value * 2;
            });

        result.ShouldBe(20);
    }

    [Fact]
    public async Task GivenTaskNone_WhenMapOrElse_ThenReturnDefaultValue()
    {
        Task<Option<int>> none = Task.FromResult(Option.None<int>());

        int result = await none.MapOrElse(
            async () =>
            {
                await Task.Yield();

                return 0;
            },
            async value =>
            {
                await Task.Yield();

                return value * 2;
            });

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenTaskSome_WhenMapOrElse_WithSyncDefaultFunc_ThenReturnMappedValue()
    {
        Task<Option<int>> some = Task.FromResult(Option.Some(10));

        int result = await some.MapOrElse(
            () => 0,
            async value =>
            {
                await Task.Yield();

                return value * 2;
            });

        result.ShouldBe(20);
    }

    [Fact]
    public async Task
        GivenTaskNone_WhenMapOrElse_WithSyncDefaultFunc_ThenReturnDefaultValue()
    {
        Task<Option<int>> none = Task.FromResult(Option.None<int>());

        int result = await none.MapOrElse(
            () => 0,
            async value =>
            {
                await Task.Yield();

                return value * 2;
            });

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenTaskSome_WhenMapOrElse_WithSyncMapFunc_ThenReturnMappedValue()
    {
        Task<Option<int>> some = Task.FromResult(Option.Some(10));

        int result = await some.MapOrElse(
            async () =>
            {
                await Task.Yield();

                return 0;
            },
            value => value * 2);

        result.ShouldBe(20);
    }

    [Fact]
    public async Task
        GivenTaskNone_WhenMapOrElse_WithSyncMapFunc_ThenReturnDefaultValue()
    {
        Task<Option<int>> none = Task.FromResult(Option.None<int>());

        int result = await none.MapOrElse(
            async () =>
            {
                await Task.Yield();

                return 0;
            },
            value => value * 2);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenTaskSome_WhenMapOrElse_WithBothSyncFunc_ThenReturnMappedValue()
    {
        Task<Option<int>> some = Task.FromResult(Option.Some(10));

        int result = await some.MapOrElse(
            () => 0,
            value => value * 2);

        result.ShouldBe(20);
    }

    [Fact]
    public async Task
        GivenTaskNone_WhenMapOrElse_WithBothSyncFunc_ThenReturnMappedValue()
    {
        Task<Option<int>> none = Task.FromResult(Option.None<int>());

        int result = await none.MapOrElse(
            () => 0,
            value => value * 2);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task GivenValueTaskSome_WhenMapOrElse_ThenReturnMappedValue()
    {
        var some = new ValueTask<Option<int>>(Option.Some(10));

        int result = await some.MapOrElse(
            async () =>
            {
                await Task.Yield();

                return 0;
            },
            async value =>
            {
                await Task.Yield();

                return value * 2;
            });

        result.ShouldBe(20);
    }

    [Fact]
    public async Task GivenValueTaskNone_WhenMapOrElse_ThenReturnDefaultValue()
    {
        var none =
            new ValueTask<Option<int>>(Option.None<int>());

        int result = await none.MapOrElse(
            async () =>
            {
                await Task.Yield();

                return 0;
            },
            async value =>
            {
                await Task.Yield();

                return value * 2;
            });

        result.ShouldBe(0);
    }

    [Fact]
    public async Task
        GivenValueTaskSome_WhenMapOrElse_WithSyncDefaultFunc_ThenReturnMappedValue()
    {
        var some = new ValueTask<Option<int>>(Option.Some(10));

        int result = await some.MapOrElse(
            () => 0,
            async value =>
            {
                await Task.Yield();

                return value * 2;
            });

        result.ShouldBe(20);
    }
}
