namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using Reqnroll;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(IsSomeAndExtensions))]
public class IsSomeAndExtensionsSteps(SpecContext context)
{
    [When("invoking IsSomeAnd on Option Task with the async predicate")]
    public async Task WhenInvokingIsSomeAndOnOptionTaskWithTheAsyncPredicate()
    {
        var optionTask = context.Subject<Task<Option<int>>>();
        var predicate = context.Subject<Func<int, Task<bool>>>();

        bool result = await optionTask.IsSomeAndAsync(predicate);

        context.SetOutcome(result);
    }

    [When("invoking IsSomeAnd on Option ValueTask with the async predicate")]
    public async Task
        WhenInvokingIsSomeAndOnOptionValueTaskWithTheAsyncPredicate()
    {
        var optionTask = context.Subject<ValueTask<Option<int>>>();
        var predicate = context.Subject<Func<int, Task<bool>>>();

        bool result = await optionTask.IsSomeAndAsync(predicate);

        context.SetOutcome(result);
    }
}
