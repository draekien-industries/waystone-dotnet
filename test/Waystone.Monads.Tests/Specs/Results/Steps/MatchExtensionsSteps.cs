namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public class MatchExtensionsSteps(SpecContext context)
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
                var taskResult = context.Subject<Task<Result<int, string>>>();

                var asyncOkHandler =
                    context.Slot<Func<int, Task>>(SpecContext.AsyncOkSlot);

                var asyncErrHandler =
                    context.Slot<Func<string, Task>>(SpecContext.AsyncErrorSlot);

                await taskResult.MatchAsync(asyncOkHandler, asyncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "Task" when okHandler == "async" && errHandler == "sync":
            {
                var taskResult = context.Subject<Task<Result<int, string>>>();

                var asyncOkHandler =
                    context.Slot<Func<int, Task>>(SpecContext.AsyncOkSlot);

                var syncErrHandler =
                    context.Slot<Action<string>>(SpecContext.SyncErrorSlot);

                await taskResult.MatchAsync(asyncOkHandler, syncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "Task" when okHandler == "sync" && errHandler == "async":
            {
                var taskResult = context.Subject<Task<Result<int, string>>>();

                var syncOkHandler =
                    context.Slot<Action<int>>(SpecContext.SyncOkSlot);

                var asyncErrHandler =
                    context.Slot<Func<string, Task>>(SpecContext.AsyncErrorSlot);

                await taskResult.MatchAsync(syncOkHandler, asyncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "Task" when okHandler == "sync" && errHandler == "sync":
            {
                var taskResult = context.Subject<Task<Result<int, string>>>();

                var syncOkHandler =
                    context.Slot<Action<int>>(SpecContext.SyncOkSlot);

                var syncErrHandler =
                    context.Slot<Action<string>>(SpecContext.SyncErrorSlot);

                await taskResult.MatchAsync(syncOkHandler, syncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "ValueTask" when okHandler == "async" && errHandler == "async":
            {
                var taskResult = context.Subject<ValueTask<Result<int, string>>>();

                var asyncOkHandler =
                    context.Slot<Func<int, Task>>(SpecContext.AsyncOkSlot);

                var asyncErrHandler =
                    context.Slot<Func<string, Task>>(SpecContext.AsyncErrorSlot);

                await taskResult.MatchAsync(asyncOkHandler, asyncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "ValueTask" when okHandler == "async" && errHandler == "sync":
            {
                var taskResult = context.Subject<ValueTask<Result<int, string>>>();

                var asyncOkHandler =
                    context.Slot<Func<int, Task>>(SpecContext.AsyncOkSlot);

                var syncErrHandler =
                    context.Slot<Action<string>>(SpecContext.SyncErrorSlot);

                await taskResult.MatchAsync(asyncOkHandler, syncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "ValueTask" when okHandler == "sync" && errHandler == "async":
            {
                var taskResult = context.Subject<ValueTask<Result<int, string>>>();

                var syncOkHandler =
                    context.Slot<Action<int>>(SpecContext.SyncOkSlot);

                var asyncErrHandler =
                    context.Slot<Func<string, Task>>(SpecContext.AsyncErrorSlot);

                await taskResult.MatchAsync(syncOkHandler, asyncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            case "ValueTask" when okHandler == "sync" && errHandler == "sync":
            {
                var taskResult =
                    context.Subject<ValueTask<Result<int, string>>>();

                var syncOkHandler =
                    context.Slot<Action<int>>(SpecContext.SyncOkSlot);

                var syncErrHandler =
                    context.Slot<Action<string>>(SpecContext.SyncErrorSlot);

                await taskResult.MatchAsync(syncOkHandler, syncErrHandler)
                   .ConfigureAwait(false);

                break;
            }
            default:
                throw new NotImplementedException(
                    "Combination not implemented: "
                  + task
                  + " "
                  + okHandler
                  + " "
                  + errHandler);
        }
    }
}
