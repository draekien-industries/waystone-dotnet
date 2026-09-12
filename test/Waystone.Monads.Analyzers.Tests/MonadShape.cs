namespace Waystone.Monads.Analyzers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

/// <remarks>
/// Renders a member as a string that identifies its <em>shape</em> — the
/// signature with every name stripped out, so two overloads collapse to the same
/// string exactly when they take the same types in the same order.
/// <para>
/// Shared by the tests asserting the DRA-211 invariants, which all work by
/// building the set of shapes a rule demands and subtracting the set that
/// exists. They compare rendered strings rather than <see cref="Type" />
/// instances because the shapes being demanded do not exist as types: an
/// expected-but-missing overload has no <see cref="MethodInfo" /> to compare
/// against, and the whole point is to name the ones that are absent.
/// </para>
/// <para>
/// <see cref="ExpectedAsyncShapes" /> is the exception and does not render an
/// existing member at all — it states what the invariant <em>demands</em>, so the
/// <c>2^N - 1</c> of rule 1 is its loop bound and the extra all-synchronous shape
/// of rule 4 is where that loop starts. Every rule is asserted by subtracting one
/// set from the other, which means an error there under-generates the expectation
/// and every grid test passes for the wrong reason. It is pinned directly by
/// <c>MonadShapeTests</c> for that reason, rather than being trusted because its
/// callers are green.
/// </para>
/// </remarks>
internal static class MonadShape
{
    internal const string StateTypeParameterName = "TState";

    private const string Awaitable = "Awaitable";

    internal static IEnumerable<MethodInfo> Declared(Type type) =>
        type.GetMethods().Where(method => method.DeclaringType == type);

    internal static bool IsAsync(MethodInfo method) =>
        method.Name.EndsWith("Async", StringComparison.Ordinal);

    internal static bool TakesADelegate(MethodInfo method) =>
        method.GetParameters()
              .Any(parameter => IsDelegate(parameter.ParameterType));

    internal static bool DeclaresState(MethodInfo method) =>
        method.GetGenericArguments()
              .Any(argument => argument.Name == StateTypeParameterName);

    private static bool IsDelegate(Type type) =>
        type.IsGenericParameter is false
     && (type.Name.StartsWith("Func`", StringComparison.Ordinal)
      || type.Name.StartsWith("Action", StringComparison.Ordinal));

    internal static string Shape(MethodInfo method) =>
        Shape(
            method.Name,
            method.GetParameters().Select(parameter => Render(parameter.ParameterType)),
            Render(method.ReturnType));

    internal static string Shape(
        string name,
        IEnumerable<string> parameters,
        string returns) =>
        $"{name}({string.Join(", ", parameters)}) -> {returns}";

    internal static string ShapeWithoutState(MethodInfo method) =>
        Shape(
            method.Name,
            method.GetParameters()
                  .Select(parameter => RenderWithoutState(parameter.ParameterType)),
            Render(method.ReturnType));

    /// <summary>
    /// Derives the asynchronous shapes a synchronous member implies, one for every
    /// non-empty subset of its delegates.
    /// </summary>
    /// <param name="method">The synchronous member to derive from.</param>
    /// <param name="includeAllSync">
    /// If true, also derives the shape where no delegate is asynchronous. That
    /// shape is meaningful only on an awaited receiver, which supplies the
    /// asynchrony itself; on the core it would just be the synchronous member.
    /// Default: false.
    /// </param>
    internal static IEnumerable<string> ExpectedAsyncShapes(
        MethodInfo method,
        bool includeAllSync = false)
    {
        ParameterInfo[] parameters = method.GetParameters();

        int[] delegates = Enumerable
                         .Range(0, parameters.Length)
                         .Where(index => IsDelegate(parameters[index].ParameterType))
                         .ToArray();

        for (int subset = includeAllSync ? 0 : 1;
             subset < 1 << delegates.Length;
             subset++)
        {
            var awaited = new HashSet<int>(
                delegates.Where((_, bit) => (subset & (1 << bit)) != 0));

            yield return Shape(
                method.Name + "Async",
                parameters.Select(
                    (parameter, index) => awaited.Contains(index)
                        ? AsyncDelegate(parameter.ParameterType)
                        : Render(parameter.ParameterType)),
                AsyncReturn(method.ReturnType));
        }
    }

    /// <remarks>
    /// Renders a delegate's own return type as <c>Awaitable</c> whether it is
    /// spelled <c>Task</c> or <c>ValueTask</c>, because the rules are about whether
    /// a branch is asynchronous and not about which awaitable carries it. The
    /// library uses both deliberately: <c>AndThenAsync</c> and <c>OrElseAsync</c>
    /// take <c>Func&lt;T, ValueTask&lt;Option&lt;TOut&gt;&gt;&gt;</c> so the
    /// awaiting branch can return the delegate's task straight through and build no
    /// state machine, while the rest take <c>Task</c>.
    /// <para>
    /// Only the delegate's return position is normalised. The member's own return
    /// type is not, so a member returning <c>Task&lt;T&gt;</c> where every sibling
    /// returns <c>ValueTask&lt;T&gt;</c> is still reported.
    /// </para>
    /// </remarks>
    internal static string AwaitableAgnosticShape(MethodInfo method, int skip = 0) =>
        Shape(
            method.Name,
            method.GetParameters()
                  .Skip(skip)
                  .Select(parameter => AwaitableAgnostic(parameter.ParameterType)),
            Render(method.ReturnType));

    internal static string Render(Type type)
    {
        if (type.IsGenericParameter || !type.IsGenericType)
        {
            return type.Name;
        }

        return $"{Unqualified(type)}<{string.Join(", ", type.GetGenericArguments().Select(Render))}>";
    }

    /// <remarks>
    /// Only the delegate's own type arguments lose <c>TState</c>. Anything nested
    /// inside them renders unchanged, so a delegate returning a
    /// <c>Task&lt;TState&gt;</c> — which no member declares today — would not be
    /// quietly rewritten into one returning a <c>Task</c>.
    /// </remarks>
    private static string RenderWithoutState(Type type)
    {
        if (!IsDelegate(type))
        {
            return Render(type);
        }

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        Type[] kept = type.GetGenericArguments()
                          .Where(argument => argument.Name != StateTypeParameterName)
                          .ToArray();

        return kept.Length == 0
            ? Unqualified(type)
            : $"{Unqualified(type)}<{string.Join(", ", kept.Select(Render))}>";
    }

    /// <remarks>
    /// Spelled with <see cref="string.Substring(int,int)" /> rather than a range
    /// indexer, which needs <c>System.Index</c> — a type net472 and net481 do not
    /// have, and which PolySharp supplies to the shipped projects but not to the
    /// test ones.
    /// </remarks>
    private static string Unqualified(Type type) =>
        type.Name.Substring(0, type.Name.IndexOf('`'));

    /// <remarks>
    /// <c>Action</c> and <c>Func</c> are separate delegate families, so making a
    /// branch asynchronous changes which family it belongs to: an
    /// <c>Action&lt;T&gt;</c> becomes a <c>Func&lt;T, Awaitable&gt;</c>, not an
    /// <c>Action</c> of anything. That is the reason the grid doubles for the
    /// void-returning members rather than reusing the value-returning ones.
    /// </remarks>
    private static string AsyncDelegate(Type type)
    {
        string[] arguments = type.IsGenericType
            ? type.GetGenericArguments().Select(Render).ToArray()
            : Array.Empty<string>();

        if (type.Name.StartsWith("Action", StringComparison.Ordinal))
        {
            return $"Func<{string.Join(", ", arguments.Concat(new[] { Awaitable }))}>";
        }

        IEnumerable<string> inputs = arguments.Take(arguments.Length - 1);
        string result = arguments[arguments.Length - 1];

        return $"Func<{string.Join(", ", inputs.Concat(new[] { $"{Awaitable}<{result}>" }))}>";
    }

    private static string AsyncReturn(Type type) =>
        type == typeof(void) ? "ValueTask" : $"ValueTask<{Render(type)}>";

    private static string AwaitableAgnostic(Type type)
    {
        if (!IsDelegate(type) || !type.IsGenericType)
        {
            return Render(type);
        }

        Type[] arguments = type.GetGenericArguments();

        if (AwaitableName(arguments[arguments.Length - 1]) is not { } returns)
        {
            return Render(type);
        }

        IEnumerable<string> inputs = arguments.Take(arguments.Length - 1).Select(Render);

        return $"Func<{string.Join(", ", inputs.Concat(new[] { returns }))}>";
    }

    private static string? AwaitableName(Type type)
    {
        if (type == typeof(Task) || type == typeof(ValueTask))
        {
            return Awaitable;
        }

        if (!type.IsGenericType)
        {
            return null;
        }

        Type definition = type.GetGenericTypeDefinition();

        return definition == typeof(Task<>) || definition == typeof(ValueTask<>)
            ? $"{Awaitable}<{Render(type.GetGenericArguments()[0])}>"
            : null;
    }
}
