namespace Waystone.Monads.Schemas;

using System;
using System.Linq;
using System.Reflection;
using Shouldly;
using Waystone.Monads.Schemas.Internal.Fields;
using Xunit;

/// <summary>
/// <c>Field.OnlyThisAssemblyMayDerive</c>, which is what closes the hierarchy: it is
/// abstract and internal, so a field declared outside this assembly cannot override
/// it and therefore cannot compile.
/// </summary>
/// <remarks>
/// The member has no behaviour and is never called in anger, so these assert the two
/// things that are actually true of it — every field in the assembly overrides it,
/// and calling one does nothing. A field that stopped overriding it would not
/// compile, but a field type added without one of the other overrides would, and the
/// first test enumerates rather than lists so a new one is included by existing.
/// </remarks>
public sealed class ClosedFieldHierarchyTests
{
    public static TheoryData<Type> EveryField()
    {
        TheoryData<Type> data = new();

        foreach (Type type in typeof(Field).Assembly.GetTypes()
                                           .Where(
                                                candidate =>
                                                    !candidate.IsAbstract
                                                 && typeof(Field)
                                                       .IsAssignableFrom(
                                                            candidate)))
        {
            data.Add(type);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(EveryField))]
    public void EveryFieldClosesTheHierarchy(Type type) =>
        type.GetMethod(
                nameof(Field.OnlyThisAssemblyMayDerive),
                BindingFlags.Instance
              | BindingFlags.NonPublic
              | BindingFlags.DeclaredOnly)
            .ShouldNotBeNull();

    [Fact]
    public void TheHierarchyHasFieldsToClose() =>
        EveryField().Count.ShouldBe(5);

    /// <summary>
    /// The seal carries no behaviour, so a field is unchanged by it having been
    /// called. Asserted through a real parse rather than by calling it and asserting
    /// nothing, which would say only that it did not throw.
    /// </summary>
    [Fact]
    public void CallingTheSealChangesNothing()
    {
        Field<string> field =
            Schema.Required("abc", new PassThrough<string>());

        Outcome<string> before = field.EvaluateValue(ParseContext.Root);

        field.OnlyThisAssemblyMayDerive();

        Outcome<string> after = field.EvaluateValue(ParseContext.Root);

        after.Value.ShouldBe(before.Value);
        after.Violations.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(EveryField))]
    public void EverySealIsReachable(Type type)
    {
        Field field = Instance(type);

        Should.NotThrow(() => field.OnlyThisAssemblyMayDerive());
    }

    /// <remarks>
    /// Matched on the name before the backtick, because every one of these is
    /// generic and <c>Type.Name</c> carries the arity.
    /// </remarks>
    private static Field Instance(Type type) =>
        type.Name.Split('`')[0] switch
        {
            nameof(RequiredField<string, string>) => Schema.Required(
                "abc",
                new PassThrough<string>()),
            nameof(OptionalField<string, string>) => Schema.Optional(
                "abc",
                new PassThrough<string>()),
            nameof(ForbiddenField<string>) => Schema.Forbidden(
                (string?)null,
                "Do not send {Path}."),
            nameof(ExtendField<string>) => Schema.Extend(
                "abc",
                new PassThrough<string>()),
            nameof(CheckedField<string>) => Schema
                                           .Required(
                                                "abc",
                                                new PassThrough<string>())
                                           .AsChecked(),
            _ => throw new InvalidOperationException(
                     $"'{type.Name}' is a field with no instance here. Add one, so the seal it declares is covered like the rest."),
        };
}
