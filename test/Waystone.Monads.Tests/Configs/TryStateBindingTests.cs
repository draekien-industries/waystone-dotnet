namespace Waystone.Monads.Configs;

using Extensions.Logging.Configs;
using Fixtures;
using JetBrains.Annotations;
using NSubstitute;
using Options;
using Results;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// <c>Option.With</c> and <c>Result.With</c> reach the <c>Try</c> factories,
/// which have no monad to be called on and so cannot be reached the way the rest
/// of the vocabulary is. The behaviour they forward to is already covered by
/// <see cref="TryStateOverloadTests" />; what is new here is the forwarding
/// itself.
/// <para>
/// The caller-info test is the one that earns its place. Those parameters are
/// declared on the wrapper rather than left to the forwarded call because the
/// compiler fills them at the outermost call site — drop them and every handled
/// exception is reported against the wrapper's own file, which is a silent loss
/// of the only breadcrumb an absence leaves behind.
/// </para>
/// </remarks>
[TestSubject(typeof(Option))]
public sealed class TryStateBindingTests
{
    private readonly Action<Exception, CallerInfo> _logger =
        Substitute.For<Action<Exception, CallerInfo>>();

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

    private static string OnError(Exception exception) => exception.Message;

    [Fact]
    public void OptionTryBindsTheStateAndReturnsSome()
    {
        using (LoggerScope())
        {
            Option.With(21).Try(Double).ShouldBe(Option.Some(42));

            _logger.DidNotReceive()
                   .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void OptionTryReturnsNoneWhenTheFactoryThrows()
    {
        using (LoggerScope())
        {
            Option.With(21).Try(Throw).ShouldBe(Option.None<int>());

            _logger.Received(1)
                   .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task OptionTryAsyncBindsTheStateAndReturnsSome()
    {
        using (LoggerScope())
        {
            Option<int> option =
                await Option.With(21).TryAsync(DoubleAsync);

            option.ShouldBe(Option.Some(42));

            _logger.DidNotReceive()
                   .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task OptionTryAsyncReturnsNoneWhenTheFactoryThrows()
    {
        using (LoggerScope())
        {
            Option<int> option = await Option.With(21).TryAsync(ThrowAsync);

            option.ShouldBe(Option.None<int>());

            _logger.Received(1)
                   .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void ResultTryBindsTheStateAndReturnsOk()
    {
        using (LoggerScope())
        {
            Result.With(21)
                  .Try(Double, OnError)
                  .ShouldBe(Result.Ok<int, string>(42));
        }
    }

    [Fact]
    public void ResultTryReportsTheErrorOnErrorProduced()
    {
        using (LoggerScope())
        {
            Result.With(21)
                  .Try(Throw, OnError)
                  .ShouldBe(Result.Err<int, string>("21"));

            _logger.Received(1)
                   .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void ResultTryWithoutOnErrorFallsBackToTheLibrarysError()
    {
        using (LoggerScope())
        {
            Result.With(21).Try(Double).ShouldBeOkValue(42);
            Result.With(21).Try(Throw).ShouldBeErr();
        }
    }

    [Fact]
    public async Task ResultTryAsyncBindsTheStateAndReturnsOk()
    {
        using (LoggerScope())
        {
            Result<int, string> result =
                await Result.With(21).TryAsync(DoubleAsync, OnError);

            result.ShouldBe(Result.Ok<int, string>(42));
        }
    }

    [Fact]
    public async Task ResultTryAsyncReportsTheErrorOnErrorProduced()
    {
        using (LoggerScope())
        {
            Result<int, string> result =
                await Result.With(21).TryAsync(ThrowAsync, OnError);

            result.ShouldBe(Result.Err<int, string>("21"));

            _logger.Received(1)
                   .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task ResultTryAsyncWithoutOnErrorFallsBackToTheLibrarysError()
    {
        using (LoggerScope())
        {
            await Result.With(21).TryAsync(DoubleAsync).ShouldBeOkValueAsync(42);
            await Result.With(21).TryAsync(ThrowAsync).ShouldBeErrAsync();
        }
    }

    /// <remarks>
    /// Both halves matter and fail differently. Without the caller-info
    /// parameters declared on the wrapper, <c>MemberName</c> becomes <c>Try</c>
    /// — the wrapper's own method — and <c>ArgumentExpression</c> becomes
    /// <c>factory</c>, the wrapper's parameter name, rather than the text the
    /// consumer wrote.
    /// </remarks>
    [Fact]
    public void CallerInfoNamesTheConsumersCallSiteAndNotTheWrapper()
    {
        CallerInfo? captured = null;

        void Capture(Exception exception, CallerInfo info) => captured = info;

        using (MonadOptions.BeginScope(
                   options =>
                       options.UseLogger(new HandledExceptionProbe(Capture))))
        {
            Option.With(21).Try(Throw);
        }

        captured.ShouldNotBeNull();
        captured.MemberName
                .ShouldBe(nameof(CallerInfoNamesTheConsumersCallSiteAndNotTheWrapper));
        captured.ArgumentExpression.ShouldBe(nameof(Throw));
        captured.LineNumber.ShouldBeGreaterThan(0);
    }
}
