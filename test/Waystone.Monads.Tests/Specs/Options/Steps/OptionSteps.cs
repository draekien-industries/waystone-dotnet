namespace Waystone.Monads.Specs.Options.Steps;

using Reqnroll;
using Shouldly;
using System.Threading.Tasks;
using System;
using Waystone.Monads.Exceptions;
using Waystone.Monads.Extensions;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Options;

[Binding]
public sealed class OptionSteps(SpecContext context)
{
    [Given("Option is None")]
    public void GivenOptionIsNone()
    {
        Option<int> option = Option.None<int>();

        context.SetSubject(option);
    }

    [Given("Option is Some with value {int}")]
    public void GivenOptionIsSomeWithValueInt(int optionValue)
    {
        Option<int> option = Option.Some(optionValue);

        context.SetSubject(option);
    }

    [Given("Option is wrapped in a Task")]
    public void GivenOptionIsWrappedInATask()
    {
        var option = context.Subject<Option<int>>();
        Task<Option<int>> taskOption = Task.FromResult(option);
        context.SetSubject(taskOption);
    }

    [Given("Option is wrapped in a ValueTask")]
    public void GivenOptionIsWrappedInAValueTask()
    {
        var option = context.Subject<Option<int>>();
        var taskOption = new ValueTask<Option<int>>(option);
        context.SetSubject(taskOption);
    }

    [Then("the result Option should be Some with value {int}")]
    public void ThenTheResultOptionShouldBeSomeWithValueInt(int value)
    {
        var result = context.Outcome<Option<int>>();
        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe(value);
    }

    [Then("the boolean result should be {string}")]
    public void ThenTheBooleanResultShouldBe(bool expected)
    {
        var result = context.Outcome<bool>();
        result.ShouldBe(expected);
    }

    [Then("the result Option should be None")]
    public void ThenTheResultOptionShouldBeNone()
    {
        var result = context.Outcome<Option<int>>();
        result.IsNone.ShouldBeTrue();
    }

    [Then("the result Option should be Some with value {string}")]
    public void ThenTheResultOptionShouldBeSomeWithValueString(string value)
    {
        var result = context.Outcome<Option<string>>();
        result.IsSome.ShouldBeTrue();
        result.Unwrap().ShouldBe(value);
    }

    [Then("the result Option should be None of {string}")]
    public void ThenTheResultOptionShouldBeNoneOfString(string type)
    {
        switch (type)
        {
            case "string":
            {
                var result = context.Outcome<Option<string>>();
                result.IsNone.ShouldBeTrue();

                break;
            }
            case "int":
            {
                var result = context.Outcome<Option<int>>();
                result.IsNone.ShouldBeTrue();

                break;
            }
            default:
                throw new NotImplementedException(
                    "Type not implemented: " + type);
        }
    }

    [Given("the Option is wrapped in an Option")]
    public void GivenTheOptionIsWrappedInAnOption()
    {
        var option = context.Subject<Option<int>>();
        Option<Option<int>> nestedOption = Option.Some(option);
        context.SetSubject(nestedOption);
    }

    [Given("the Option of Option is wrapped in a Task")]
    public void GivenTheOptionOfOptionIsWrappedInATask()
    {
        var nestedOption = context.Subject<Option<Option<int>>>();

        Task<Option<Option<int>>> taskNestedOption =
            Task.FromResult(nestedOption);

        context.SetSubject(taskNestedOption);
    }

    [Given("the Option of Option is wrapped in a ValueTask")]
    public void GivenTheOptionOfOptionIsWrappedInAValueTask()
    {
        var nestedOption = context.Subject<Option<Option<int>>>();

        var taskNestedOption =
            new ValueTask<Option<Option<int>>>(nestedOption);

        context.SetSubject(taskNestedOption);
    }
}
