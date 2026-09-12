namespace Waystone.Analyzers;

using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

public class UnguardedFactoryAnalyzerTests
{
    /// <summary>
    /// A guarded type and the pair of guards over it, as every source below
    /// declares them.
    /// </summary>
    private const string Guarded = """
        public sealed class Box<T>
        {
            public T? Value { get; set; }
        }

        public static class Boxes
        {
            public static Box<T> NotNull<T>(Box<T> box, string producedBy) =>
                box ?? throw new ArgumentNullException(producedBy);

            public static ValueTask<Box<T>> NotNullAsync<T>(
                ValueTask<Box<T>> box,
                string producedBy) =>
                box;
        }
        """;

    [Fact]
    public async Task ReportsADelegateReturningAGuardedTypeUnguarded()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            $$"""
              {{Guarded}}

              public static class Subject
              {
                  public static Box<int> Run(Func<Box<int>> factory) =>
                      factory();
              }
              """);

        Diagnostic single = reported.ShouldHaveSingleItem();

        single.Id.ShouldBe("WA0001");
        single.Severity.ShouldBe(DiagnosticSeverity.Error);
        single.GetMessage().ShouldContain("'factory'");
        single.GetMessage().ShouldContain("'Box<int>'");
        single.GetMessage().ShouldContain("'Boxes.NotNull'");
    }

    /// <remarks>
    /// The message has to name the asynchronous guard here, not merely some guard
    /// for the type. Naming the synchronous one would not compile if a reader
    /// followed it, which is the one way a message can be worse than silence.
    /// </remarks>
    [Fact]
    public async Task NamesTheAsynchronousGuardForAnAsynchronousDelegate()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            $$"""
              {{Guarded}}

              public static class Subject
              {
                  public static ValueTask<Box<int>> Run(
                      Func<ValueTask<Box<int>>> factory) =>
                      factory();
              }
              """);

        reported.ShouldHaveSingleItem()
                .GetMessage()
                .ShouldContain("'Boxes.NotNullAsync'");
    }

    [Fact]
    public async Task AcceptsACallRoutedThroughTheGuard()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            $$"""
              {{Guarded}}

              public static class Subject
              {
                  public static Box<int> Run(Func<Box<int>> factory) =>
                      Boxes.NotNull(factory(), nameof(factory));
              }
              """);

        reported.ShouldBeEmpty();
    }

    [Fact]
    public async Task AcceptsAnAsynchronousCallRoutedThroughTheGuard()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            $$"""
              {{Guarded}}

              public static class Subject
              {
                  public static ValueTask<Box<int>> Run(
                      Func<ValueTask<Box<int>>> factory) =>
                      Boxes.NotNullAsync(factory(), nameof(factory));
              }
              """);

        reported.ShouldBeEmpty();
    }

    /// <remarks>
    /// The shape every forwarding overload has. Handing the delegate on rather
    /// than calling it inherits the guard of whatever eventually calls it, and
    /// there is nothing to check at the forward.
    /// </remarks>
    [Fact]
    public async Task AcceptsADelegateForwardedRatherThanInvoked()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            $$"""
              {{Guarded}}

              public static class Subject
              {
                  public static Box<int> Run(Func<Box<int>> factory) =>
                      Guarded(factory);

                  private static Box<int> Guarded(Func<Box<int>> factory) =>
                      Boxes.NotNull(factory(), nameof(factory));
              }
              """);

        reported.ShouldBeEmpty();
    }

    /// <remarks>
    /// The branch that keeps the rule off every ordinary delegate in the
    /// codebase. Only a type something guards is a type the rule has an opinion
    /// about.
    /// </remarks>
    [Fact]
    public async Task AcceptsADelegateReturningAnUnguardedType()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            $$"""
              {{Guarded}}

              public static class Subject
              {
                  public static string Run(Func<string> factory) =>
                      factory();
              }
              """);

        reported.ShouldBeEmpty();
    }

    /// <remarks>
    /// A compilation declaring no guard has no guarded types, so the rule is
    /// inert rather than reporting every delegate it can see. This is what lets
    /// the analyzer be imported by a project that has not opted into anything.
    /// </remarks>
    [Fact]
    public async Task StaysSilentInACompilationWithNoGuards()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            """
            public sealed class Box<T>
            {
                public T? Value { get; set; }
            }

            public static class Subject
            {
                public static Box<int> Run(Func<Box<int>> factory) =>
                    factory();
            }
            """);

        reported.ShouldBeEmpty();
    }

    /// <remarks>
    /// The name alone does not make a guard, or any helper called <c>NotNull</c>
    /// would start conscripting its parameter's type into the rule. This one
    /// returns something other than what it took, so it guards nothing.
    /// </remarks>
    [Fact]
    public async Task IgnoresAMethodNamedLikeAGuardThatReturnsAnotherType()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            """
            public sealed class Box<T>
            {
                public T? Value { get; set; }
            }

            public static class Boxes
            {
                public static string NotNull<T>(Box<T> box, string producedBy) =>
                    producedBy;
            }

            public static class Subject
            {
                public static Box<int> Run(Func<Box<int>> factory) =>
                    factory();
            }
            """);

        reported.ShouldBeEmpty();
    }

    /// <remarks>
    /// The second parameter is what the guard puts in the exception, so a
    /// one-argument overload cannot report which delegate produced the null and
    /// is not the shape being looked for.
    /// </remarks>
    [Fact]
    public async Task IgnoresAGuardShapedMethodThatCannotNameItsSource()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            """
            public sealed class Box<T>
            {
                public T? Value { get; set; }
            }

            public static class Boxes
            {
                public static Box<T> NotNull<T>(Box<T> box) => box;
            }

            public static class Subject
            {
                public static Box<int> Run(Func<Box<int>> factory) =>
                    factory();
            }
            """);

        reported.ShouldBeEmpty();
    }

    /// <remarks>
    /// Awaiting the call is not guarding it. The rule reports the invocation
    /// rather than the return, so moving the value through a local changes
    /// nothing about whether it was checked.
    /// </remarks>
    [Fact]
    public async Task ReportsACallWhoseResultIsAwaitedIntoALocal()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            $$"""
              {{Guarded}}

              public static class Subject
              {
                  public static async ValueTask<Box<int>> Run(
                      Func<ValueTask<Box<int>>> factory)
                  {
                      Box<int> box = await factory();

                      return box;
                  }
              }
              """);

        reported.ShouldHaveSingleItem().Id.ShouldBe("WA0001");
    }

    /// <remarks>
    /// <c>Task</c> is unwrapped as readily as <c>ValueTask</c>, because which of
    /// the two a delegate returns is the caller's choice and says nothing about
    /// whether its result was checked. The guard named is still the asynchronous
    /// one, whose own parameter is a <c>ValueTask</c>.
    /// </remarks>
    [Fact]
    public async Task ReportsADelegateReturningATaskOfAGuardedType()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            $$"""
              {{Guarded}}

              public static class Subject
              {
                  public static Task<Box<int>> Run(Func<Task<Box<int>>> factory) =>
                      factory();
              }
              """);

        reported.ShouldHaveSingleItem()
                .GetMessage()
                .ShouldContain("'Boxes.NotNullAsync'");
    }

    /// <remarks>
    /// A type parameter is not a named type, so it cannot be looked up and cannot
    /// be guarded. This is the branch that keeps the rule off a generic helper
    /// whose delegate might be instantiated at a guarded type by some caller.
    /// </remarks>
    [Fact]
    public async Task AcceptsADelegateReturningAnUnboundTypeParameter()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            $$"""
              {{Guarded}}

              public static class Subject
              {
                  public static T Run<T>(Func<T> factory) => factory();
              }
              """);

        reported.ShouldBeEmpty();
    }

    /// <remarks>
    /// A type guarded on one side only leaves the other silent. Reporting it would
    /// mean naming the synchronous guard at an asynchronous call, which does not
    /// compile — a message a reader cannot act on is worse than none.
    /// </remarks>
    [Fact]
    public async Task AcceptsAnAsyncCallOnATypeWithOnlyASynchronousGuard()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            """
            public sealed class Box<T>
            {
                public T? Value { get; set; }
            }

            public static class Boxes
            {
                public static Box<T> NotNull<T>(Box<T> box, string producedBy) =>
                    box ?? throw new ArgumentNullException(producedBy);
            }

            public static class Subject
            {
                public static ValueTask<Box<int>> Run(
                    Func<ValueTask<Box<int>>> factory) =>
                    factory();
            }
            """);

        reported.ShouldBeEmpty();
    }

    /// <remarks>
    /// A delegate held in a field is as capable of returning null as one passed
    /// in, so the rule reports it and falls back to the syntax for a name, there
    /// being no parameter to point at.
    /// </remarks>
    [Fact]
    public async Task ReportsADelegateReachedThroughAField()
    {
        ImmutableArray<Diagnostic> reported = await VerifyAnalyzer.Run(
            $$"""
              {{Guarded}}

              public static class Subject
              {
                  private static readonly Func<Box<int>> Factory = () => new();

                  public static Box<int> Run() => Factory();
              }
              """);

        reported.ShouldHaveSingleItem().GetMessage().ShouldContain("'Factory'");
    }
}
