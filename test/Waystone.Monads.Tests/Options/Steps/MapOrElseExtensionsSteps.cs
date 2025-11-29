namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Reqnroll;

[Binding, TestSubject(typeof(MapOrElseExtensions))]
public class MapOrElseExtensionsSteps(ScenarioContext context)
{
    [Given("async Else returns {string}")]
    public void GivenAsyncElseReturnsString(string fallback)
    {
        context.Set<Func<Task<string>>>(
            async () =>
            {
                await Task.Yield();

                return fallback;
            },
            Constants.ElseKey);
    }

    [Given("async Map returns {string} + value")]
    public void GivenAsyncMapReturnsStringValue(string mapped)
    {
        context.Set<Func<int, Task<string>>>(
            async value =>
            {
                await Task.Yield();

                return mapped + value;
            },
            Constants.MapKey);
    }

    [When("MapOrElse Task is invoked with {string} Else and {string} Map")]
    public async Task WhenMapOrElseIsInvokedWithStringElseAndStringMap(
        string elseType,
        string mapType)
    {
        switch (elseType, mapType)
        {
            case ("async", "async"):
            {
                var optionTask = context.Get<Task<Option<int>>>();

                var elseFunc =
                    context.Get<Func<Task<string>>>(Constants.ElseKey);

                var mapFunc =
                    context.Get<Func<int, Task<string>>>(Constants.MapKey);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.Set(result, Constants.ResultKey);

                break;
            }
            case ("sync", "sync"):
            {
                var optionTask = context.Get<Task<Option<int>>>();
                var elseFunc = context.Get<Func<string>>(Constants.ElseKey);
                var mapFunc = context.Get<Func<int, string>>(Constants.MapKey);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.Set(result, Constants.ResultKey);

                break;
            }
            case ("async", "sync"):
            {
                var optionTask = context.Get<Task<Option<int>>>();

                var elseFunc =
                    context.Get<Func<Task<string>>>(Constants.ElseKey);

                var mapFunc = context.Get<Func<int, string>>(Constants.MapKey);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.Set(result, Constants.ResultKey);

                break;
            }
            case ("sync", "async"):
            {
                var optionTask = context.Get<Task<Option<int>>>();
                var elseFunc = context.Get<Func<string>>(Constants.ElseKey);

                var mapFunc =
                    context.Get<Func<int, Task<string>>>(Constants.MapKey);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.Set(result, Constants.ResultKey);

                break;
            }
            default:
                throw new InvalidOperationException(
                    "Invalid combination of else and map types.");
        }
    }

    [Given("sync Else returns {string}")]
    public void GivenSyncElseReturns(string syncFallback)
    {
        context.Set<Func<string>>(() => syncFallback, Constants.ElseKey);
    }

    [Given("sync Map returns {string} + value")]
    public void GivenSyncMapReturnsValue(string syncMapped)
    {
        context.Set<Func<int, string>>(
            value => syncMapped + value,
            Constants.MapKey);
    }

    [When("MapOrElse ValueTask is invoked with {string} Else and {string} Map")]
    public async Task WhenMapOrElseValueTaskIsInvokedWithElseAndMap(
        string elseType,
        string mapType)
    {
        switch (elseType, mapType)
        {
            case ("async", "async"):
            {
                var optionTask = context.Get<ValueTask<Option<int>>>();

                var elseFunc =
                    context.Get<Func<Task<string>>>(Constants.ElseKey);

                var mapFunc =
                    context.Get<Func<int, Task<string>>>(Constants.MapKey);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.Set(result, Constants.ResultKey);

                break;
            }
            case ("sync", "sync"):
            {
                var optionTask = context.Get<ValueTask<Option<int>>>();
                var elseFunc = context.Get<Func<string>>(Constants.ElseKey);
                var mapFunc = context.Get<Func<int, string>>(Constants.MapKey);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.Set(result, Constants.ResultKey);

                break;
            }
            case ("async", "sync"):
            {
                var optionTask = context.Get<ValueTask<Option<int>>>();

                var elseFunc =
                    context.Get<Func<Task<string>>>(Constants.ElseKey);

                var mapFunc = context.Get<Func<int, string>>(Constants.MapKey);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.Set(result, Constants.ResultKey);

                break;
            }
            case ("sync", "async"):
            {
                var optionTask = context.Get<ValueTask<Option<int>>>();
                var elseFunc = context.Get<Func<string>>(Constants.ElseKey);

                var mapFunc =
                    context.Get<Func<int, Task<string>>>(Constants.MapKey);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.Set(result, Constants.ResultKey);

                break;
            }
            default:
                throw new InvalidOperationException(
                    "Invalid combination of else and map types.");
        }
    }
}
