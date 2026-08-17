namespace Waystone.Monads.Specs.Options.Steps;

using JetBrains.Annotations;
using NSubstitute;
using Reqnroll;
using Shouldly;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding, TestSubject(typeof(MatchExtensions))]
public class MatchExtensionsSteps(SpecContext context)
{
    [Given("Option is wrapped in a Task {string}")]
    public void GivenOptionIsWrappedInATaskString(string optionTask)
    {
        var option = context.Subject<Option<int>>();
        Task<Option<int>> taskOption = Task.FromResult(option);
        context.SetSlot(taskOption, optionTask);
    }

    [Given("Option is wrapped in a ValueTask {string}")]
    public void GivenOptionIsWrappedInAValueTaskString(string optionValueTask)
    {
        var option = context.Subject<Option<int>>();
        var taskOption = new ValueTask<Option<int>>(option);
        context.SetSlot(taskOption, optionValueTask);
    }

    [Given("an async {string} function that returns {string}")]
    public void GivenAnAsyncStringFunctionThatReturnsString(
        string funcType,
        string output)
    {
        switch (funcType)
        {
            case "OnSome":
            {
                var func = Substitute.For<Func<int, Task<string>>>();
                func.Invoke(Arg.Any<int>()).Returns(Task.FromResult(output));

                context.SetSlot(func, funcType);

                return;
            }
            case "OnNone":
            {
                var func = Substitute.For<Func<Task<string>>>();
                func.Invoke().Returns(Task.FromResult(output));

                context.SetSlot(func, funcType);

                return;
            }
            default:
                throw new ArgumentException(
                    $"Invalid function type: {funcType}",
                    nameof(funcType));
        }
    }

    [Then("the result should be {string}")]
    public void ThenTheResultShouldBeString(string expected)
    {
        var result = context.Outcome<string>();
        result.ShouldBe(expected);
    }

    [When("invoking Match on Option Task with {string} and {string} handlers")]
    public async Task WhenInvokingMatchOnOptionTaskWithAndHandlers(
        string onSome,
        string onNone)
    {
        var onSomeFunc = context.Slot<Func<int, Task<string>>>(onSome);
        var onNoneFunc = context.Slot<Func<Task<string>>>(onNone);
        var option = context.Subject<Task<Option<int>>>();

        string result = await option.MatchAsync(onSomeFunc, onNoneFunc);

        context.SetOutcome(result);
    }

    [When(
        "invoking Match on Option ValueTask with {string} and {string} handlers")]
    public void WhenInvokingMatchOnOptionValueTaskWithAndHandlers(
        string onSome,
        string onNone)
    {
        var onSomeFunc = context.Slot<Func<int, Task<string>>>(onSome);
        var onNoneFunc = context.Slot<Func<Task<string>>>(onNone);
        var option = context.Subject<ValueTask<Option<int>>>();

        string result = option.MatchAsync(onSomeFunc, onNoneFunc)
           .GetAwaiter()
           .GetResult();

        context.SetOutcome(result);
    }
}
