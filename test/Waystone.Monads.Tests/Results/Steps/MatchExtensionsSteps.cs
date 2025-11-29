namespace Waystone.Monads.Results.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using Reqnroll;

[Binding]
public class MatchExtensionsSteps(ScenarioContext context)
{
    [When(
        "invoking MatchAsync with the {string} OK handler and {string} Error handler on the result {string}")]
    public async Task
        WhenInvokingMatchAsyncWithTheOkHandlerAndErrorHandlerOnTheResult(
            string okHandler,
            string errHandler,
            string task)
    {
        switch (task)
        {
            case "Task" when okHandler == "async" && errHandler == "async":
            {
                var taskResult = context.Get<Task<Result<int, string>>>();

                var asyncOkHandler =
                    context.Get<Func<int, Task>>(Constants.AsyncOkDelegate);

                var asyncErrHandler =
                    context.Get<Func<string, Task>>(
                        Constants.AsyncErrorDelegate);

                await taskResult.MatchAsync(asyncOkHandler, asyncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "Task" when okHandler == "async" && errHandler == "sync":
            {
                var taskResult = context.Get<Task<Result<int, string>>>();

                var asyncOkHandler =
                    context.Get<Func<int, Task>>(Constants.AsyncOkDelegate);

                var syncErrHandler =
                    context.Get<Action<string>>(Constants.SyncErrorDelegate);

                await taskResult.MatchAsync(asyncOkHandler, syncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "Task" when okHandler == "sync" && errHandler == "async":
            {
                var taskResult = context.Get<Task<Result<int, string>>>();

                var syncOkHandler =
                    context.Get<Action<int>>(Constants.SyncOkDelegate);

                var asyncErrHandler =
                    context.Get<Func<string, Task>>(
                        Constants.AsyncErrorDelegate);

                await taskResult.MatchAsync(syncOkHandler, asyncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "Task" when okHandler == "sync" && errHandler == "sync":
            {
                var taskResult = context.Get<Task<Result<int, string>>>();

                var syncOkHandler =
                    context.Get<Action<int>>(Constants.SyncOkDelegate);

                var syncErrHandler =
                    context.Get<Action<string>>(Constants.SyncErrorDelegate);

                await taskResult.MatchAsync(syncOkHandler, syncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "ValueTask" when okHandler == "async" && errHandler == "async":
            {
                var taskResult = context.Get<ValueTask<Result<int, string>>>();

                var asyncOkHandler =
                    context.Get<Func<int, Task>>(Constants.AsyncOkDelegate);

                var asyncErrHandler =
                    context.Get<Func<string, Task>>(
                        Constants.AsyncErrorDelegate);

                await taskResult.MatchAsync(asyncOkHandler, asyncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "ValueTask" when okHandler == "async" && errHandler == "sync":
            {
                var taskResult = context.Get<ValueTask<Result<int, string>>>();

                var asyncOkHandler =
                    context.Get<Func<int, Task>>(Constants.AsyncOkDelegate);

                var syncErrHandler =
                    context.Get<Action<string>>(Constants.SyncErrorDelegate);

                await taskResult.MatchAsync(asyncOkHandler, syncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "ValueTask" when okHandler == "sync" && errHandler == "async":
            {
                var taskResult = context.Get<ValueTask<Result<int, string>>>();

                var syncOkHandler =
                    context.Get<Action<int>>(Constants.SyncOkDelegate);

                var asyncErrHandler =
                    context.Get<Func<string, Task>>(
                        Constants.AsyncErrorDelegate);

                await taskResult.MatchAsync(syncOkHandler, asyncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "ValueTask" when okHandler == "sync" && errHandler == "sync":
            {
                var taskResult =
                    context.Get<ValueTask<Result<int, string>>>();

                var syncOkHandler =
                    context.Get<Action<int>>(Constants.SyncOkDelegate);

                var syncErrHandler =
                    context.Get<Action<string>>(Constants.SyncErrorDelegate);

                await taskResult.MatchAsync(syncOkHandler, syncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
        }
    }
}
