namespace Waystone.Monads.Specs;

using NSubstitute;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public sealed class DelegateSteps(ScenarioContext context)
{
    [Given("an async delegate")]
    public void GivenAnAsyncDelegate()
    {
        var asyncDelegate = Substitute.For<Func<int, Task>>();
        context.Set(asyncDelegate);
    }

    [Then("the async delegate should be invoked with value {int}")]
    public void ThenTheAsyncDelegateShouldBeInvokedWithValue(int value)
    {
        var asyncDelegate = context.Get<Func<int, Task>>();
        asyncDelegate.Received(1).Invoke(value);
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
    public void ThenTheSynchronousDelegateShouldBeInvokedWithValue(int value)
    {
        var syncDelegate = context.Get<Action<int>>();
        syncDelegate.Received(1).Invoke(value);
    }

    [Then("the synchronous delegate should not be invoked")]
    public void ThenTheSynchronousDelegateShouldNotBeInvoked()
    {
        var syncDelegate = context.Get<Action<int>>();
        syncDelegate.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Given("an async Error delegate that returns {string}")]
    public void GivenAnAsyncErrorDelegateThatReturns(string value)
    {
        var asyncErrDelegate = Substitute.For<Func<Task<string>>>();
        asyncErrDelegate.Invoke().Returns(Task.FromResult(value));
        context.Set(asyncErrDelegate, Constants.AsyncErrorDelegate);
    }

    [Given("a synchronous Error delegate that returns {string}")]
    public void GivenASynchronousErrorDelegateThatReturns(string value)
    {
        var syncErrDelegate = Substitute.For<Func<string>>();
        syncErrDelegate.Invoke().Returns(value);
        context.Set(syncErrDelegate, Constants.SyncErrorDelegate);
    }

    [Given("an async delegate that returns an OK result with value {int}")]
    public void GivenAnAsyncDelegateThatReturnsAnOkResultWithValue(int value)
    {
        var func = Substitute.For<Func<int, Task<Result<int, string>>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(Task.FromResult(Result.Ok<int, string>(value)));

        context.Set(func, Constants.AsyncOkDelegate);
    }

    [Given(
        "an async delegate that returns an Error result with message {string}")]
    public void GivenAnAsyncDelegateThatReturnsAnErrorResultWithMessage(
        string message)
    {
        var func = Substitute.For<Func<int, Task<Result<int, string>>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(Task.FromResult(Result.Err<int, string>(message)));

        context.Set(func, Constants.AsyncErrorDelegate);
    }

    [Given("a sync delegate that returns an OK result with value {int}")]
    public void GivenASyncDelegateThatReturnsAnOkResultWithValue(int value)
    {
        var func = Substitute.For<Func<int, Result<int, string>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(Result.Ok<int, string>(value));

        context.Set(func, Constants.SyncOkDelegate);
    }

    [Given(
        "a sync delegate that returns an Error result with message {string}")]
    public void GivenASyncDelegateThatReturnsAnErrorResultWithMessage(string message)
    {
        var func = Substitute.For<Func<int, Result<int, string>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(Result.Err<int, string>(message));

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
        string message)
    {
        var func = context.Get<Func<string, Task>>();

        func.Received(1).Invoke(message);
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
    public void GivenASyncFactoryThatReturns(string value)
    {
        var syncFactory = Substitute.For<Func<string, string>>();

        syncFactory.Invoke(Arg.Any<string>()).Returns(value);

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

    [Given("an {string} {string} handler that returns no value")]
    public void GivenAnHandlerThatReturnsNoValue(string variant, string handler)
    {
        switch (variant)
        {
            case "async" when handler == "Ok":
            {
                var asyncOkHandler = Substitute.For<Func<int, Task>>();

                context.Set(asyncOkHandler, Constants.AsyncOkDelegate);

                break;
            }
            case "async" when handler == "Error":
            {
                var asyncErrHandler = Substitute.For<Func<string, Task>>();

                context.Set(asyncErrHandler, Constants.AsyncErrorDelegate);

                break;
            }
            case "sync" when handler == "Ok":
            {
                var syncOkHandler = Substitute.For<Action<int>>();

                context.Set(syncOkHandler, Constants.SyncOkDelegate);

                break;
            }
            case "sync" when handler == "Error":
            {
                var syncErrHandler = Substitute.For<Action<string>>();

                context.Set(syncErrHandler, Constants.SyncErrorDelegate);

                break;
            }
            default:
                throw new NotImplementedException(
                    "Handler not implemented: " + variant + " " + handler);
        }
    }

    [Then(
        "the {string} {string} handler should have been called with value {int}")]
    public void ThenTheHandlerShouldHaveBeenCalledWithValue(
        string variant,
        string handler,
        int value)
    {
        switch (variant)
        {
            case "async" when handler == "Ok":
            {
                var asyncOkHandler =
                    context.Get<Func<int, Task>>(Constants.AsyncOkDelegate);

                asyncOkHandler.Received(1).Invoke(value);

                break;
            }
            case "sync" when handler == "Ok":
            {
                var syncOkHandler =
                    context.Get<Action<int>>(Constants.SyncOkDelegate);

                syncOkHandler.Received(1).Invoke(value);

                break;
            }
            default:
                throw new NotImplementedException(
                    "Handler not implemented: " + variant + " " + handler);
        }
    }

    [Then("the {string} {string} handler should not have been called")]
    public void ThenTheHandlerShouldNotHaveBeenCalled(
        string variant,
        string handler)
    {
        switch (variant)
        {
            case "async" when handler == "Error":
            {
                var asyncErrHandler =
                    context.Get<Func<string, Task>>(
                        Constants.AsyncErrorDelegate);

                asyncErrHandler.DidNotReceive().Invoke(Arg.Any<string>());

                break;
            }
            case "sync" when handler == "Error":
            {
                var syncErrHandler =
                    context.Get<Action<string>>(Constants.SyncErrorDelegate);

                syncErrHandler.DidNotReceive().Invoke(Arg.Any<string>());

                break;
            }
            default:
                throw new NotImplementedException(
                    "Handler not implemented: " + variant + " " + handler);
        }
    }

    [Then(
        "the {string} {string} handler should have been called with value {string}")]
    public void ThenTheHandlerShouldHaveBeenCalledWithValue(
        string variant,
        string handler,
        string value)
    {
        switch (variant)
        {
            case "async" when handler == "Error":
            {
                var asyncErrHandler =
                    context.Get<Func<string, Task>>(
                        Constants.AsyncErrorDelegate);

                asyncErrHandler.Received(1).Invoke(value);

                break;
            }
            case "sync" when handler == "Error":
            {
                var syncErrHandler =
                    context.Get<Action<string>>(Constants.SyncErrorDelegate);

                syncErrHandler.Received(1).Invoke(value);

                break;
            }
            default:
                throw new NotImplementedException(
                    "Handler not implemented: " + variant + " " + handler);
        }
    }

    [Then("the {string} {string} handler should have not been called")]
    public void ThenTheHandlerShouldHaveNotBeenCalled(string variant, string handler)
    {
        switch (variant)
        {
            case "async" when handler == "Ok":
            {
                var asyncOkHandler =
                    context.Get<Func<int, Task>>(Constants.AsyncOkDelegate);

                asyncOkHandler.DidNotReceive().Invoke(Arg.Any<int>());

                break;
            }
            case "sync" when handler == "Ok":
            {
                var syncOkHandler =
                    context.Get<Action<int>>(Constants.SyncOkDelegate);

                syncOkHandler.DidNotReceive().Invoke(Arg.Any<int>());

                break;
            }
            default:
                throw new NotImplementedException(
                    "Handler not implemented: " + variant + " " + handler);
        }
    }
}
