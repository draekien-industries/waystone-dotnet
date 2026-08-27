namespace Waystone.Monads;

using Options;
using Results;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// <para>
/// Nothing else relates the core surface to the awaited one, and seven members
/// reached 7.0.0 with no awaited shape at all because of it: <c>Option.Or</c>,
/// <c>Option.Xor</c>, <c>Option.Zip</c>, <c>Result.And</c>, <c>Result.Or</c>,
/// <c>Result.GetOk</c> and <c>Result.GetErr</c>. DRA-136 closed them; these tests
/// are what stops the next one opening.
/// </para>
/// <para>
/// They assert on the emitted surface rather than on
/// <c>[GenerateAwaitedMember]</c>, which is the stricter of the two and the reason
/// consolidating the extension classes would not have prevented the gap. Three of
/// the seven were lost because no destination class existed, which one class per
/// monad would have made impossible — but <c>Option.Zip</c> was lost beside a
/// <c>ZipWithExtensions</c> that already existed, purely because nobody added it
/// to a list. Reading the surface catches both causes, and catches a shape that is
/// hand-written rather than generated as well.
/// </para>
/// </remarks>
public sealed class AwaitedReceiverCoverageTests
{
    /// <summary>
    /// Every public member of a monad is reachable from a
    /// <see cref="Task{TResult}" /> or <see cref="ValueTask{TResult}" /> of it, so
    /// a chain never has to break out to <c>(await …)</c> for one operation.
    /// </summary>
    /// <param name="monad">
    /// The unbound monad definition. Passed as a case rather than asserted over
    /// both at once so a failure names which monad drifted — the two are converted
    /// family by family and go asymmetric in between.
    /// </param>
    [Theory]
    [MemberData(nameof(Monads))]
    public void GivenACoreMember_ThenAnAwaitedReceiverShouldForwardToIt(
        Type monad)
    {
        IReadOnlyCollection<string> awaited = AwaitedMemberNames(monad);

        List<string> uncovered = CoreMemberNames(monad)
                                .Where(name => !awaited.Contains(name + "Async"))
                                .OrderBy(name => name, StringComparer.Ordinal)
                                .ToList();

        uncovered.ShouldBeEmpty();
    }

    /// <summary>
    /// Both receiver shapes carry every awaited member, so a caller holding a
    /// <see cref="Task{TResult}" /> and one holding a
    /// <see cref="ValueTask{TResult}" /> reach the same surface.
    /// </summary>
    /// <remarks>
    /// The generator emits the pair together, so this cannot fail on generated
    /// output. It guards a hand-written family, where one receiver is a separate
    /// block that can be forgotten — and the hand-written blocks are the majority
    /// of what is left.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Monads))]
    public void GivenAnAwaitedMember_ThenBothReceiverShapesShouldCarryIt(
        Type monad)
    {
        IReadOnlyCollection<string> onTask =
            AwaitedMemberNames(monad, typeof(Task<>));

        IReadOnlyCollection<string> onValueTask =
            AwaitedMemberNames(monad, typeof(ValueTask<>));

        List<string> asymmetric = onTask.Except(onValueTask)
                                        .Concat(onValueTask.Except(onTask))
                                        .OrderBy(
                                             name => name,
                                             StringComparer.Ordinal)
                                        .ToList();

        asymmetric.ShouldBeEmpty();
    }

    public static TheoryData<Type> Monads() =>
        new TheoryData<Type> { typeof(Option<>), typeof(Result<,>) };

    /// <summary>
    /// Guards the three tests above against passing because they inspected
    /// nothing, which is how a reflection filter fails silently after a rename.
    /// </summary>
    [Theory]
    [MemberData(nameof(Monads))]
    public void GivenAMonad_ThenThereShouldBeMembersToInspect(Type monad)
    {
        CoreMemberNames(monad).Count.ShouldBeGreaterThan(20);
        AwaitedMemberNames(monad).Count.ShouldBeGreaterThan(20);
    }

    /// <summary>
    /// Gets the names of the members an awaited receiver is expected to forward
    /// to: the monad's own public instance methods.
    /// </summary>
    /// <remarks>
    /// <c>IsSpecialName</c> drops the property accessors and the operators, and
    /// the listed names drop what the record declaration synthesises. Declaring
    /// the exclusions rather than pattern-matching them means a new core member
    /// is covered by default and a new exclusion is a deliberate edit.
    /// </remarks>
    private static IReadOnlyCollection<string> CoreMemberNames(Type monad) =>
        monad.GetMethods(BindingFlags.Public | BindingFlags.Instance)
             .Where(method => !method.IsSpecialName)
             .Select(method => method.Name)
             .Where(name => !Synthesised.Contains(name))
             .Distinct(StringComparer.Ordinal)
             .ToList();

    private static readonly HashSet<string> Synthesised = new(
        StringComparer.Ordinal)
    {
        "Equals",
        "GetHashCode",
        "GetType",
        "ToString",
        "Deconstruct",
        "<Clone>$",
    };

    /// <summary>
    /// Gets the names of every public extension member across the assembly whose
    /// receiver is an awaitable of <paramref name="monad" />.
    /// </summary>
    /// <param name="monad">
    /// The unbound monad definition, <c>Option&lt;&gt;</c> or
    /// <c>Result&lt;,&gt;</c>.
    /// </param>
    /// <param name="awaitable">
    /// The unbound receiver wrapper to restrict to, or null for both.
    /// </param>
    private static IReadOnlyCollection<string> AwaitedMemberNames(
        Type monad,
        Type? awaitable = null) =>
        typeof(Option<>).Assembly.GetExportedTypes()
                        .SelectMany(
                             type => type.GetMethods(
                                 BindingFlags.Public | BindingFlags.Static))
                        .Where(method => method.GetParameters().Length > 0)
                        .Where(
                             method => Receives(
                                 method.GetParameters()[0].ParameterType,
                                 monad,
                                 awaitable))
                        .Select(method => method.Name)
                        .Distinct(StringComparer.Ordinal)
                        .ToList();

    private static bool Receives(Type receiver, Type monad, Type? awaitable)
    {
        if (!receiver.IsGenericType) return false;

        Type wrapper = receiver.GetGenericTypeDefinition();

        if (wrapper != typeof(Task<>) && wrapper != typeof(ValueTask<>))
        {
            return false;
        }

        if (awaitable is not null && wrapper != awaitable) return false;

        Type awaited = receiver.GetGenericArguments()[0];

        return awaited.IsGenericType
            && awaited.GetGenericTypeDefinition() == monad;
    }
}
