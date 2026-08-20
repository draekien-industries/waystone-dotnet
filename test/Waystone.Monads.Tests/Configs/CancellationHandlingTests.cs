namespace Waystone.Monads.Configs;

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NSubstitute;
using Options;
using Results;
using Results.Errors;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadOptions))]
public sealed class CancellationHandlingTests
{
    private readonly Action<Exception, CallerInfo> _logger;

    public CancellationHandlingTests()
    {
        _logger = Substitute.For<Action<Exception, CallerInfo>>();
    }

    private MonadOptionsScope Default() =>
        MonadOptions.BeginScope(options => options.UseExceptionLogger(_logger));

    private MonadOptionsScope OptedIn() =>
        MonadOptions.BeginScope(
            options => options.UseExceptionLogger(_logger)
               .UseCancellationAsFailure());

    private static OperationCanceledException Cancelled()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();

        try
        {
            source.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException ex)
        {
            return ex;
        }

        throw new InvalidOperationException("The token did not cancel.");
    }

    private void ShouldNotHaveLogged() =>
        _logger.DidNotReceive()
           .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());

    [Fact]
    public void GivenTheDefault_WhenOptionTryIsCancelled_ThenRethrow()
    {
        using (Default())
        {
            Should.Throw<OperationCanceledException>(
                () => Option.Try<int>(() => throw Cancelled()));

            ShouldNotHaveLogged();
        }
    }

    [Fact]
    public async Task GivenTheDefault_WhenOptionTryAsyncIsCancelled_ThenRethrow()
    {
        using (Default())
        {
            await Should.ThrowAsync<OperationCanceledException>(
                () => Option.TryAsync<int>(() => throw Cancelled()));

            ShouldNotHaveLogged();
        }
    }

    [Fact]
    public void GivenTheDefault_WhenResultTryIsCancelled_ThenRethrow()
    {
        var onError = Substitute.For<Func<Exception, string>>();

        using (Default())
        {
            Should.Throw<OperationCanceledException>(
                () => Result.Try<int, string>(
                    () => throw Cancelled(),
                    onError));

            onError.DidNotReceive().Invoke(Arg.Any<Exception>());
            ShouldNotHaveLogged();
        }
    }

    [Fact]
    public async Task GivenTheDefault_WhenResultTryAsyncIsCancelled_ThenRethrow()
    {
        var onError = Substitute.For<Func<Exception, string>>();

        using (Default())
        {
            await Should.ThrowAsync<OperationCanceledException>(
                () => Result.TryAsync<int, string>(
                    () => throw Cancelled(),
                    onError));

            onError.DidNotReceive().Invoke(Arg.Any<Exception>());
            ShouldNotHaveLogged();
        }
    }

    [Fact]
    public void GivenTheDefault_WhenTheErrorReturningTryIsCancelled_ThenRethrow()
    {
        using (Default())
        {
            Should.Throw<OperationCanceledException>(
                () => Result.Try<int>(() => throw Cancelled()));

            ShouldNotHaveLogged();
        }
    }

    [Fact]
    public async Task
        GivenTheDefault_WhenTheErrorReturningTryAsyncIsCancelled_ThenRethrow()
    {
        using (Default())
        {
            await Should.ThrowAsync<OperationCanceledException>(
                () => Result.TryAsync<int>(() => throw Cancelled()));

            ShouldNotHaveLogged();
        }
    }

    [Fact]
    public async Task GivenTheDefault_WhenATaskIsCancelled_ThenRethrow()
    {
        using (Default())
        {
            using var source = new CancellationTokenSource();
            source.Cancel();

            await Should.ThrowAsync<TaskCanceledException>(
                () => Option.TryAsync(
                    () => Task.FromCanceled<int>(source.Token)));

            ShouldNotHaveLogged();
        }
    }

    [Fact]
    public void GivenTheDefault_WhenAnotherExceptionIsThrown_ThenStillCatch()
    {
        using (Default())
        {
            Option.Try<int>(() => throw new InvalidOperationException())
               .ShouldBe(Option.None<int>());

            _logger.Received(1)
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenTheOptIn_WhenOptionTryIsCancelled_ThenReturnNone()
    {
        using (OptedIn())
        {
            Option.Try<int>(() => throw Cancelled())
               .ShouldBe(Option.None<int>());

            _logger.Received(1)
               .Invoke(
                    Arg.Any<OperationCanceledException>(),
                    Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task
        GivenTheOptIn_WhenOptionTryAsyncIsCancelled_ThenReturnNone()
    {
        using (OptedIn())
        {
            Option<int> option =
                await Option.TryAsync<int>(() => throw Cancelled());

            option.ShouldBe(Option.None<int>());

            _logger.Received(1)
               .Invoke(
                    Arg.Any<OperationCanceledException>(),
                    Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenTheOptIn_WhenResultTryIsCancelled_ThenReturnErr()
    {
        using (OptedIn())
        {
            Result<int, string> result = Result.Try<int, string>(
                () => throw Cancelled(),
                _ => "cancelled");

            result.ShouldBe(Result.Err<int, string>("cancelled"));

            _logger.Received(1)
               .Invoke(
                    Arg.Any<OperationCanceledException>(),
                    Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task GivenTheOptIn_WhenResultTryAsyncIsCancelled_ThenReturnErr()
    {
        using (OptedIn())
        {
            Result<int, string> result = await Result.TryAsync<int, string>(
                () => throw Cancelled(),
                _ => "cancelled");

            result.ShouldBe(Result.Err<int, string>("cancelled"));

            _logger.Received(1)
               .Invoke(
                    Arg.Any<OperationCanceledException>(),
                    Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenTheOptIn_WhenTheErrorReturningTryIsCancelled_ThenReturnErr()
    {
        using (OptedIn())
        {
            Result<int, Error> result =
                Result.Try<int>(() => throw Cancelled());

            result.IsErr.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task
        GivenTheOptIn_WhenTheErrorReturningTryAsyncIsCancelled_ThenReturnErr()
    {
        using (OptedIn())
        {
            Result<int, Error> result =
                await Result.TryAsync<int>(() => throw Cancelled());

            result.IsErr.ShouldBeTrue();
        }
    }

    [Fact]
    public void GivenTheOptIn_WhenANestedScopeIsOpened_ThenItIsInherited()
    {
        using (OptedIn())
        {
            using (MonadOptions.BeginScope(
                       options => options.UseFallbackErrorCode("Nested")))
            {
                Option.Try<int>(() => throw Cancelled())
                   .ShouldBe(Option.None<int>());
            }
        }
    }

    [Fact]
    public void GivenTheOptIn_WhenTheScopeEnds_ThenTheDefaultReturns()
    {
        using (OptedIn())
        {
            Option.Try<int>(() => throw Cancelled())
               .ShouldBe(Option.None<int>());
        }

        using (Default())
        {
            Should.Throw<OperationCanceledException>(
                () => Option.Try<int>(() => throw Cancelled()));
        }
    }
}
