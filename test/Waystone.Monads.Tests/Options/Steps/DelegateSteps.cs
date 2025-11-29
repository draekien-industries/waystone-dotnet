namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using NSubstitute;
using Reqnroll;
using Results;

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

    [Given("an async delegate that returns an OK result with value {int}")]
    public void GivenAnAsyncDelegateThatReturnsAnOkResultWithValue(int p0)
    {
        var func = Substitute.For<Func<int, Task<Result<int, string>>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(Task.FromResult(Result.Ok<int, string>(p0)));

        context.Set(func, Constants.AsyncOkDelegate);
    }

    [Given(
        "an async delegate that returns an Error result with message {string}")]
    public void GivenAnAsyncDelegateThatReturnsAnErrorResultWithMessage(
        string p0)
    {
        var func = Substitute.For<Func<int, Task<Result<int, string>>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(Task.FromResult(Result.Err<int, string>(p0)));

        context.Set(func, Constants.AsyncErrorDelegate);
    }

    [Given("a sync delegate that returns an OK result with value {int}")]
    public void GivenASyncDelegateThatReturnsAnOkResultWithValue(int p0)
    {
        var func = Substitute.For<Func<int, Result<int, string>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(Result.Ok<int, string>(p0));

        context.Set(func, Constants.SyncOkDelegate);
    }

    [Given(
        "a sync delegate that returns an Error result with message {string}")]
    public void GivenASyncDelegateThatReturnsAnErrorResultWithMessage(string p0)
    {
        var func = Substitute.For<Func<int, Result<int, string>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(Result.Err<int, string>(p0));

        context.Set(func, Constants.SyncErrorDelegate);
    }

    [Then("the async delegate should not have been invoked")]
    public void ThenTheAsyncDelegateShouldNotHaveBeenInvoked()
    {
        var func = context.Get<Func<string, Task>>();

        func.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Then(
        "the async delegate should have been invoked once with message {string}")]
    public void ThenTheAsyncDelegateShouldHaveBeenInvokedOnceWithMessage(
        string error)
    {
        var func = context.Get<Func<string, Task>>();

        func.Received(1).Invoke(Arg.Any<string>());
    }

    [Given("an async delegate for string returning Task")]
    public void GivenAnAsyncDelegateForStringReturningTask()
    {
        var asyncDelegate = Substitute.For<Func<string, Task>>();
        context.Set(asyncDelegate);
    }

    [Given("an async factory that returns {string}")]
    public void GivenAnAsyncFactoryThatReturns(string missing)
    {
        var asyncFactory = Substitute.For<Func<string, Task<string>>>();

        asyncFactory.Invoke(Arg.Any<string>())
           .Returns(Task.FromResult(missing));

        context.Set(asyncFactory, Constants.AsyncErrorDelegate);
    }

    [Given("an async map that converts the value into a string")]
    public void GivenAnAsyncMapThatConvertsTheValueIntoAString()
    {
        var asyncMap = Substitute.For<Func<int, Task<string>>>();

        asyncMap.Invoke(Arg.Any<int>())
           .Returns(callInfo =>
            {
                var value = callInfo.Arg<int>();

                return Task.FromResult(value.ToString());
            });

        context.Set(asyncMap, Constants.AsyncOkDelegate);
    }

    [Given("a sync factory that returns {string}")]
    public void GivenASyncFactoryThatReturns(string p0)
    {
        var syncFactory = Substitute.For<Func<string, string>>();

        syncFactory.Invoke(Arg.Any<string>()).Returns(p0);

        context.Set(syncFactory, Constants.SyncErrorDelegate);
    }

    [Given("a sync map that converts the value into a string")]
    public void GivenASyncMapThatConvertsTheValueIntoAString()
    {
        var syncMap = Substitute.For<Func<int, string>>();

        syncMap.Invoke(Arg.Any<int>())
           .Returns(callInfo =>
            {
                var value = callInfo.Arg<int>();

                return value.ToString();
            });

        context.Set(syncMap, Constants.SyncOkDelegate);
    }
}
