namespace Waystone.Monads.Extensions;

using Options;
using Options.Extensions;
using Results;
using Results.Extensions;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// These twenty-two overloads exist only because DRA-110 converted the last
/// hand-written families to generated awaited receivers, and the generator lifts
/// every overload of the core member rather than the subset that happened to be
/// written by hand. The hand-written blocks lifted the plain overload and never
/// the state one, so a caller on an awaited receiver had no way to avoid a
/// closure.
/// <para>
/// Every delegate here is stored in a field rather than written as a lambda, for
/// the same reason <c>StateOverloadResolutionTests</c> does it: a lambda lets the
/// compiler disambiguate on inferred arity, so it would bind even if the
/// forwarder passed the state in the wrong position.
/// </para>
/// </remarks>
public sealed class AwaitedStateOverloadTests
{
    private const int State = 7;

    private static readonly Func<int, int, bool> Exceeds = (value, state) =>
        value > state;

    private static readonly Func<int, string> ErrorFrom = state =>
        $"error {state}";

    private static readonly Func<int, Option<int>> SomeFromState = state =>
        Option.Some(state);

    private static readonly Func<int, int> ValueFromState = state => state;

    private static readonly Func<int, int, int> Sum = (value, state) =>
        value + state;

    private static readonly Func<string, int, int> LengthPlusState =
        (error, state) => error.Length + state;

    private static readonly Func<int, int, Result<int, string>> OkSum =
        (value, state) => Result.Ok<int, string>(value + state);

    private static readonly Func<string, int, Result<int, string>>
        RecoverFromError = (error, state) =>
            Result.Ok<int, string>(error.Length + state);

    private static Task<Option<int>> SomeTask(int value) =>
        Task.FromResult(Option.Some(value));

    private static ValueTask<Option<int>> SomeValueTask(int value) =>
        new(Option.Some(value));

    private static Task<Option<int>> NoneTask() =>
        Task.FromResult(Option.None<int>());

    private static ValueTask<Option<int>> NoneValueTask() =>
        new(Option.None<int>());

    private static Task<Result<int, string>> OkTask(int value) =>
        Task.FromResult(Result.Ok<int, string>(value));

    private static ValueTask<Result<int, string>> OkValueTask(int value) =>
        new(Result.Ok<int, string>(value));

    private static Task<Result<int, string>> ErrTask() =>
        Task.FromResult(Result.Err<int, string>("failed"));

    private static ValueTask<Result<int, string>> ErrValueTask() =>
        new(Result.Err<int, string>("failed"));

    [Fact]
    public async Task GivenSomeTask_WhenIsSomeAndAsyncWithState_ThenUseTheState()
    {
        (await SomeTask(9).IsSomeAndAsync(State, Exceeds)).ShouldBeTrue();
        (await SomeTask(3).IsSomeAndAsync(State, Exceeds)).ShouldBeFalse();
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenIsSomeAndAsyncWithState_ThenUseTheState()
    {
        (await SomeValueTask(9).IsSomeAndAsync(State, Exceeds)).ShouldBeTrue();
        (await NoneValueTask().IsSomeAndAsync(State, Exceeds)).ShouldBeFalse();
    }

    [Fact]
    public async Task GivenNoneTask_WhenOkOrElseAsyncWithState_ThenUseTheState()
    {
        Result<int, string> result =
            await NoneTask().OkOrElseAsync(State, ErrorFrom);

        result.ShouldBeErrValue("error 7");
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenOkOrElseAsyncWithState_ThenUseTheState()
    {
        Result<int, string> result =
            await NoneValueTask().OkOrElseAsync(State, ErrorFrom);

        result.ShouldBeErrValue("error 7");
    }

    [Fact]
    public async Task GivenNoneTask_WhenOrElseAsyncWithState_ThenUseTheState()
    {
        Option<int> result = await NoneTask().OrElseAsync(State, SomeFromState);

        result.ShouldBeSomeValue(7);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenOrElseAsyncWithState_ThenUseTheState()
    {
        Option<int> result =
            await NoneValueTask().OrElseAsync(State, SomeFromState);

        result.ShouldBeSomeValue(7);
    }

    [Fact]
    public async Task
        GivenNoneTask_WhenUnwrapOrElseAsyncWithState_ThenUseTheState()
    {
        int result = await NoneTask().UnwrapOrElseAsync(State, ValueFromState);

        result.ShouldBe(7);
    }

    [Fact]
    public async Task
        GivenNoneValueTask_WhenUnwrapOrElseAsyncWithState_ThenUseTheState()
    {
        int result =
            await NoneValueTask().UnwrapOrElseAsync(State, ValueFromState);

        result.ShouldBe(7);
    }

    [Fact]
    public async Task GivenSomeTask_WhenMapOrElseAsyncWithState_ThenUseTheState()
    {
        int mapped =
            await SomeTask(2).MapOrElseAsync(State, ValueFromState, Sum);
        int fallback =
            await NoneTask().MapOrElseAsync(State, ValueFromState, Sum);

        mapped.ShouldBe(9);
        fallback.ShouldBe(7);
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenMapOrElseAsyncWithState_ThenUseTheState()
    {
        int mapped =
            await SomeValueTask(2).MapOrElseAsync(State, ValueFromState, Sum);
        int fallback =
            await NoneValueTask().MapOrElseAsync(State, ValueFromState, Sum);

        mapped.ShouldBe(9);
        fallback.ShouldBe(7);
    }

    [Fact]
    public async Task GivenOkTask_WhenAndThenAsyncWithState_ThenUseTheState()
    {
        Result<int, string> result =
            await OkTask(2).AndThenAsync(State, OkSum);

        result.ShouldBeOkValue(9);
    }

    [Fact]
    public async Task
        GivenOkValueTask_WhenAndThenAsyncWithState_ThenUseTheState()
    {
        Result<int, string> result =
            await OkValueTask(2).AndThenAsync(State, OkSum);

        result.ShouldBeOkValue(9);
    }

    [Fact]
    public async Task GivenErrTask_WhenOrElseAsyncWithState_ThenUseTheState()
    {
        Result<int, string> result =
            await ErrTask().OrElseAsync(State, RecoverFromError);

        result.ShouldBeOkValue(13);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenOrElseAsyncWithState_ThenUseTheState()
    {
        Result<int, string> result =
            await ErrValueTask().OrElseAsync(State, RecoverFromError);

        result.ShouldBeOkValue(13);
    }

    [Fact]
    public async Task
        GivenErrTask_WhenUnwrapOrElseAsyncWithState_ThenUseTheState()
    {
        int result =
            await ErrTask().UnwrapOrElseAsync(State, LengthPlusState);

        result.ShouldBe(13);
    }

    [Fact]
    public async Task
        GivenErrValueTask_WhenUnwrapOrElseAsyncWithState_ThenUseTheState()
    {
        int result =
            await ErrValueTask().UnwrapOrElseAsync(State, LengthPlusState);

        result.ShouldBe(13);
    }

    [Fact]
    public async Task
        GivenResultTask_WhenMapOrElseAsyncWithState_ThenUseTheState()
    {
        int mapped =
            await OkTask(2).MapOrElseAsync(State, LengthPlusState, Sum);
        int fallback =
            await ErrTask().MapOrElseAsync(State, LengthPlusState, Sum);

        mapped.ShouldBe(9);
        fallback.ShouldBe(13);
    }

    [Fact]
    public async Task
        GivenResultValueTask_WhenMapOrElseAsyncWithState_ThenUseTheState()
    {
        int mapped =
            await OkValueTask(2).MapOrElseAsync(State, LengthPlusState, Sum);
        int fallback =
            await ErrValueTask().MapOrElseAsync(State, LengthPlusState, Sum);

        mapped.ShouldBe(9);
        fallback.ShouldBe(13);
    }

    [Fact]
    public async Task GivenResultTask_WhenMapOrAsyncWithState_ThenUseTheState()
    {
        int mapped = await OkTask(2).MapOrAsync(State, -1, Sum);
        int fallback = await ErrTask().MapOrAsync(State, -1, Sum);

        mapped.ShouldBe(9);
        fallback.ShouldBe(-1);
    }

    [Fact]
    public async Task
        GivenResultValueTask_WhenMapOrAsyncWithState_ThenUseTheState()
    {
        int mapped = await OkValueTask(2).MapOrAsync(State, -1, Sum);
        int fallback = await ErrValueTask().MapOrAsync(State, -1, Sum);

        mapped.ShouldBe(9);
        fallback.ShouldBe(-1);
    }

    [Fact]
    public async Task GivenOptionTask_WhenMapOrAsyncWithState_ThenUseTheState()
    {
        int mapped = await SomeTask(2).MapOrAsync(State, -1, Sum);
        int fallback = await NoneTask().MapOrAsync(State, -1, Sum);

        mapped.ShouldBe(9);
        fallback.ShouldBe(-1);
    }

    [Fact]
    public async Task
        GivenOptionValueTask_WhenMapOrAsyncWithState_ThenUseTheState()
    {
        int mapped = await SomeValueTask(2).MapOrAsync(State, -1, Sum);
        int fallback = await NoneValueTask().MapOrAsync(State, -1, Sum);

        mapped.ShouldBe(9);
        fallback.ShouldBe(-1);
    }
}

