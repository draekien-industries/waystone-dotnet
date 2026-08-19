namespace Waystone.Monads.Options;

using System;
using System.Threading.Tasks;
using Configs;
using JetBrains.Annotations;
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
            options => options.UseExceptionLogger(_callback));

    [Fact]
    public async Task GivenAsyncFactory_WhenBinding_ReturnSome()
    {
        Task<Option<int>> optionTask =
            Option.TryAsync(() => Task.FromResult(42));

        Option<int> option = await optionTask;

        option.ShouldBe(Option.Some(42));
    }

    [Fact]
    public async Task GivenObsoleteAsyncTry_WhenBinding_ThenReturnSome()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        Task<Option<int>> optionTask =
            Option.Try(() => Task.FromResult(42));
#pragma warning restore CS0618 // Type or member is obsolete

        Option<int> option = await optionTask;

        option.ShouldBe(Option.Some(42));
    }

    [Fact]
    public async Task
        GivenAsyncFactoryThrows_WhenBinding_ThenReturnNone()
    {
        using (LoggerScope())
        {
            Task<Option<int>> optionTask = Option.TryAsync<int>(async () =>
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
    public void GivenFactoryReturningDefault_WhenBinding_ThenReturnNone()
    {
        using (LoggerScope())
        {
            Option.Try(() => 0).ShouldBe(Option.None<int>());
            Option.Try(() => false).ShouldBe(Option.None<bool>());
            Option.Try(() => default(Guid)).ShouldBe(Option.None<Guid>());

            _callback.DidNotReceive()
               .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
        }
    }

    [Fact]
    public void GivenAnyFactory_WhenBinding_ThenAgreeWithTheConversion()
    {
        using (LoggerScope())
        {
            Option<int> zero = 0;
            Option<int> one = 1;
            Option<Guid> empty = default(Guid);

            Option.Try(() => 0).ShouldBe(zero);
            Option.Try(() => 1).ShouldBe(one);
            Option.Try(() => default(Guid)).ShouldBe(empty);
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
        GivenAsyncFactoryReturningDefault_WhenBinding_ThenReturnNone()
    {
        using (LoggerScope())
        {
            Option<int> option =
                await Option.TryAsync(() => Task.FromResult(0));

            option.ShouldBe(Option.None<int>());

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
    public void WhenImplicitlyCreatingOption_ThenReturnExpected()
    {
        Option<int> option1 = 0;
        Option<int> option2 = 1;
        Option<string> option3 = string.Empty;
#pragma warning disable CS8604 // Possible null reference argument.

        // ReSharper disable once PreferConcreteValueOverDefault
        Option<string> option4 = default(string);

        // ReSharper disable once PreferConcreteValueOverDefault
        Option<Guid> option5 = default(Guid);
#pragma warning restore CS8604 // Possible null reference argument.
        Option<Guid> option6 = Guid.NewGuid();

        option1.IsSome.ShouldBeFalse();
        option2.IsSome.ShouldBeTrue();
        option3.IsSome.ShouldBeTrue();
        option4.IsSome.ShouldBeFalse();
        option5.IsSome.ShouldBeFalse();
        option6.IsSome.ShouldBeTrue();
    }

    [Fact]
    public void GivenNullReferenceType_WhenCreatingOption_ThenReturnNone()
    {
        string? value = null;
        Option<string> result = Option.FromNullable(value);
        result.IsNone.ShouldBeTrue();
        result.ShouldBeOfType<None<string>>();
    }

    [Fact]
    public void GivenNullValueType_WhenCreatingOption_ThenReturnNone()
    {
        int? value = null;
        Option<int> result = Option.FromNullable(value);
        result.IsNone.ShouldBeTrue();
        result.ShouldBeOfType<None<int>>();
    }

    [Fact]
    public void GivenNotNullReferenceType_WhenCreatingOption_ThenReturnSome()
    {
        var value = "test";
        Option<string> result = Option.FromNullable(value);
        result.IsSome.ShouldBeTrue();
        result.ShouldBeOfType<Some<string>>();
        result.Unwrap().ShouldBe("test");
    }

    [Fact]
    public void GivenNotNullValueType_WhenCreatingOption_ThenReturnSome()
    {
        int? value = 42;
        Option<int> result = Option.FromNullable(value);
        result.IsSome.ShouldBeTrue();
        result.ShouldBeOfType<Some<int>>();
        result.Unwrap().ShouldBe(42);
    }
}
