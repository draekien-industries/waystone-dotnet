namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(MapOrElseExtensions))]
public class MapOrElseExtensionsSteps(SpecContext context)
{
    [Given("async Else returns {string}")]
    public void GivenAsyncElseReturnsString(string fallback)
    {
        context.SetSlot<Func<Task<string>>>(async () =>
            {
                await Task.Yield();

                return fallback;
            }, SpecContext.ElseSlot);
    }

    [Given("async Map returns {string} + value")]
    public void GivenAsyncMapReturnsStringValue(string mapped)
    {
        context.SetSlot<Func<int, Task<string>>>(async value =>
            {
                await Task.Yield();

                return mapped + value;
            }, SpecContext.MapSlot);
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
                var optionTask = context.Subject<Task<Option<int>>>();

                var elseFunc =
                    context.Slot<Func<Task<string>>>(SpecContext.ElseSlot);

                var mapFunc =
                    context.Slot<Func<int, Task<string>>>(SpecContext.MapSlot);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.SetOutcome(result);

                break;
            }
            case ("sync", "sync"):
            {
                var optionTask = context.Subject<Task<Option<int>>>();
                var elseFunc = context.Slot<Func<string>>(SpecContext.ElseSlot);
                var mapFunc = context.Slot<Func<int, string>>(SpecContext.MapSlot);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.SetOutcome(result);

                break;
            }
            case ("async", "sync"):
            {
                var optionTask = context.Subject<Task<Option<int>>>();

                var elseFunc =
                    context.Slot<Func<Task<string>>>(SpecContext.ElseSlot);

                var mapFunc = context.Slot<Func<int, string>>(SpecContext.MapSlot);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.SetOutcome(result);

                break;
            }
            case ("sync", "async"):
            {
                var optionTask = context.Subject<Task<Option<int>>>();
                var elseFunc = context.Slot<Func<string>>(SpecContext.ElseSlot);

                var mapFunc =
                    context.Slot<Func<int, Task<string>>>(SpecContext.MapSlot);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.SetOutcome(result);

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
        context.SetSlot<Func<string>>(() => syncFallback, SpecContext.ElseSlot);
    }

    [Given("sync Map returns {string} + value")]
    public void GivenSyncMapReturnsValue(string syncMapped)
    {
        context.SetSlot<Func<int, string>>(value => syncMapped + value, SpecContext.MapSlot);
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
                var optionTask = context.Subject<ValueTask<Option<int>>>();

                var elseFunc =
                    context.Slot<Func<Task<string>>>(SpecContext.ElseSlot);

                var mapFunc =
                    context.Slot<Func<int, Task<string>>>(SpecContext.MapSlot);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.SetOutcome(result);

                break;
            }
            case ("sync", "sync"):
            {
                var optionTask = context.Subject<ValueTask<Option<int>>>();
                var elseFunc = context.Slot<Func<string>>(SpecContext.ElseSlot);
                var mapFunc = context.Slot<Func<int, string>>(SpecContext.MapSlot);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.SetOutcome(result);

                break;
            }
            case ("async", "sync"):
            {
                var optionTask = context.Subject<ValueTask<Option<int>>>();

                var elseFunc =
                    context.Slot<Func<Task<string>>>(SpecContext.ElseSlot);

                var mapFunc = context.Slot<Func<int, string>>(SpecContext.MapSlot);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.SetOutcome(result);

                break;
            }
            case ("sync", "async"):
            {
                var optionTask = context.Subject<ValueTask<Option<int>>>();
                var elseFunc = context.Slot<Func<string>>(SpecContext.ElseSlot);

                var mapFunc =
                    context.Slot<Func<int, Task<string>>>(SpecContext.MapSlot);

                string result =
                    await optionTask.MapOrElseAsync(elseFunc, mapFunc);

                context.SetOutcome(result);

                break;
            }
            default:
                throw new InvalidOperationException(
                    "Invalid combination of else and map types.");
        }
    }
}
