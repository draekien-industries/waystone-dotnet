namespace Waystone.Monads.Options
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using NSubstitute;
    using Shouldly;
    using Xunit;

    [TestSubject(typeof(Option))]
    public sealed class OptionTests
    {
        [Fact]
        public async Task GivenAsyncFactory_WhenBinding_ReturnSome()
        {
            Task<Option<int>> optionTask =
                Option.Bind(() => Task.FromResult(42));

            Option<int> option = await optionTask;

            option.ShouldBe(Option.Some(42));
        }

        [Fact]
        public async Task
            GivenAsyncFactoryThrows_WhenBinding_ThenReturnNone()
        {
            var callback = Substitute.For<Action<Exception>>();
            Task<Option<int>> optionTask = Option.Bind<int>(
                async () =>
                {
                    await Task.Delay(10);
                    throw new Exception();
                },
                callback);

            Option<int> option = await optionTask;

            option.ShouldBe(Option.None<int>());
            callback.Received().Invoke(Arg.Any<Exception>());
        }


        [Fact]
        public void WhenBindingFactoryThatSucceeds_ThenReturnSome()
        {
            var callback = Substitute.For<Action<Exception>>();
            Option<int> option = Option.Bind(() => 1, callback);
            option.ShouldBe(Option.Some(1));
            callback.DidNotReceive().Invoke(Arg.Any<Exception>());
        }

        [Fact]
        public void
            GivenFactoryThatThrows_AndOnErrorCallback_WhenBindingFactory_ThenInvokeCallback()
        {
            var callback = Substitute.For<Action<Exception>>();
            Option<int> option = Option.Bind(
                () =>
                {
                    throw new Exception();
#pragma warning disable CS0162 // Unreachable code detected
                    return 1;
#pragma warning restore CS0162 // Unreachable code detected
                },
                callback);
            option.ShouldBe(Option.None<int>());
            callback.Received(1).Invoke(Arg.Any<Exception>());
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
    }
}
