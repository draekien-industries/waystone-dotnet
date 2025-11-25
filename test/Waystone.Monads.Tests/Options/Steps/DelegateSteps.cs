namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using NSubstitute;
using Reqnroll;

[Binding]
public class DelegateSteps(ScenarioContext context)
{
    [Given("an async delegate")]
    public void GivenAnAsyncDelegate()
    {
        var asyncDelegate = Substitute.For<Func<int, Task>>();
        context.Set(asyncDelegate);
    }

    [Then("the async delegate should be invoked with value {int}")]
    public void ThenTheAsyncDelegateShouldBeInvokedWithValue(int p0)
    {
        var asyncDelegate = context.Get<Func<int, Task>>();
        asyncDelegate.Received(1).Invoke(p0);
    }

    [Then("the async delegate should not be invoked")]
    public void ThenTheAsyncDelegateShouldNotBeInvoked()
    {
        var asyncDelegate = context.Get<Func<int, Task>>();
        asyncDelegate.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Given("a synchronous delegate")]
    public void GivenASynchronousDelegate()
    {
        var syncDelegate = Substitute.For<Action<int>>();
        context.Set(syncDelegate);
    }

    [Then("the synchronous delegate should be invoked with value {int}")]
    public void ThenTheSynchronousDelegateShouldBeInvokedWithValue(int p0)
    {
        var syncDelegate = context.Get<Action<int>>();
        syncDelegate.Received(1).Invoke(p0);
    }

    [Then("the synchronous delegate should not be invoked")]
    public void ThenTheSynchronousDelegateShouldNotBeInvoked()
    {
        var syncDelegate = context.Get<Action<int>>();
        syncDelegate.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Given("an async Error delegate that returns {string}")]
    public void GivenAnAsyncErrorDelegateThatReturns(string p0)
    {
        var asyncErrDelegate = Substitute.For<Func<Task<string>>>();
        asyncErrDelegate.Invoke().Returns(Task.FromResult(p0));
        context.Set(asyncErrDelegate, Constants.AsyncErrorDelegate);
    }

    [Given("a synchronous Error delegate that returns {string}")]
    public void GivenASynchronousErrorDelegateThatReturns(string p0)
    {
        var syncErrDelegate = Substitute.For<Func<string>>();
        syncErrDelegate.Invoke().Returns(p0);
        context.Set(syncErrDelegate, Constants.SyncErrorDelegate);
    }
}
