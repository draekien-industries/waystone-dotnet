namespace Waystone.Monads.Configs;

using System;
using System.Threading;
using System.Threading.Tasks;
using Extensions.Logging.Configs;
using Fixtures;
using JetBrains.Annotations;
using NSubstitute;
using Options;
using Results;
using Results.Errors;
using Shouldly;
using Xunit;

[TestSubject(typeof(Option))]
public sealed class TryStateOverloadTests
{
    private readonly Action<Exception, CallerInfo> _logger;

    public TryStateOverloadTests()
    {
        _logger = Substitute.For<Action<Exception, CallerInfo>>();
    }

    private MonadOptionsScope LoggerScope() =>
        MonadOptions.BeginScope(
            options => options.UseLogger(new HandledExceptionProbe(_logger)));

    private static int Double(int state) => state * 2;

    private static Task<int> DoubleAsync(int state) =>
        Task.FromResult(state * 2);

    private static int Throw(int state) =>
        throw new InvalidOperationException(state.ToString());

    private static Task<int> ThrowAsync(int state) =>
        throw new InvalidOperationException(state.ToString());

    [Fact]
    public void GivenState_WhenOptionTrySucceeds_ThenReturnSome()
    {
        using (LoggerScope())
        {
            Option.Try(21, Double).ShouldBe(Option.Some(42));

            _logger.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenState_WhenOptionTryThrows_ThenReturnNone()
    {
        using (LoggerScope())
        {
            Option.Try(21, Throw).ShouldBe(Option.None<int>());

            _logger.Received(1)
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenState_WhenOptionTryReturnsNull_ThenReturnNone()
    {
        using (LoggerScope())
        {
            Option.Try("x", static _ => default(string)!)
               .ShouldBe(Option.None<string>());

            _logger.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task GivenState_WhenOptionTryAsyncSucceeds_ThenReturnSome()
    {
        using (LoggerScope())
        {
            Option<int> option = await Option.TryAsync(21, DoubleAsync);

            option.ShouldBe(Option.Some(42));
        }
    }

    [Fact]
    public async Task GivenState_WhenOptionTryAsyncThrows_ThenReturnNone()
    {
        using (LoggerScope())
        {
            Option<int> option = await Option.TryAsync(21, ThrowAsync);

            option.ShouldBe(Option.None<int>());

            _logger.Received(1)
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task GivenState_WhenOptionTryAsyncReturnsNull_ThenReturnNone()
    {
        using (LoggerScope())
        {
            Option<string> option = await Option.TryAsync(
                "x",
                static _ => Task.FromResult(default(string)!));

            option.ShouldBe(Option.None<string>());

            _logger.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenState_WhenResultTrySucceeds_ThenReturnOk()
    {
        var onError = Substitute.For<Func<Exception, string>>();

        Result.Try(21, Double, onError)
           .ShouldBe(Result.Ok<int, string>(42));

        onError.DidNotReceive().Invoke(Arg.Any<Exception>());
    }

    [Fact]
    public void GivenState_WhenResultTryThrows_ThenReturnErr()
    {
        using (LoggerScope())
        {
            Result.Try(21, Throw, static ex => ex.Message)
               .ShouldBe(Result.Err<int, string>("21"));

            _logger.Received(1)
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenState_WhenResultTryReturnsNull_ThenReturnErr()
    {
        var onError = Substitute.For<Func<Exception, string>>();
        onError.Invoke(Arg.Any<Exception>()).Returns("null");

        Result.Try("x", static _ => default(string)!, onError)
           .ShouldBe(Result.Err<string, string>("null"));

        onError.Received(1).Invoke(Arg.Any<ArgumentNullException>());
    }

    [Fact]
    public async Task GivenState_WhenResultTryAsyncSucceeds_ThenReturnOk()
    {
        Result<int, string> result =
            await Result.TryAsync(21, DoubleAsync, static ex => ex.Message);

        result.ShouldBe(Result.Ok<int, string>(42));
    }

    [Fact]
    public async Task GivenState_WhenResultTryAsyncThrows_ThenReturnErr()
    {
        using (LoggerScope())
        {
            Result<int, string> result = await Result.TryAsync(
                21,
                ThrowAsync,
                static ex => ex.Message);

            result.ShouldBe(Result.Err<int, string>("21"));

            _logger.Received(1)
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenState_WhenTheErrorReturningTrySucceeds_ThenReturnOk()
    {
        Result<int, Error> result = Result.Try(21, Double);

        result.ShouldBe(Result.Ok<int, Error>(42));
    }

    [Fact]
    public void GivenState_WhenTheErrorReturningTryThrows_ThenReturnErr()
    {
        using (LoggerScope())
        {
            Result<int, Error> result = Result.Try(21, Throw);

            result.ShouldBeErr();
            result.UnwrapErr().Message.ShouldBe("21");
        }
    }

    [Fact]
    public async Task
        GivenState_WhenTheErrorReturningTryAsyncSucceeds_ThenReturnOk()
    {
        Result<int, Error> result = await Result.TryAsync(21, DoubleAsync);

        result.ShouldBe(Result.Ok<int, Error>(42));
    }

    [Fact]
    public async Task
        GivenState_WhenTheErrorReturningTryAsyncThrows_ThenReturnErr()
    {
        using (LoggerScope())
        {
            Result<int, Error> result = await Result.TryAsync(21, ThrowAsync);

            result.ShouldBeErr();
            result.UnwrapErr().Message.ShouldBe("21");
        }
    }

    [Fact]
    public void GivenState_WhenTheFactoryIsCancelled_ThenRethrow()
    {
        using (LoggerScope())
        {
            using var source = new CancellationTokenSource();
            source.Cancel();

            Should.Throw<OperationCanceledException>(
                () => Option.Try(
                    source.Token,
                    static token =>
                    {
                        token.ThrowIfCancellationRequested();

                        return 1;
                    }));

            _logger.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task GivenAToken_WhenTryAsyncIsHandedIt_ThenItReachesTheFactory()
    {
        using var source = new CancellationTokenSource();

        Option<int> option = await Option.TryAsync(
            source.Token,
            static async token =>
            {
                await Task.Delay(1, token);

                return token.CanBeCanceled ? 1 : 0;
            });

        option.ShouldBe(Option.Some(1));
    }
}
