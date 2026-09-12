namespace Waystone.Monads.Analyzers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Options;
using Results;
using Shouldly;
using Xunit;

/// <remarks>
/// Asserts rules 1 and 2 of the DRA-211 shape invariant on the core monads.
/// <see cref="BinderShapeCompletenessTests" /> asserts rule 3 on the binders, and
/// <see cref="AwaitedShapeGridTests" /> rule 4 on the awaited receivers.
/// <para>
/// <strong>Rule 1.</strong> A member taking <c>N</c> delegates has one synchronous
/// shape and <c>2^N - 1</c> asynchronous ones — every non-empty subset of its
/// delegates being the asynchronous one. For <c>N = 1</c> that is the ordinary
/// <c>Map</c>/<c>MapAsync</c> pair. For <c>N = 2</c> it is four shapes, and the two
/// mixed ones are what the library kept losing: <c>Option&lt;T&gt;</c> had them for
/// value-returning <c>MatchAsync</c> and not for the void-returning form,
/// <c>Result&lt;TOk,TErr&gt;</c> the other way round.
/// </para>
/// <para>
/// <strong>Rule 2.</strong> Every delegate-taking <em>synchronous</em> member has a
/// <c>TState</c> overload, and no asynchronous member has one. The second half is
/// not an oversight being tolerated — it is the reason
/// <see cref="Option{T}.Bound{TState}" /> exists, and adding async state overloads
/// to the monads would cost three declarations each for surface that <c>With</c>
/// already provides.
/// </para>
/// <para>
/// Both are stated as set subtraction so a failure names the missing shapes rather
/// than reporting a count that does not match. A test asserting counts passes for
/// the wrong reason as soon as two gaps open at once.
/// </para>
/// </remarks>
public sealed class CoreShapeGridTests
{
    [Fact]
    public void EveryOptionDelegateSubsetHasAnAsyncShape() =>
        AsyncShapesMissingFrom(typeof(Option<>)).ShouldBeEmpty();

    [Fact]
    public void EveryResultDelegateSubsetHasAnAsyncShape() =>
        AsyncShapesMissingFrom(typeof(Result<,>)).ShouldBeEmpty();

    [Fact]
    public void EverySyncOptionMemberHasAStateOverload() =>
        StateOverloadsMissingFrom(typeof(Option<>)).ShouldBeEmpty();

    [Fact]
    public void EverySyncResultMemberHasAStateOverload() =>
        StateOverloadsMissingFrom(typeof(Result<,>)).ShouldBeEmpty();

    /// <remarks>
    /// The other half of rule 2, and the one a well-meaning change is most likely
    /// to break: closing the binder's async gap by adding state overloads to the
    /// monad reads like symmetry and is the thing
    /// <see cref="Option{T}.Bound{TState}" /> was built to avoid.
    /// </remarks>
    [Theory]
    [InlineData(typeof(Option<>))]
    [InlineData(typeof(Result<,>))]
    public void NoAsyncMemberTakesState(Type monad) =>
        MonadShape.Declared(monad)
                  .Where(MonadShape.IsAsync)
                  .Where(MonadShape.DeclaresState)
                  .Select(MonadShape.Shape)
                  .ShouldBeEmpty();

    /// <remarks>
    /// Guards the guard, the way
    /// <c>BinderShapeCompletenessTests.TheRenderingSeparatesShapesThatDifferOnlyInAsynchrony</c>
    /// does for rule 3. Normalising <c>Task</c> and <c>ValueTask</c> to one token is
    /// what lets rule 1 accept either spelling, and a normalisation that went one
    /// step further — collapsing the delegates themselves — would make
    /// <see cref="EveryOptionDelegateSubsetHasAnAsyncShape" /> pass against a monad
    /// carrying a single <c>MatchAsync</c>.
    /// </remarks>
    [Fact]
    public void NormalisingTheAwaitableKeepsTheMixedShapesApart()
    {
        MethodInfo[] matches = MonadShape
                              .Declared(typeof(Option<>))
                              .Where(method => method.Name == nameof(Option<int>.MatchAsync))
                              .ToArray();

        matches.Select(method => MonadShape.AwaitableAgnosticShape(method))
               .Distinct(StringComparer.Ordinal)
               .Count()
               .ShouldBe(matches.Length);
    }

    private static IReadOnlyList<string> AsyncShapesMissingFrom(Type monad)
    {
        MethodInfo[] declared = MonadShape.Declared(monad).ToArray();

        var present = new HashSet<string>(
            declared.Where(MonadShape.IsAsync)
                    .Select(method => MonadShape.AwaitableAgnosticShape(method)),
            StringComparer.Ordinal);

        return declared.Where(method => !MonadShape.IsAsync(method))
                       .Where(MonadShape.TakesADelegate)
                       .Where(method => !MonadShape.DeclaresState(method))
                       .SelectMany(method => MonadShape.ExpectedAsyncShapes(method))
                       .Where(shape => !present.Contains(shape))
                       .Distinct(StringComparer.Ordinal)
                       .OrderBy(shape => shape, StringComparer.Ordinal)
                       .ToList();
    }

    private static IReadOnlyList<string> StateOverloadsMissingFrom(Type monad)
    {
        MethodInfo[] declared = MonadShape.Declared(monad).ToArray();

        ILookup<string, string> stateful = declared
                                          .Where(MonadShape.DeclaresState)
                                          .ToLookup(
                                               method => method.Name,
                                               MonadShape.ShapeWithoutState);

        return declared.Where(method => !MonadShape.IsAsync(method))
                       .Where(MonadShape.TakesADelegate)
                       .Where(method => !MonadShape.DeclaresState(method))
                       .Where(method => !stateful[method.Name]
                                 .Contains(WithLeadingStateAdded(method)))
                       .Select(MonadShape.Shape)
                       .OrderBy(shape => shape, StringComparer.Ordinal)
                       .ToList();
    }

    /// <remarks>
    /// A state overload takes the state first and appends <c>TState</c> to each
    /// delegate, so <see cref="MonadShape.ShapeWithoutState" /> — which strips only
    /// the delegate arguments — leaves the leading operand behind. Rendering the
    /// stateless member with that operand reinstated is what makes the two
    /// comparable.
    /// </remarks>
    private static string WithLeadingStateAdded(MethodInfo method) =>
        MonadShape.Shape(
            method.Name,
            new[] { MonadShape.StateTypeParameterName }.Concat(
                method.GetParameters()
                      .Select(parameter => MonadShape.Render(parameter.ParameterType))),
            MonadShape.Render(method.ReturnType));
}
