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
        MonadOptions.Global.UseExceptionLogger(_callback);
    }

    [Fact]
    public async Task GivenAsyncFactory_WhenBinding_ReturnSome()
    {
        Task<Option<int>> optionTask =
            Option.Try(() => Task.FromResult(42));

        Option<int> option = await optionTask;

        option.ShouldBe(Option.Some(42));
    }

    [Fact]
    public async Task
        GivenAsyncFactoryThrows_WhenBinding_ThenReturnNone()
    {
        Task<Option<int>> optionTask = Option.Try<int>(async () =>
        {
            await Task.Delay(10);
            throw new Exception();
        });

        Option<int> option = await optionTask;

        option.ShouldBe(Option.None<int>());
        _callback.Received()
                 .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
    }


    [Fact]
    public void WhenBindingFactoryThatSucceeds_ThenReturnSome()
    {
        Option<int> option = Option.Try(() => 1);
        option.ShouldBe(Option.Some(1));
        _callback.DidNotReceive()
                 .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
    }

    [Fact]
    public void
        GivenFactoryThatThrows_AndOnErrorCallback_WhenBindingFactory_ThenInvokeCallback()
    {
        Option<int> option = Option.Try(() =>
        {
            throw new Exception();
#pragma warning disable CS0162 // Unreachable code detected
            return 1;
#pragma warning restore CS0162 // Unreachable code detected
        });
        option.ShouldBe(Option.None<int>());
        _callback.Received(1)
                 .Invoke(Arg.Any<Exception>(), Arg.Any<CallerInfo>());
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
