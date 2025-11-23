namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Reqnroll;

[Binding, TestSubject(typeof(MapOrExtensions))]
public class MapOrExtensionsSteps(ScenarioContext context)
{
    [Given("async MapOr returns {string} + value")]
    public void GivenAsyncMapOrReturnsValue(string mapped)
    {
        context.Set<Func<int, Task<string>>>(
            value => Task.FromResult($"{mapped}{value}"),
            Constants.MapKey);
    }

    [Given("sync MapOr returns {string} + value")]
    public void GivenSyncMapOrReturnsValue(string syncMapped)
    {
        context.Set<Func<int, string>>(
            value => $"{syncMapped}{value}",
            Constants.MapKey);
    }

    [When("Option Task is invoked with {string} MapOr and default {string}")]
    public async Task WhenOptionTaskIsInvokedWithMapOrAndDefault(
        string mapType,
        string defaultValue)
    {
        var option = context.Get<Task<Option<int>>>();

        switch (mapType)
        {
            case "async":
            {
                var mapFunc =
                    context.Get<Func<int, Task<string>>>(Constants.MapKey);

                string result = await option.MapOr(defaultValue, mapFunc);
                context.Set(result, Constants.ResultKey);

                break;
            }
            case "sync":
            {
                var mapFunc = context.Get<Func<int, string>>(Constants.MapKey);
                string result = await option.MapOr(defaultValue, mapFunc);
                context.Set(result, Constants.ResultKey);

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(mapType),
                    mapType,
                    null);
        }
    }

    [When(
        "Option ValueTask is invoked with {string} MapOr and default {string}")]
    public async Task WhenOptionValueTaskIsInvokedWithMapOrAndDefault(
        string mapType,
        string defaultValue)
    {
        var option = context.Get<ValueTask<Option<int>>>();

        switch (mapType)
        {
            case "async":
            {
                var mapFunc =
                    context.Get<Func<int, Task<string>>>(Constants.MapKey);

                string result = await option.MapOr(defaultValue, mapFunc);
                context.Set(result, Constants.ResultKey);

                break;
            }
            case "sync":
            {
                var mapFunc = context.Get<Func<int, string>>(Constants.MapKey);
                string result = await option.MapOr(defaultValue, mapFunc);
                context.Set(result, Constants.ResultKey);

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(mapType),
                    mapType,
                    null);
        }
    }
}
