namespace Waystone.Monads.Options.Steps;

using System;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Reqnroll;

[Binding, TestSubject(typeof(MapExtensions))]
public class MapExtensionsSteps(ScenarioContext context)
{
    [When("Option Task is invoked with {string} Map")]
    public async Task WhenOptionTaskIsInvokedWithStringMap(string mapType)
    {
        var option = context.Get<Task<Option<int>>>();

        switch (mapType)
        {
            case "async":
            {
                var map =
                    context.Get<Func<int, Task<string>>>(Constants.MapKey);

                Option<string> result =
                    await option.Map(map).ConfigureAwait(false);

                context.Set(result, Constants.ResultKey);

                break;
            }
            case "sync":
            {
                var map = context.Get<Func<int, string>>(Constants.MapKey);

                Option<string> result =
                    await option.Map(map).ConfigureAwait(false);

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

    [When("Option ValueTask is invoked with {string} Map")]
    public async Task WhenOptionValueTaskIsInvokedWithStringMap(string mapType)
    {
        var option = context.Get<ValueTask<Option<int>>>();

        switch (mapType)
        {
            case "async":
            {
                var map =
                    context.Get<Func<int, Task<string>>>(Constants.MapKey);

                Option<string> result =
                    await option.Map(map).ConfigureAwait(false);

                context.Set(result, Constants.ResultKey);

                return;
            }
            case "sync":
            {
                var map = context.Get<Func<int, string>>(Constants.MapKey);

                Option<string> result =
                    await option.Map(map).ConfigureAwait(false);

                context.Set(result, Constants.ResultKey);

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
