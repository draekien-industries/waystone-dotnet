namespace Waystone.Monads.Specs.Results.Steps;

using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;
using Waystone.Monads.Results;

[Binding]
public class MapOrElseExtensionsSteps(SpecContext context)
{
    [When(
        "invoking MapOrElse with the async factory and async map on the sync Result")]
    public async Task
        WhenInvokingMapOrElseWithTheAsyncFactoryAndAsyncMapOnTheSyncResult()
    {
        var result = context.Subject<Result<int, string>>();

        var factory =
            context.Slot<Func<string, Task<string>>>(SpecContext.AsyncErrorSlot);

        var map =
            context.Slot<Func<int, Task<string>>>(SpecContext.AsyncOkSlot);

        string output = await result.MapOrElseAsync(factory, map);

        context.SetOutcome(output);
    }

    [When(
        "invoking MapOrElse with the async factory and async map on the async Result")]
    public async Task
        WhenInvokingMapOrElseWithTheAsyncFactoryAndAsyncMapOnTheAsyncResult()
    {
        var taskResult = context.Subject<Task<Result<int, string>>>();

        var factory =
            context.Slot<Func<string, Task<string>>>(SpecContext.AsyncErrorSlot);

        var map =
            context.Slot<Func<int, Task<string>>>(SpecContext.AsyncOkSlot);

        string output = await taskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When(
        "invoking MapOrElse with the sync factory and async map on the async Result")]
    public async Task
        WhenInvokingMapOrElseWithTheSyncFactoryAndAsyncMapOnTheAsyncResult()
    {
        var taskResult = context.Subject<Task<Result<int, string>>>();

        var factory =
            context.Slot<Func<string, string>>(SpecContext.SyncErrorSlot);

        var map =
            context.Slot<Func<int, Task<string>>>(SpecContext.AsyncOkSlot);

        string output = await taskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When(
        "invoking MapOrElse with the async factory and sync map on the async Result")]
    public async Task
        WhenInvokingMapOrElseWithTheAsyncFactoryAndSyncMapOnTheAsyncResult()
    {
        var taskResult = context.Subject<Task<Result<int, string>>>();

        var factory =
            context.Slot<Func<string, Task<string>>>(SpecContext.AsyncErrorSlot);

        var map =
            context.Slot<Func<int, string>>(SpecContext.SyncOkSlot);

        string output = await taskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When(
        "invoking MapOrElse with the sync factory and sync map on the async Result")]
    public async Task
        WhenInvokingMapOrElseWithTheSyncFactoryAndSyncMapOnTheAsyncResult()
    {
        var taskResult = context.Subject<Task<Result<int, string>>>();

        var factory =
            context.Slot<Func<string, string>>(SpecContext.SyncErrorSlot);

        var map =
            context.Slot<Func<int, string>>(SpecContext.SyncOkSlot);

        string output = await taskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When(
        "invoking MapOrElse with the async factory and async map on the ValueTask Result")]
    public async Task
        WhenInvokingMapOrElseWithTheAsyncFactoryAndAsyncMapOnTheValueTaskResult()
    {
        var valueTaskResult = context.Subject<ValueTask<Result<int, string>>>();

        var factory =
            context.Slot<Func<string, Task<string>>>(SpecContext.AsyncErrorSlot);

        var map =
            context.Slot<Func<int, Task<string>>>(SpecContext.AsyncOkSlot);

        string output = await valueTaskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When(
        "invoking MapOrElse with the sync factory and sync map on the ValueTask Result")]
    public async Task
        WhenInvokingMapOrElseWithTheSyncFactoryAndSyncMapOnTheValueTaskResult()
    {
        var valueTaskResult = context.Subject<ValueTask<Result<int, string>>>();

        var factory =
            context.Slot<Func<string, string>>(SpecContext.SyncErrorSlot);

        var map =
            context.Slot<Func<int, string>>(SpecContext.SyncOkSlot);

        string output = await valueTaskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When(
        "invoking MapOrElse with the async factory and sync map on the ValueTask Result")]
    public async Task
        WhenInvokingMapOrElseWithTheAsyncFactoryAndSyncMapOnTheValueTaskResult()
    {
        var valueTaskResult = context.Subject<ValueTask<Result<int, string>>>();

        var factory =
            context.Slot<Func<string, Task<string>>>(SpecContext.AsyncErrorSlot);

        var map =
            context.Slot<Func<int, string>>(SpecContext.SyncOkSlot);

        string output = await valueTaskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }

    [When(
        "invoking MapOrElse with the sync factory and async map on the ValueTask Result")]
    public async Task
        WhenInvokingMapOrElseWithTheSyncFactoryAndAsyncMapOnTheValueTaskResult()
    {
        var valueTaskResult = context.Subject<ValueTask<Result<int, string>>>();

        var factory =
            context.Slot<Func<string, string>>(SpecContext.SyncErrorSlot);

        var map =
            context.Slot<Func<int, Task<string>>>(SpecContext.AsyncOkSlot);

        string output = await valueTaskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.SetOutcome(output);
    }
}
