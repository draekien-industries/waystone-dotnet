namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(MapExtensions))]
public class MapExtensionsSteps(SpecContext context)
{
    [When("Option Task is invoked with {string} Map")]
    public async Task WhenOptionTaskIsInvokedWithStringMap(string mapType)
    {
        var option = context.Subject<Task<Option<int>>>();

        switch (mapType)
        {
            case "async":
            {
                var map =
                    context.Slot<Func<int, Task<string>>>(SpecContext.MapSlot);

                Option<string> result =
                    await option.MapAsync(map).ConfigureAwait(false);

                context.SetOutcome(result);

                break;
            }
            case "sync":
            {
                var map = context.Slot<Func<int, string>>(SpecContext.MapSlot);

                Option<string> result =
                    await option.MapAsync(map).ConfigureAwait(false);

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

    [When("Option ValueTask is invoked with {string} Map")]
    public async Task WhenOptionValueTaskIsInvokedWithStringMap(string mapType)
    {
        var option = context.Subject<ValueTask<Option<int>>>();

        switch (mapType)
        {
            case "async":
            {
                var map =
                    context.Slot<Func<int, Task<string>>>(SpecContext.MapSlot);

                Option<string> result =
                    await option.MapAsync(map).ConfigureAwait(false);

                context.SetOutcome(result);

                return;
            }
            case "sync":
            {
                var map = context.Slot<Func<int, string>>(SpecContext.MapSlot);

                Option<string> result =
                    await option.MapAsync(map).ConfigureAwait(false);

                context.SetOutcome(result);

                return;
            }
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(mapType),
                    mapType,
                    null);
        }
    }
}
