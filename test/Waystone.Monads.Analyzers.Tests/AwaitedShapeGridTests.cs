namespace Waystone.Monads.Analyzers;

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

/// <remarks>
/// Asserts rule 4 of the DRA-211 shape invariant: an awaited receiver carries
/// <c>2^N</c> shapes where the core carries <c>2^N - 1</c>, because the extra one
/// is the all-synchronous delegate set. On <c>Task&lt;Option&lt;T&gt;&gt;</c> that
/// shape is meaningful — the receiver supplies the asynchrony — whereas on
/// <c>Option&lt;T&gt;</c> it would just be the synchronous member.
/// <para>
/// <c>AwaitedReceiverCoverageTests</c> already asserts that every core member is
/// reachable from both receivers, but it matches on member <em>name</em>. That is
/// the same altitude the WM2017 gate sits at, and the same altitude the defect
/// this issue fixes slipped through: a name can be present while three of its four
/// shapes are missing. This test is the shape-level statement.
/// </para>
/// <para>
/// The receiver parameter is skipped rather than reconstructed. An extension
/// declares it as its first parameter and the core member has no equivalent, so
/// dropping it is what makes a core member and its lifted form comparable without
/// building a <c>Task&lt;Option&lt;T&gt;&gt;</c> by hand to render against.
/// </para>
/// </remarks>
public sealed class AwaitedShapeGridTests
{
    public static TheoryData<string, Type, Type, Type> Receivers() =>
        new()
        {
            { "Task<Option<T>>", typeof(Task<>), typeof(Option<>), typeof(OptionExtensions) },
            { "ValueTask<Option<T>>", typeof(ValueTask<>), typeof(Option<>), typeof(OptionExtensions) },
            { "Task<Result<TOk, TErr>>", typeof(Task<>), typeof(Result<,>), typeof(ResultExtensions) },
            { "ValueTask<Result<TOk, TErr>>", typeof(ValueTask<>), typeof(Result<,>), typeof(ResultExtensions) },
        };

    /// <param name="receiver">
    /// Names the receiver in the test's display name, so a failure says which of the
    /// four drifted without the reader decoding three <see cref="Type" /> arguments.
    /// </param>
    [Theory]
    [MemberData(nameof(Receivers))]
    public void EveryCoreDelegateSubsetIsReachableFromTheAwaitedReceiver(
        string receiver,
        Type awaitable,
        Type monad,
        Type extensions)
    {
        _ = receiver;

        var present = new HashSet<string>(
            LiftedOnto(awaitable, monad, extensions)
               .Select(method => MonadShape.AwaitableAgnosticShape(method, skip: 1)),
            StringComparer.Ordinal);

        MonadShape.Declared(monad)
                  .Where(method => !MonadShape.IsAsync(method))
                  .Where(MonadShape.TakesADelegate)
                  .Where(method => !MonadShape.DeclaresState(method))
                  .SelectMany(
                       method => MonadShape.ExpectedAsyncShapes(
                           method,
                           includeAllSync: true))
                  .Where(shape => !present.Contains(shape))
                  .Distinct(StringComparer.Ordinal)
                  .OrderBy(shape => shape, StringComparer.Ordinal)
                  .ShouldBeEmpty();
    }

    /// <remarks>
    /// Guards the guard. <see cref="LiftedOnto" /> matching nothing would make the
    /// test above assert that an empty expectation is absent from an empty surface,
    /// which passes — so a receiver predicate that silently stopped matching would
    /// take rule 4 with it and report nothing.
    /// <para>
    /// Only non-emptiness is pinned, not a count, which would need editing on every
    /// addition. That the two receivers carry the <em>same</em> members is asserted
    /// by <c>AwaitedReceiverCoverageTests</c>, and by name rather than by shape on
    /// purpose: <c>ZipWithAsync</c> awaits a second operand as well as the receiver,
    /// so it is <c>Task&lt;Option&lt;TOther&gt;&gt;</c> on one and
    /// <c>ValueTask&lt;Option&lt;TOther&gt;&gt;</c> on the other. Shape-level
    /// equality across the two receivers is false by design.
    /// </para>
    /// </remarks>
    [Theory]
    [MemberData(nameof(Receivers))]
    public void TheReceiverPredicateMatchesSomething(
        string receiver,
        Type awaitable,
        Type monad,
        Type extensions)
    {
        _ = receiver;

        LiftedOnto(awaitable, monad, extensions).ShouldNotBeEmpty();
    }

    private static IEnumerable<MethodInfo> LiftedOnto(
        Type awaitable,
        Type monad,
        Type extensions) =>
        MonadShape.Declared(extensions)
                  .Where(method => method.IsStatic)
                  .Where(method => method.GetParameters().Length > 0)
                  .Where(
                       method => Receives(
                           method.GetParameters()[0].ParameterType,
                           awaitable,
                           monad));

    /// <remarks>
    /// Near-identical to <c>AwaitedReceiverCoverageTests.Receives</c> in
    /// <c>Waystone.Monads.Tests</c>, and deliberately not shared. That one is
    /// <c>private</c> in an assembly this project does not reference —
    /// <c>InternalsVisibleTo</c> runs from a shipped assembly to its own tests,
    /// never test to test — so sharing it means a linked file or a third project
    /// for eight lines. It also returns member <em>names</em>, where the grid needs
    /// the <see cref="MethodInfo" /> to render a shape from.
    /// </remarks>
    private static bool Receives(Type parameter, Type awaitable, Type monad)
    {
        if (!parameter.IsGenericType
         || parameter.GetGenericTypeDefinition() != awaitable)
        {
            return false;
        }

        Type argument = parameter.GetGenericArguments()[0];

        return argument.IsGenericType
            && argument.GetGenericTypeDefinition() == monad;
    }
}
