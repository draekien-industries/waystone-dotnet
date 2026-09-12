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
/// <see cref="StateOverloadAnalyzer" /> decides whether to report WM2017 by
/// asking whether the binder declares a member of the same <em>name</em> —
/// <c>TheBinderDeclares</c> calls <c>binder.GetMembers(method.Name)</c> and looks
/// no further. So the diagnostic fires for every overload of that name, and the
/// code fix rewrites the call onto the binder without changing the name. If the
/// binder has no overload of the matching shape, the fix emits source that does
/// not compile.
/// <para>
/// That is not hypothetical. Before DRA-211 the binders carried only the
/// both-asynchronous <c>MatchAsync</c> and <c>MapOrElseAsync</c>, while
/// <see cref="Option{T}" /> and <see cref="Result{TOk,TErr}" /> carried the mixed
/// shapes too, and accepting the fix on one of those produced <c>CS0029</c>.
/// </para>
/// <para>
/// This test is the guard, and it is deliberately stated as the analyzer's
/// precondition rather than as a symmetry rule: for every shape of a name the
/// binder declares, the binder declares that shape with <c>TState</c> appended to
/// each delegate. Stated that way it cannot drift from the analyzer, because it
/// is the same question the analyzer asks. A future overload added to
/// <see cref="Option{T}" /> or <see cref="Result{TOk,TErr}" /> without its binder
/// twin fails here rather than reaching a consumer as a broken lightbulb.
/// </para>
/// <para>
/// Names the binder does not declare at all are outside the rule, because
/// WM2017 never fires for them.
/// </para>
/// </remarks>
public sealed class BinderShapeCompletenessTests
{
    [Fact]
    public void EveryOptionShapeTheBinderIsAskedForExists() =>
        ShapesWithNoBinderTwin(typeof(Option<>), typeof(Option<>.Bound<>))
           .ShouldBeEmpty();

    [Fact]
    public void EveryResultShapeTheBinderIsAskedForExists() =>
        ShapesWithNoBinderTwin(typeof(Result<,>), typeof(Result<,>.Bound<>))
           .ShouldBeEmpty();

    /// <remarks>
    /// Guards the guard. Were the rendering to collapse every shape to the same
    /// string, the two tests above would pass against any binder at all, so the
    /// thing that makes them capable of failing is pinned separately.
    /// </remarks>
    [Fact]
    public void TheRenderingSeparatesShapesThatDifferOnlyInAsynchrony()
    {
        MethodInfo[] mixed = typeof(Option<>)
                            .GetMethods()
                            .Where(method => method.Name == nameof(Option<int>.MatchAsync))
                            .ToArray();

        mixed.Select(MonadShape.Shape)
             .Distinct()
             .Count()
             .ShouldBe(mixed.Length);
    }

    private static IReadOnlyList<string> ShapesWithNoBinderTwin(
        Type monad,
        Type binder)
    {
        ILookup<string, string> twins = binder
                                       .GetMethods()
                                       .Where(method => method.DeclaringType == binder)
                                       .ToLookup(
                                            method => method.Name,
                                            MonadShape.ShapeWithoutState);

        return monad.GetMethods()
                    .Where(method => method.DeclaringType == monad)
                    .Where(MonadShape.TakesADelegate)
                    .Where(method => !MonadShape.DeclaresState(method))
                    .Where(method => twins.Contains(method.Name))
                    .Where(method => !twins[method.Name]
                              .Contains(MonadShape.Shape(method)))
                    .Select(method => $"{monad.Name}.{MonadShape.Shape(method)}")
                    .OrderBy(shape => shape, StringComparer.Ordinal)
                    .ToList();
    }
}
