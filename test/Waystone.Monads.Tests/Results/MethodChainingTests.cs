namespace Waystone.Monads.Results;

using System;
using System.Threading.Tasks;
using Extensions;
using NSubstitute;
using Xunit;

public sealed class MethodChainingTests
{
    [Fact]
    public async Task GivenAsyncResult_ThenMethodsShouldChain()
    {
        var onOk = Substitute.For<Func<int, Task>>();
        var onErr = Substitute.For<Func<string, Task>>();
        var inspectOk = Substitute.For<Func<int, Task>>();
        var inspectErr = Substitute.For<Func<string, Task>>();

        await Task.FromResult(Result.Ok<int, string>(1))
                  .MapAsync(x => Task.FromResult(x + 1))
                  .MapErrAsync(e => Task.FromResult(e.ToUpper()))
                  .InspectAsync(inspectOk)
                  .InspectErrAsync(inspectErr)
                  .MatchAsync(onOk, onErr);

        await onOk.Received(1).Invoke(2);
        await onErr.DidNotReceive().Invoke(Arg.Any<string>());
        await inspectOk.Received(1).Invoke(2);
        await inspectErr.DidNotReceive().Invoke(Arg.Any<string>());
    }
}
