namespace Waystone.Monads;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Options;
using Results;
using Shouldly;
using Xunit;

/// <summary>
/// Pins the shape <c>WA0001</c> recognises a null guard by.
/// </summary>
/// <remarks>
/// The rule finds guards by convention rather than by an attribute, so nothing in
/// the build connects <see cref="Option" /> to it. Rename <c>NotNull</c>, drop the
/// <see cref="string" /> parameter, or return something other than what was passed
/// in, and the rule stops recognising the method — then stops reporting the
/// unguarded factories it was added to catch, silently and while still passing
/// every other test in this project.
/// <para>
/// So this asserts the convention rather than the behaviour. It is the only thing
/// standing between a refactor and an analyzer that reports nothing at all.
/// </para>
/// </remarks>
public sealed class GuardConventionTests
{
    public static TheoryData<Type, string> Guards =>
        new()
        {
            { typeof(Option), "NotNull" },
            { typeof(Option), "NotNullAsync" },
            { typeof(Result), "NotNull" },
            { typeof(Result), "NotNullAsync" },
        };

    [Theory]
    [MemberData(nameof(Guards))]
    public void AGuardIsStaticAndTakesTheValueAndItsSource(
        Type factory,
        string name)
    {
        MethodInfo guard = Find(factory, name);

        guard.IsStatic.ShouldBeTrue();

        ParameterInfo[] parameters = guard.GetParameters();

        parameters.Length.ShouldBe(2);
        parameters[1].ParameterType.ShouldBe(typeof(string));
    }

    /// <remarks>
    /// The identity shape is what tells the rule which type a guard guards, and it
    /// is read off the return rather than declared, so the two have to agree.
    /// <c>Task</c> and <c>ValueTask</c> are unwrapped first, which is what lets the
    /// asynchronous guard register against the same type as the synchronous one.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Guards))]
    public void AGuardReturnsWhatItWasGiven(Type factory, string name)
    {
        MethodInfo guard = Find(factory, name);

        Unwrap(guard.ReturnType)
           .ShouldBe(Unwrap(guard.GetParameters()[0].ParameterType));
    }

    /// <remarks>
    /// A third guard, or one on a type the rule has never seen, changes which
    /// delegates the build rejects. That is a decision rather than an edit, so it
    /// fails here until someone records it.
    /// </remarks>
    [Fact]
    public void TheGuardedTypesAreOptionAndResultAndNothingElse()
    {
        IEnumerable<Type> guarded =
            new[] { typeof(Option), typeof(Result) }
               .SelectMany(
                    factory => factory.GetMethods(
                        BindingFlags.Static
                      | BindingFlags.Public
                      | BindingFlags.NonPublic))
               .Where(method => method.Name is "NotNull" or "NotNullAsync")
               .Select(method => Unwrap(method.ReturnType))
               .Select(
                    type => type.IsGenericType
                        ? type.GetGenericTypeDefinition()
                        : type)
               .Distinct();

        guarded.ShouldBe(
            [typeof(Option<>), typeof(Result<,>)],
            ignoreOrder: true);
    }

    private static MethodInfo Find(Type factory, string name) =>
        factory.GetMethod(
                    name,
                    BindingFlags.Static
                  | BindingFlags.Public
                  | BindingFlags.NonPublic)
               .ShouldNotBeNull(
                    $"'{factory.Name}.{name}' is the shape WA0001 looks for. "
                  + "Renaming it turns the rule off rather than breaking the "
                  + "build.");

    private static Type Unwrap(Type type) =>
        type.IsGenericType
     && (type.GetGenericTypeDefinition() == typeof(Task<>)
      || type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            ? type.GetGenericArguments()[0]
            : type;
}
