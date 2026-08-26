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
public sealed class DelegateSteps(SpecContext context)
{
    [Given("an async delegate")]
    public void GivenAnAsyncDelegate()
    {
        var asyncDelegate = Substitute.For<Func<int, Task>>();
        context.SetSubject(asyncDelegate);
    }

    [Then("the async delegate should be invoked with value {int}")]
    public void ThenTheAsyncDelegateShouldBeInvokedWithValue(int value)
    {
        var asyncDelegate = context.Subject<Func<int, Task>>();
        asyncDelegate.Received(1).Invoke(value);
    }

    [Then("the async delegate should not be invoked")]
    public void ThenTheAsyncDelegateShouldNotBeInvoked()
    {
        var asyncDelegate = context.Subject<Func<int, Task>>();
        asyncDelegate.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Given("a synchronous delegate")]
    public void GivenASynchronousDelegate()
    {
        var syncDelegate = Substitute.For<Action<int>>();
        context.SetSubject(syncDelegate);
    }

    [Then("the synchronous delegate should be invoked with value {int}")]
    public void ThenTheSynchronousDelegateShouldBeInvokedWithValue(int value)
    {
        var syncDelegate = context.Subject<Action<int>>();
        syncDelegate.Received(1).Invoke(value);
    }

    [Then("the synchronous delegate should not be invoked")]
    public void ThenTheSynchronousDelegateShouldNotBeInvoked()
    {
        var syncDelegate = context.Subject<Action<int>>();
        syncDelegate.DidNotReceive().Invoke(Arg.Any<int>());
    }

    [Given("an async Error delegate that returns {string}")]
    public void GivenAnAsyncErrorDelegateThatReturns(string value)
    {
        var asyncErrDelegate = Substitute.For<Func<Task<string>>>();
        asyncErrDelegate.Invoke().Returns(Task.FromResult(value));
        context.SetSlot(asyncErrDelegate, SpecContext.AsyncErrorSlot);
    }

    [Given("a synchronous Error delegate that returns {string}")]
    public void GivenASynchronousErrorDelegateThatReturns(string value)
    {
        var syncErrDelegate = Substitute.For<Func<string>>();
        syncErrDelegate.Invoke().Returns(value);
        context.SetSlot(syncErrDelegate, SpecContext.SyncErrorSlot);
    }

    [Given("an async delegate that returns an OK result with value {int}")]
    public void GivenAnAsyncDelegateThatReturnsAnOkResultWithValue(int value)
    {
        var func = Substitute.For<Func<int, ValueTask<Result<int, string>>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(
                new ValueTask<Result<int, string>>(
                    Result.Ok<int, string>(value)));

        context.SetSlot(func, SpecContext.AsyncOkSlot);
    }

    [Given(
        "an async delegate that returns an Error result with message {string}")]
    public void GivenAnAsyncDelegateThatReturnsAnErrorResultWithMessage(
        string message)
    {
        var func = Substitute.For<Func<int, ValueTask<Result<int, string>>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(
                new ValueTask<Result<int, string>>(
                    Result.Err<int, string>(message)));

        context.SetSlot(func, SpecContext.AsyncErrorSlot);
    }

    [Given("a sync delegate that returns an OK result with value {int}")]
    public void GivenASyncDelegateThatReturnsAnOkResultWithValue(int value)
    {
        var func = Substitute.For<Func<int, Result<int, string>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(Result.Ok<int, string>(value));

        context.SetSlot(func, SpecContext.SyncOkSlot);
    }

    [Given(
        "a sync delegate that returns an Error result with message {string}")]
    public void GivenASyncDelegateThatReturnsAnErrorResultWithMessage(string message)
    {
        var func = Substitute.For<Func<int, Result<int, string>>>();

        func.Invoke(Arg.Any<int>())
           .Returns(Result.Err<int, string>(message));

        context.SetSlot(func, SpecContext.SyncErrorSlot);
    }

    [Then("the async delegate should not have been invoked")]
    public void ThenTheAsyncDelegateShouldNotHaveBeenInvoked()
    {
        var func = context.Subject<Func<string, Task>>();

        func.DidNotReceive().Invoke(Arg.Any<string>());
    }

    [Then(
        "the async delegate should have been invoked once with message {string}")]
    public void ThenTheAsyncDelegateShouldHaveBeenInvokedOnceWithMessage(
        string message)
    {
        var func = context.Subject<Func<string, Task>>();

        func.Received(1).Invoke(message);
    }

    [Given("an async delegate for string returning Task")]
    public void GivenAnAsyncDelegateForStringReturningTask()
    {
        var asyncDelegate = Substitute.For<Func<string, Task>>();
        context.SetSubject(asyncDelegate);
    }

    [Given("an async factory that returns {string}")]
    public void GivenAnAsyncFactoryThatReturns(string missing)
    {
        var asyncFactory = Substitute.For<Func<string, Task<string>>>();

        asyncFactory.Invoke(Arg.Any<string>())
           .Returns(Task.FromResult(missing));

        context.SetSlot(asyncFactory, SpecContext.AsyncErrorSlot);
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

        context.SetSlot(asyncMap, SpecContext.AsyncOkSlot);
    }

    [Given("a sync factory that returns {string}")]
    public void GivenASyncFactoryThatReturns(string value)
    {
        var syncFactory = Substitute.For<Func<string, string>>();

        syncFactory.Invoke(Arg.Any<string>()).Returns(value);

        context.SetSlot(syncFactory, SpecContext.SyncErrorSlot);
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

        context.SetSlot(syncMap, SpecContext.SyncOkSlot);
    }

    [Given("an {string} {string} handler that returns no value")]
    public void GivenAnHandlerThatReturnsNoValue(string variant, string handler)
    {
        switch (variant)
        {
            case "async" when handler == "Ok":
            {
                var asyncOkHandler = Substitute.For<Func<int, Task>>();

                context.SetSlot(asyncOkHandler, SpecContext.AsyncOkSlot);

                break;
            }
            case "async" when handler == "Error":
            {
                var asyncErrHandler = Substitute.For<Func<string, Task>>();

                context.SetSlot(asyncErrHandler, SpecContext.AsyncErrorSlot);

                break;
            }
            case "sync" when handler == "Ok":
            {
                var syncOkHandler = Substitute.For<Action<int>>();

                context.SetSlot(syncOkHandler, SpecContext.SyncOkSlot);

                break;
            }
            case "sync" when handler == "Error":
            {
                var syncErrHandler = Substitute.For<Action<string>>();

                context.SetSlot(syncErrHandler, SpecContext.SyncErrorSlot);

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
                    context.Slot<Func<int, Task>>(SpecContext.AsyncOkSlot);

                asyncOkHandler.Received(1).Invoke(value);

                break;
            }
            case "sync" when handler == "Ok":
            {
                var syncOkHandler =
                    context.Slot<Action<int>>(SpecContext.SyncOkSlot);

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
            case "async" when handler == "Ok":
            {
                var asyncOkHandler =
                    context.Slot<Func<int, Task>>(SpecContext.AsyncOkSlot);

                asyncOkHandler.DidNotReceive().Invoke(Arg.Any<int>());

                break;
            }
            case "sync" when handler == "Ok":
            {
                var syncOkHandler =
                    context.Slot<Action<int>>(SpecContext.SyncOkSlot);

                syncOkHandler.DidNotReceive().Invoke(Arg.Any<int>());

                break;
            }
            case "async" when handler == "Error":
            {
                var asyncErrHandler =
                    context.Slot<Func<string, Task>>(SpecContext.AsyncErrorSlot);

                asyncErrHandler.DidNotReceive().Invoke(Arg.Any<string>());

                break;
            }
            case "sync" when handler == "Error":
            {
                var syncErrHandler =
                    context.Slot<Action<string>>(SpecContext.SyncErrorSlot);

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
                    context.Slot<Func<string, Task>>(SpecContext.AsyncErrorSlot);

                asyncErrHandler.Received(1).Invoke(value);

                break;
            }
            case "sync" when handler == "Error":
            {
                var syncErrHandler =
                    context.Slot<Action<string>>(SpecContext.SyncErrorSlot);

                syncErrHandler.Received(1).Invoke(value);

                break;
            }
            default:
                throw new NotImplementedException(
                    "Handler not implemented: " + variant + " " + handler);
        }
    }

}
