namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shouldly;
using Xunit;

/// <summary>
/// Guards the one hazard the type system does not: <c>Evaluate</c> is abstract
/// but <c>EvaluateAsync</c> is virtual with a synchronous default, so a node that
/// overrides the first and forgets the second compiles cleanly and then runs an
/// asynchronous inner schema synchronously, with nothing in the build noticing.
/// </summary>
/// <remarks>
/// The default is correct for a <i>leaf</i> — a rule with no inner schema has no
/// asynchronous work to await — so this asserts only against nodes that hold one.
/// Deriving from <c>DecoratorSchema</c> satisfies it, since that type seals both
/// paths. A node added in a later layer that wraps a schema and hand-rolls only
/// the synchronous path fails here.
/// </remarks>
public sealed class CompositeNodeAsyncPathTests
{
    [Fact]
    public void GivenANodeHoldingASchema_ThenItOverridesTheAsynchronousPath()
    {
        List<Type> offenders = CompositeNodes()
                              .Where(node => !OverridesEvaluateAsync(node))
                              .ToList();

        offenders.ShouldBeEmpty(
            "A node holding an inner schema must override EvaluateAsync, or "
          + "derive from DecoratorSchema, or it will run that schema "
          + "synchronously. Offenders: "
          + string.Join(", ", offenders.Select(static type => type.Name)));
    }

    [Fact]
    public void GivenTheAssembly_ThenThereAreCompositeNodesToCheck()
    {
        CompositeNodes().ShouldNotBeEmpty();
    }

    private static IReadOnlyList<Type> CompositeNodes() =>
        typeof(Schema<,>).Assembly.GetTypes()
                         .Where(IsSchema)
                         .Where(HoldsASchema)
                         .ToList();

    private static bool IsSchema(Type type) =>
        !type.IsAbstract && Unwind(type).Any(IsSchemaDefinition);

    private static bool HoldsASchema(Type type) =>
        type.GetFields(BindingFlags.Instance
                     | BindingFlags.NonPublic
                     | BindingFlags.Public)
            .Any(
                 field => Unwind(ElementOf(field.FieldType))
                    .Any(IsSchemaDefinition));

    private static Type ElementOf(Type type) =>
        type.IsArray ? type.GetElementType()! : type;

    private static IEnumerable<Type> Unwind(Type? type)
    {
        for (Type? current = type; current is not null;
             current = current.BaseType)
        {
            yield return current;
        }
    }

    private static bool IsSchemaDefinition(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Schema<,>);

    private static bool OverridesEvaluateAsync(Type type) =>
        Unwind(type)
           .TakeWhile(static candidate => !IsSchemaDefinition(candidate))
           .Any(
                candidate => candidate.GetMethod(
                    "EvaluateAsync",
                    BindingFlags.Instance
                  | BindingFlags.NonPublic
                  | BindingFlags.DeclaredOnly) is not null);
}
