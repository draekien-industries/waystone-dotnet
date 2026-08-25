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
                  .AndThenAsync(value => Task.FromResult(Option.Some(value + 1)))
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
                           value => Task.FromResult(Option.Some(value + 1)))
                      .FilterAsync(value => Task.FromResult(value > 0));

        (await chain).ShouldBe(Option.Some(3));
    }

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
