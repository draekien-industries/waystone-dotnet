namespace Waystone.Monads.Results.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using Reqnroll;

[Binding]
public class MapOrElseExtensionsSteps(ScenarioContext context)
{
    [When(
        "invoking MapOrElse with the async factory and async map on the sync Result")]
    public async Task
        WhenInvokingMapOrElseWithTheAsyncFactoryAndAsyncMapOnTheSyncResult()
    {
        var result = context.Get<Result<int, string>>();

        var factory =
            context.Get<Func<string, Task<string>>>(
                Constants.AsyncErrorDelegate);

        var map =
            context.Get<Func<int, Task<string>>>(Constants.AsyncOkDelegate);

        string output = await result.MapOrElseAsync(factory, map);

        context.Set(output, Constants.ResultKey);
    }

    [When(
        "invoking MapOrElse with the async factory and async map on the async Result")]
    public async Task
        WhenInvokingMapOrElseWithTheAsyncFactoryAndAsyncMapOnTheAsyncResult()
    {
        var taskResult = context.Get<Task<Result<int, string>>>();

        var factory =
            context.Get<Func<string, Task<string>>>(
                Constants.AsyncErrorDelegate);

        var map =
            context.Get<Func<int, Task<string>>>(Constants.AsyncOkDelegate);

        string output = await taskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [When(
        "invoking MapOrElse with the sync factory and async map on the async Result")]
    public async Task
        WhenInvokingMapOrElseWithTheSyncFactoryAndAsyncMapOnTheAsyncResult()
    {
        var taskResult = context.Get<Task<Result<int, string>>>();

        var factory =
            context.Get<Func<string, string>>(Constants.SyncErrorDelegate);

        var map =
            context.Get<Func<int, Task<string>>>(Constants.AsyncOkDelegate);

        string output = await taskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [When(
        "invoking MapOrElse with the async factory and sync map on the async Result")]
    public async Task
        WhenInvokingMapOrElseWithTheAsyncFactoryAndSyncMapOnTheAsyncResult()
    {
        var taskResult = context.Get<Task<Result<int, string>>>();

        var factory =
            context.Get<Func<string, Task<string>>>(
                Constants.AsyncErrorDelegate);

        var map =
            context.Get<Func<int, string>>(Constants.SyncOkDelegate);

        string output = await taskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }

    [When(
        "invoking MapOrElse with the sync factory and sync map on the async Result")]
    public async Task
        WhenInvokingMapOrElseWithTheSyncFactoryAndSyncMapOnTheAsyncResult()
    {
        var taskResult = context.Get<Task<Result<int, string>>>();

        var factory =
            context.Get<Func<string, string>>(Constants.SyncErrorDelegate);

        var map =
            context.Get<Func<int, string>>(Constants.SyncOkDelegate);

        string output = await taskResult.MapOrElseAsync(factory, map)
           .ConfigureAwait(false);

        context.Set(output, Constants.ResultKey);
    }
}
