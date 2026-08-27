namespace Waystone.Monads.Options;

using System;
using System.Threading.Tasks;
using Configs;
using Fixtures;
using JetBrains.Annotations;
using Monads.Extensions.Logging.Configs;
using NSubstitute;
using Shouldly;
using Xunit;

[TestSubject(typeof(Option))]
public sealed class OptionTests
{
    private readonly Action<Exception, CallerInfo> _callback;

    public OptionTests()
    {
        _callback = Substitute.For<Action<Exception, CallerInfo>>();
    }

    private MonadOptionsScope LoggerScope() =>
        MonadOptions.BeginScope(
            options => options.UseLogger(new HandledExceptionProbe(_callback)));

    [Fact]
    public async Task GivenAsyncFactory_WhenBinding_ReturnSome()
    {
        ValueTask<Option<int>> optionTask =
            Option.TryAsync(() => Task.FromResult(42));

        Option<int> option = await optionTask;

        option.ShouldBe(Option.Some(42));
    }

    [Fact]
    public async Task
        GivenAsyncFactoryThrows_WhenBinding_ThenReturnNone()
    {
        using (LoggerScope())
        {
            ValueTask<Option<int>> optionTask = Option.TryAsync<int>(async () =>
            {
                await Task.Delay(10, TestContext.Current.CancellationToken);

                throw new Exception();
            });

            Option<int> option = await optionTask;

            option.ShouldBe(Option.None<int>());

            _callback.Received()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void WhenBindingFactoryThatSucceeds_ThenReturnSome()
    {
        using (LoggerScope())
        {
            Option<int> option = Option.Try(() => 1);
            option.ShouldBe(Option.Some(1));

            _callback.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenFactoryReturningDefault_WhenBinding_ThenReturnSome()
    {
        using (LoggerScope())
        {
            Option.Try(() => 0).ShouldBe(Option.Some(0));
            Option.Try(() => false).ShouldBe(Option.Some(false));
            Option.Try(() => default(Guid)).ShouldBe(Option.Some(Guid.Empty));

            _callback.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenFactoryReturningNull_WhenBinding_ThenReturnNone()
    {
        using (LoggerScope())
        {
            Option<string> option = Option.Try(() => default(string)!);

            option.ShouldBe(Option.None<string>());

            _callback.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task
        GivenAsyncFactoryReturningDefault_WhenBinding_ThenReturnSome()
    {
        using (LoggerScope())
        {
            Option<int> option =
                await Option.TryAsync(() => Task.FromResult(0));

            option.ShouldBe(Option.Some(0));

            _callback.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public async Task
        GivenAsyncFactoryReturningNull_WhenBinding_ThenReturnNone()
    {
        using (LoggerScope())
        {
            Option<string> option = await Option.TryAsync(
                () => Task.FromResult(default(string)!));

            option.ShouldBe(Option.None<string>());

            _callback.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenNullReferenceType_WhenCreatingOption_ThenReturnNone()
    {
        string? value = null;
        Option<string> result = Option.FromNullable(value);
        result.ShouldBeNone();
        result.ShouldBeOfType<None<string>>();
    }

    [Fact]
    public void GivenNullValueType_WhenCreatingOption_ThenReturnNone()
    {
        int? value = null;
        Option<int> result = Option.FromNullable(value);
        result.ShouldBeNone();
        result.ShouldBeOfType<None<int>>();
    }

    [Fact]
    public void GivenNotNullReferenceType_WhenCreatingOption_ThenReturnSome()
    {
        var value = "test";
        Option<string> result = Option.FromNullable(value);
        result.ShouldBeSome();
        result.ShouldBeOfType<Some<string>>();
        result.ShouldBeSomeValue("test");
    }

    [Fact]
    public void GivenNotNullValueType_WhenCreatingOption_ThenReturnSome()
    {
        int? value = 42;
        Option<int> result = Option.FromNullable(value);
        result.ShouldBeSome();
        result.ShouldBeOfType<Some<int>>();
        result.ShouldBeSomeValue(42);
    }

    [Fact]
    public void GivenTheDefaultOfAValueType_WhenCreatingOption_ThenReturnSome()
    {
        int? zero = 0;
        Option<int> result = Option.FromNullable(zero);
        result.ShouldBeSome();
        result.ShouldBeOfType<Some<int>>();
        result.ShouldBeSomeValue(0);

        Guid? empty = Guid.Empty;
        Option.FromNullable(empty).ShouldBe(Option.Some(Guid.Empty));
    }
}
