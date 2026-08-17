namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(MapOrExtensions))]
public class MapOrExtensionsSteps(SpecContext context)
{
    [Given("async MapOr returns {string} + value")]
    public void GivenAsyncMapOrReturnsValue(string mapped)
    {
        context.SetSlot<Func<int, Task<string>>>(value => Task.FromResult($"{mapped}{value}"), SpecContext.MapSlot);
    }

    [Given("sync MapOr returns {string} + value")]
    public void GivenSyncMapOrReturnsValue(string syncMapped)
    {
        context.SetSlot<Func<int, string>>(value => $"{syncMapped}{value}", SpecContext.MapSlot);
    }

    [When("Option Task is invoked with {string} MapOr and default {string}")]
    public async Task WhenOptionTaskIsInvokedWithMapOrAndDefault(
        string mapType,
        string defaultValue)
    {
        var option = context.Subject<Task<Option<int>>>();

        switch (mapType)
        {
            case "async":
            {
                var mapFunc =
                    context.Slot<Func<int, Task<string>>>(SpecContext.MapSlot);

                string result = await option.MapOrAsync(defaultValue, mapFunc);
                context.SetOutcome(result);

                break;
            }
            case "sync":
            {
                var mapFunc = context.Slot<Func<int, string>>(SpecContext.MapSlot);
                string result = await option.MapOrAsync(defaultValue, mapFunc);
                context.SetOutcome(result);

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
        var option = context.Subject<ValueTask<Option<int>>>();

        switch (mapType)
        {
            case "async":
            {
                var mapFunc =
                    context.Slot<Func<int, Task<string>>>(SpecContext.MapSlot);

                string result = await option.MapOrAsync(defaultValue, mapFunc);
                context.SetOutcome(result);

                break;
            }
            case "sync":
            {
                var mapFunc = context.Slot<Func<int, string>>(SpecContext.MapSlot);
                string result = await option.MapOrAsync(defaultValue, mapFunc);
                context.SetOutcome(result);

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
