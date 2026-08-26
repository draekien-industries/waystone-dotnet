namespace Waystone.Monads.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Options;
using Options.Extensions;
using Results;
using Results.Extensions;
using Shouldly;
using Xunit;

public sealed class AsyncReturnTypeTests
{
    private static readonly string[] ExtensionNamespaces =
    [
        "Waystone.Monads.Options.Extensions",
        "Waystone.Monads.Results.Extensions",
    ];

    [Fact]
    public void
        GivenAnAsyncExtensionThatMayCompleteSynchronously_ThenItShouldNotReturnTask()
    {
        List<string> offenders =
            typeof(Option<>).Assembly.GetTypes()
                            .Where(
                                 type => type.Namespace is not null
                                      && ExtensionNamespaces.Contains(
                                             type.Namespace))
                            .SelectMany(
                                 type => type.GetMethods(
                                     BindingFlags.Public
                                   | BindingFlags.Static))
                            .Where(
                                 method => method.Name.EndsWith(
                                     "Async",
                                     StringComparison.Ordinal))
                            .Where(method => !CannotCompleteSynchronously(method))
                            .Where(method => ReturnsTask(method.ReturnType))
                            .Select(
                                 method =>
                                     $"{method.DeclaringType!.Name}.{method.Name}")
                            .Distinct()
                            .OrderBy(name => name, StringComparer.Ordinal)
                            .ToList();

        offenders.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAnAsyncExtension_ThenTheAssemblyShouldDeclareSome()
    {
        int count = typeof(Option<>).Assembly.GetTypes()
                                    .Where(
                                         type => type.Namespace is not null
                                          && ExtensionNamespaces.Contains(
                                                 type.Namespace))
                                    .SelectMany(
                                         type => type.GetMethods(
                                             BindingFlags.Public
                                           | BindingFlags.Static))
                                    .Count(
                                         method => method.Name.EndsWith(
                                             "Async",
                                             StringComparison.Ordinal));

        count.ShouldBeGreaterThan(100);
    }

    [Fact]
    public async Task GivenEachOptionReceiverShape_ThenMapAsyncShouldReturnValueTask()
    {
        Option<int> option = Option.Some(1);
        Task<Option<int>> optionTask = Task.FromResult(option);
        ValueTask<Option<int>> optionValueTask = new(option);

        ValueTask<Option<int>> fromSync =
            option.MapAsync(value => Task.FromResult(value + 1));
        ValueTask<Option<int>> fromTask =
            optionTask.MapAsync(value => Task.FromResult(value + 1));
        ValueTask<Option<int>> fromValueTask =
            optionValueTask.MapAsync(value => Task.FromResult(value + 1));

        (await fromSync).ShouldBe(Option.Some(2));
        (await fromTask).ShouldBe(Option.Some(2));
        (await fromValueTask).ShouldBe(Option.Some(2));
    }

    [Fact]
    public async Task GivenEachResultReceiverShape_ThenMapAsyncShouldReturnValueTask()
    {
        Result<int, string> result = Result.Ok<int, string>(1);
        Task<Result<int, string>> resultTask = Task.FromResult(result);
        ValueTask<Result<int, string>> resultValueTask = new(result);

        ValueTask<Result<int, string>> fromSync =
            result.MapAsync(value => Task.FromResult(value + 1));
        ValueTask<Result<int, string>> fromTask =
            resultTask.MapAsync(value => Task.FromResult(value + 1));
        ValueTask<Result<int, string>> fromValueTask =
            resultValueTask.MapAsync(value => Task.FromResult(value + 1));

        (await fromSync).ShouldBe(Result.Ok<int, string>(2));
        (await fromTask).ShouldBe(Result.Ok<int, string>(2));
        (await fromValueTask).ShouldBe(Result.Ok<int, string>(2));
    }

    [Fact]
    public async Task GivenASyncReceiver_ThenAThreeLinkChainShouldStayValueTask()
    {
        Option<int> option = Option.Some(1);

        ValueTask<Option<int>> chain =
            option.MapAsync(value => Task.FromResult(value + 1))
                  .AndThenAsync(
                       value => new ValueTask<Option<int>>(
                           Option.Some(value + 1)))
                  .FilterAsync(value => Task.FromResult(value > 0));

        (await chain).ShouldBe(Option.Some(3));
    }

    [Fact]
    public async Task GivenATaskReceiverPartway_ThenTheChainShouldStayValueTask()
    {
        Task<Option<int>> optionTask = Task.FromResult(Option.Some(1));

        ValueTask<Option<int>> chain =
            optionTask.MapAsync(value => Task.FromResult(value + 1))
                      .AndThenAsync(
                           value => new ValueTask<Option<int>>(
                               Option.Some(value + 1)))
                      .FilterAsync(value => Task.FromResult(value > 0));

        (await chain).ShouldBe(Option.Some(3));
    }

    /// <summary>
    /// A two-link async chain, whose signature is a step's signature. Up to 6.x
    /// this could not exist as a step: every step parameter took a
    /// <c>Task</c>-returning delegate while every member returned a
    /// <see cref="ValueTask{TResult}" />, so a chain was terminal.
    /// </summary>
    private static ValueTask<Option<int>> DoubledThenIncremented(int value) =>
        Option.Some(value)
              .MapAsync(inner => Task.FromResult(inner * 2))
              .AndThenAsync(
                   inner => new ValueTask<Option<int>>(Option.Some(inner + 1)));

    private static ValueTask<Result<int, string>> Halved(int value) =>
        Result.Ok<int, string>(value)
              .MapAsync(inner => Task.FromResult(inner / 2));

    /// <summary>
    /// The whole point of the delegate change: the chain above is handed over as a
    /// method group, with no lambda and no <c>.AsTask()</c>. If this stops
    /// compiling, an async chain has stopped being a step.
    /// </summary>
    [Fact]
    public async Task GivenAnAsyncChain_ThenItComposesAsAStepByName()
    {
        Option<int> result = await Option.Some(3)
                                        .AndThenAsync(DoubledThenIncremented);

        result.ShouldBe(Option.Some(7));
    }

    [Fact]
    public async Task GivenAnAsyncChainOverAResult_ThenItComposesAsAStepByName()
    {
        Result<int, string> result =
            await Result.Ok<int, string>(8).AndThenAsync(Halved);

        result.ShouldBe(Result.Ok<int, string>(4));
    }

    /// <summary>
    /// A chain reused across two receiver shapes, which is what makes the step a
    /// unit of reuse rather than a one-off.
    /// </summary>
    [Fact]
    public async Task GivenAnAwaitedReceiver_ThenAnAsyncChainStillComposesAsAStep()
    {
        Option<int> result = await Task.FromResult(Option.Some(3))
                                      .AndThenAsync(DoubledThenIncremented);

        result.ShouldBe(Option.Some(7));
    }

    [Fact]
    public async Task GivenAnAsyncChain_ThenItComposesAsAnOrElseStepByName()
    {
        Result<int, string> result = await Result.Err<int, string>("failed")
                                               .OrElseAsync(Recovered);

        result.ShouldBe(Result.Ok<int, string>(1));
    }

    private static ValueTask<Result<int, string>> Recovered(string error) =>
        Result.Ok<int, string>(error.Length)
              .MapAsync(inner => Task.FromResult(inner / 6));

    private static bool CannotCompleteSynchronously(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();

        return parameters.Length > 0
            && parameters[0].ParameterType.IsGenericType
            && parameters[0].ParameterType.GetGenericTypeDefinition()
            == typeof(IAsyncEnumerable<>);
    }

    private static bool ReturnsTask(Type returnType) =>
        returnType == typeof(Task)
     || (returnType.IsGenericType
      && returnType.GetGenericTypeDefinition() == typeof(Task<>));
}
