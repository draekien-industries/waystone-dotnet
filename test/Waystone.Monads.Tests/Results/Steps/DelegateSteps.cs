namespace Waystone.Monads.Results.Steps;

using System;
using System.Threading.Tasks;
using NSubstitute;
using Reqnroll;

[Binding]
public sealed class DelegateSteps(ScenarioContext context)
{
    [Given("an {string} {string} handler that returns no value")]
    public void GivenAnHandlerThatReturnsNoValue(string async, string ok)
    {
        switch (async)
        {
            case "async" when ok == "Ok":
            {
                var asyncOkHandler = Substitute.For<Func<int, Task>>();

                context.Set(asyncOkHandler, Constants.AsyncOkDelegate);

                break;
            }
            case "async" when ok == "Error":
            {
                var asyncErrHandler = Substitute.For<Func<string, Task>>();

                context.Set(asyncErrHandler, Constants.AsyncErrorDelegate);

                break;
            }
            case "sync" when ok == "Ok":
            {
                var syncOkHandler = Substitute.For<Action<int>>();

                context.Set(syncOkHandler, Constants.SyncOkDelegate);

                break;
            }
            case "sync" when ok == "Error":
            {
                var syncErrHandler = Substitute.For<Action<string>>();

                context.Set(syncErrHandler, Constants.SyncErrorDelegate);

                break;
            }
        }
    }

    [Then(
        "the {string} {string} handler should have been called with value {int}")]
    public void ThenTheHandlerShouldHaveBeenCalledWithValue(
        string async,
        string ok,
        int p2)
    {
        switch (async)
        {
            case "async" when ok == "Ok":
            {
                var asyncOkHandler =
                    context.Get<Func<int, Task>>(Constants.AsyncOkDelegate);

                asyncOkHandler.Received(1).Invoke(p2);

                break;
            }
            case "async" when ok == "Error":
            {
                var asyncErrHandler =
                    context.Get<Func<string, Task>>(
                        Constants.AsyncErrorDelegate);

                asyncErrHandler.Received(1).Invoke("Error");

                break;
            }
            case "sync" when ok == "Ok":
            {
                var syncOkHandler =
                    context.Get<Action<int>>(Constants.SyncOkDelegate);

                syncOkHandler.Received(1).Invoke(p2);

                break;
            }
            case "sync" when ok == "Error":
            {
                var syncErrHandler =
                    context.Get<Action<string>>(Constants.SyncErrorDelegate);

                syncErrHandler.Received(1).Invoke("Error");

                break;
            }
        }
    }

    [Then("the {string} {string} handler should not have been called")]
    public void ThenTheHandlerShouldNotHaveBeenCalled(
        string async,
        string error)
    {
        switch (async)
        {
            case "async" when error == "Ok":
            {
                var asyncOkHandler =
                    context.Get<Func<int, Task>>(Constants.AsyncOkDelegate);

                asyncOkHandler.DidNotReceive().Invoke(Arg.Any<int>());

                break;
            }
            case "async" when error == "Error":
            {
                var asyncErrHandler =
                    context.Get<Func<string, Task>>(
                        Constants.AsyncErrorDelegate);

                asyncErrHandler.DidNotReceive().Invoke(Arg.Any<string>());

                break;
            }
            case "sync" when error == "Ok":
            {
                var syncOkHandler =
                    context.Get<Action<int>>(Constants.SyncOkDelegate);

                syncOkHandler.DidNotReceive().Invoke(Arg.Any<int>());

                break;
            }
            case "sync" when error == "Error":
            {
                var syncErrHandler =
                    context.Get<Action<string>>(Constants.SyncErrorDelegate);

                syncErrHandler.DidNotReceive().Invoke(Arg.Any<string>());

                break;
            }
        }
    }
}
