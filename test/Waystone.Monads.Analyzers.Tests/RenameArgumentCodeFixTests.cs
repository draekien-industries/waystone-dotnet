namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

public class RenameArgumentCodeFixTests
{
    [Fact]
    public Task RenamesOnACoreMemberInReducedForm() =>
        Fixed(
            """
            internal int Unwrap(Option<int> option) =>
                option.UnwrapOrElse({|#0:@else|}: () => 0);
            """,
            """
            internal int Unwrap(Option<int> option) =>
                option.UnwrapOrElse(valueFactory: () => 0);
            """);

    /// <remarks>
    /// The renamed argument sits beside one whose name did not change, which is
    /// the shape most of the inventory takes — a rename converged one parameter
    /// of a member and left its sibling alone.
    /// </remarks>
    [Fact]
    public Task RenamesBesideAnArgumentThatKeptItsName() =>
        Fixed(
            """
            internal int Map(Option<int> option) =>
                option.MapOr({|#0:@default|}: 0, map: value => value);
            """,
            """
            internal int Map(Option<int> option) =>
                option.MapOr(defaultValue: 0, map: value => value);
            """);

    /// <remarks>
    /// Result names its step factory for the type it produces, so the same role
    /// takes a different name on each monad and the fix cannot key on the old
    /// name alone.
    /// </remarks>
    [Fact]
    public Task RenamesOnResult() =>
        Fixed(
            """
            internal Result<int, Error> Chain(Result<int, Error> result) =>
                result.AndThen({|#0:createOther|}: value => Result.Ok<int>(value + 1));
            """,
            """
            internal Result<int, Error> Chain(Result<int, Error> result) =>
                result.AndThen(resultFactory: value => Result.Ok<int>(value + 1));
            """);

    /// <remarks>
    /// A generated awaited receiver takes its parameter names from the core
    /// member it forwards to, so the rename reaches call sites in a family
    /// nobody edited.
    /// </remarks>
    [Fact]
    public Task RenamesOnAGeneratedAwaitedReceiver() =>
        Fixed(
            """
            internal ValueTask<int> Unwrap(Task<Result<int, Error>> resultTask) =>
                resultTask.UnwrapOrAsync({|#0:@default|}: 0);
            """,
            """
            internal ValueTask<int> Unwrap(Task<Result<int, Error>> resultTask) =>
                resultTask.UnwrapOrAsync(defaultValue: 0);
            """);

    /// <remarks>
    /// Two arguments rather than one on an awaited receiver, which is what pins the
    /// arity the receiver is recognised by: the same call in reduced form has one
    /// fewer argument than the compatibility method has parameters, whatever the
    /// argument count is.
    /// </remarks>
    [Fact]
    public Task RenamesTheFirstOfTwoArgumentsOnAnAwaitedReceiver() =>
        Fixed(
            """
            internal ValueTask<int> Map(Task<Option<int>> optionTask) =>
                optionTask.MapOrElseAsync({|#0:defaultFunc|}: () => 0, map: value => value);
            """,
            """
            internal ValueTask<int> Map(Task<Option<int>> optionTask) =>
                optionTask.MapOrElseAsync(defaultFactory: () => 0, map: value => value);
            """);

    /// <remarks>
    /// The compatibility static form carries the receiver in the argument list, so
    /// the parameter a given argument names sits one later than it does in reduced
    /// form. A fix that read the position without accounting for that would offer
    /// the receiver's name here.
    /// </remarks>
    [Fact]
    public Task RenamesInStaticForm() =>
        Fixed(
            """
            internal ValueTask<int> Unwrap(Task<Result<int, Error>> resultTask) =>
                ResultExtensions.UnwrapOrAsync(resultTask, {|#0:@default|}: 0);
            """,
            """
            internal ValueTask<int> Unwrap(Task<Result<int, Error>> resultTask) =>
                ResultExtensions.UnwrapOrAsync(resultTask, defaultValue: 0);
            """);

    /// <remarks>
    /// CS1739 is an ordinary consequence of a typo, so the fix has to stay quiet
    /// on every method that is not this library's.
    /// </remarks>
    [Fact]
    public Task LeavesAnUnrelatedMethodAlone() =>
        Unfixed(
            """
            internal int Call() => Keep({|#0:valeu|}: 1);

            private static int Keep(int value) => value;
            """);

    /// <remarks>
    /// Arguments named out of declaration order would take the fix to a name
    /// another argument already uses, trading CS1739 for CS1740. The intended
    /// name is not recoverable here, so the fix declines rather than guessing.
    /// </remarks>
    [Fact]
    public Task LeavesAnArgumentAloneWhenItsPositionIsSpokenFor() =>
        Unfixed(
            """
            internal int Map(Option<int> option) =>
                option.MapOr(map: value => value, {|#0:@default|}: 0);
            """);

    private static DiagnosticResult NoSuchParameter =>
        DiagnosticResult.CompilerError("CS1739").WithLocation(0);

    private static Task Fixed(string source, string fixedSource) =>
        Verify.CompilerCodeFixAsync<RenameArgumentCodeFix>(
            source,
            fixedSource,
            new[] { NoSuchParameter },
            Array.Empty<DiagnosticResult>());

    private static Task Unfixed(string source) =>
        Verify.DeclinedCompilerCodeFixAsync<RenameArgumentCodeFix>(
            source,
            NoSuchParameter);
}
