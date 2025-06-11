namespace Waystone.Monads.Configs;

using System;
using NSubstitute;
using Options;
using Shouldly;
using Xunit;

public sealed class MonadsGlobalConfigTests
{
    [Fact]
    public void
        GivenConfiguredLogActionThatAcceptsCallerInfo_WhenTryFails_ThenInvokeConfiguredLogAction()
    {
        var configuredLogAction =
            Substitute.For<Action<Exception, CallerInfo>>();
        MonadsGlobalConfig.UseExceptionLogger(configuredLogAction);

        Option<int> option = Option.Try(() =>
        {
            throw new Exception("test exception");
            return 1;
        });

        option.ShouldBe(Option.None<int>());
        configuredLogAction.Received(1)
                           .Invoke(
                                Arg.Is<Exception>(e => e.Message
                                                   == "test exception"),
                                Arg.Is<CallerInfo>(ci => ci.MemberName
                                                    == nameof(
                                                           GivenConfiguredLogActionThatAcceptsCallerInfo_WhenTryFails_ThenInvokeConfiguredLogAction)));
    }
}
