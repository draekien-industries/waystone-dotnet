namespace Waystone.SourceGenerators;

using System.Linq;
using Shouldly;
using Xunit;

public sealed class AwaitedReceiversGeneratorTests
{
    private const string Box = """
        public abstract class Box<T> where T : notnull
        {
            /// <summary>Gets the value.</summary>
            /// <exception cref="System.InvalidOperationException">When the box is empty.</exception>
            public abstract T Get();

            /// <summary>Maps the value.</summary>
            /// <param name="map">The map function.</param>
            /// <typeparam name="TOut">The mapped type.</typeparam>
            public abstract Box<TOut> Map<TOut>(Func<T, TOut> map) where TOut : notnull;

            public abstract void Peek(Action<T> peek);

            public abstract T Unlisted();
        }


        """;

    [Fact]
    public void GeneratesANamedMemberInBothAwaitedShapes()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            public static partial class BoxExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain(
            "extension<T>(global::System.Threading.Tasks.Task<global::Waystone.Monads.Options.Extensions.Box<T>> boxTask)");
        run.Source.ShouldContain(
            "extension<T>(global::System.Threading.Tasks.ValueTask<global::Waystone.Monads.Options.Extensions.Box<T>> boxTask)");
        run.Source.ShouldContain(
            "public async global::System.Threading.Tasks.ValueTask<T> GetAsync()");
        run.Source.ShouldContain("return box.Get();");
    }

    [Fact]
    public void AcceptsTheMemberNameAsAnUnboundNameof()
    {
        GeneratorRun viaNameof = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember(nameof(Box<>.Get))]
            [GenerateAwaitedMember(nameof(Box<>.Map))]
            public static partial class BoxExtensions
            {
            }
            """);

        GeneratorRun viaString = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            [GenerateAwaitedMember("Map")]
            public static partial class BoxExtensions
            {
            }
            """);

        viaNameof.CompilationDiagnostics.ShouldBeEmpty();
        viaNameof.GeneratorDiagnostics.ShouldBeEmpty();
        viaNameof.Source.ShouldBe(viaString.Source);
        viaNameof.Source.ShouldContain("GetAsync");
        viaNameof.Source.ShouldContain("MapAsync<TOut>");
    }

    [Fact]
    public void PropagatesTheReceiverAndMemberConstraints()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Map")]
            public static partial class BoxExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("        where T : notnull");
        run.Source.ShouldContain("            where TOut : notnull");
        run.Source.ShouldContain("return box.Map<TOut>(map);");
    }

    [Fact]
    public void ForwardsTheExceptionTagAndPrefixesTheSummary()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            public static partial class BoxExtensions
            {
            }
            """);

        run.Source.ShouldContain(
            """<exception cref="T:System.InvalidOperationException">When the box is empty.</exception>""");
        run.Source.ShouldContain(
            """/// Asynchronously awaits the <see cref="T:Waystone.Monads.Options.Extensions.Box`1" /> then""");
        run.Source.ShouldContain("/// gets the value.");
    }

    [Fact]
    public void ReturnsANonGenericValueTaskForAVoidMember()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Peek")]
            public static partial class BoxExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain(
            "public async global::System.Threading.Tasks.ValueTask PeekAsync(global::System.Action<T> peek)");
        run.Source.ShouldContain("box.Peek(peek);");
        run.Source.ShouldNotContain("return box.Peek");
    }

    [Fact]
    public void EmitsNothingForAMemberWithNoAttribute()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            public static partial class BoxExtensions
            {
            }
            """);

        run.Source.ShouldNotContain("UnlistedAsync");
    }

    [Fact]
    public void ReplacesTheSummaryWhenTheAttributeSuppliesOne()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get", Summary = "Awaits the box and takes what is inside.")]
            public static partial class BoxExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("/// Awaits the box and takes what is inside.");
        run.Source.ShouldNotContain("Asynchronously awaits the");
        run.Source.ShouldNotContain("gets the value.");
        run.Source.ShouldContain(
            """<exception cref="T:System.InvalidOperationException">When the box is empty.</exception>""");
    }

    [Fact]
    public void DerivesFromAMarkedClassesOwnExtensionBlock()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : notnull
                {
                    /// <summary>Reads the value.</summary>
                    /// <param name="read">The reader.</param>
                    public async ValueTask<T> ReadAsync(Func<T, Task> read)
                    {
                        T value = box.Get();
                        await read.Invoke(value).ConfigureAwait(false);

                        return value;
                    }
                }
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain(
            "public async global::System.Threading.Tasks.ValueTask<T> ReadAsync(global::System.Func<T, global::System.Threading.Tasks.Task> read)");
        run.Source.ShouldContain("return await box.ReadAsync(read).ConfigureAwait(false);");
        run.Source.ShouldContain("/// reads the value.");
    }

    [Fact]
    public void DoesNotWrapAnAlreadyAwaitedReceiverBlock()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                extension<T>(Task<Box<T>> boxTask) where T : notnull
                {
                    public async ValueTask<T> AlreadyAsync()
                    {
                        Box<T> box = await boxTask.ConfigureAwait(false);

                        return box.Get();
                    }
                }
            }
            """);

        run.Generated.ShouldBeNull();
    }

    [Fact]
    public void EmitsNothingForAnUnmarkedClass()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : notnull
                {
                    public T Read() => box.Get();
                }
            }
            """);

        run.Generated.ShouldBeNull();
        run.GeneratorDiagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ReportsWsg0001WhenTheMarkedClassIsNotPartial()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            public static class BoxExtensions
            {
            }
            """);

        run.DiagnosticIds.ShouldBe(["WSG0001"]);
        run.Generated.ShouldBeNull();
    }

    [Fact]
    public void ReportsWsg0002WhenAMemberNameMatchesNothing()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            [GenerateAwaitedMember("Nonexistent")]
            public static partial class BoxExtensions
            {
            }
            """);

        run.DiagnosticIds.ShouldBe(["WSG0002"]);
        run.GeneratorDiagnostics.Single()
           .GetMessage()
           .ShouldContain("'Nonexistent'");
        run.Source.ShouldContain("GetAsync");
    }

    [Fact]
    public void EmitsTheWholeFileForAReceiverWithTwoGeneratedMembers()
    {
        GeneratorRun run = Verify.Run(
            """
            public abstract class Box<T> where T : notnull
            {
                /// <summary>Gets the value.</summary>
                /// <exception cref="System.InvalidOperationException">When the box is empty.</exception>
                public abstract T Get();

                /// <summary>Maps the value.</summary>
                /// <param name="map">The map function.</param>
                /// <typeparam name="TOut">The mapped type.</typeparam>
                public abstract Box<TOut> Map<TOut>(Func<T, TOut> map) where TOut : notnull;
            }

            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            [GenerateAwaitedMember("Map")]
            public static partial class BoxExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldBe(
            NormaliseNewlines(
                """
            #nullable enable

            namespace Waystone.Monads.Options.Extensions;

            public static partial class BoxExtensions
            {
                extension<T>(global::System.Threading.Tasks.Task<global::Waystone.Monads.Options.Extensions.Box<T>> boxTask)
                    where T : notnull
                {
                    /// <summary>
                    /// Asynchronously awaits the <see cref="T:Waystone.Monads.Options.Extensions.Box`1" /> then
                    /// gets the value.
                    /// </summary>
                    /// <exception cref="T:System.InvalidOperationException">When the box is empty.</exception>
                    public async global::System.Threading.Tasks.ValueTask<T> GetAsync()
                    {
                        global::Waystone.Monads.Options.Extensions.Box<T> box = await boxTask.ConfigureAwait(false);

                        return box.Get();
                    }

                    /// <summary>
                    /// Asynchronously awaits the <see cref="T:Waystone.Monads.Options.Extensions.Box`1" /> then
                    /// maps the value.
                    /// </summary>
                    /// <param name="map">The map function.</param>
                    /// <typeparam name="TOut">The mapped type.</typeparam>
                    public async global::System.Threading.Tasks.ValueTask<global::Waystone.Monads.Options.Extensions.Box<TOut>> MapAsync<TOut>(global::System.Func<T, TOut> map)
                        where TOut : notnull
                    {
                        global::Waystone.Monads.Options.Extensions.Box<T> box = await boxTask.ConfigureAwait(false);

                        return box.Map<TOut>(map);
                    }
                }

                extension<T>(global::System.Threading.Tasks.ValueTask<global::Waystone.Monads.Options.Extensions.Box<T>> boxTask)
                    where T : notnull
                {
                    /// <summary>
                    /// Asynchronously awaits the <see cref="T:Waystone.Monads.Options.Extensions.Box`1" /> then
                    /// gets the value.
                    /// </summary>
                    /// <exception cref="T:System.InvalidOperationException">When the box is empty.</exception>
                    public async global::System.Threading.Tasks.ValueTask<T> GetAsync()
                    {
                        global::Waystone.Monads.Options.Extensions.Box<T> box = await boxTask.ConfigureAwait(false);

                        return box.Get();
                    }

                    /// <summary>
                    /// Asynchronously awaits the <see cref="T:Waystone.Monads.Options.Extensions.Box`1" /> then
                    /// maps the value.
                    /// </summary>
                    /// <param name="map">The map function.</param>
                    /// <typeparam name="TOut">The mapped type.</typeparam>
                    public async global::System.Threading.Tasks.ValueTask<global::Waystone.Monads.Options.Extensions.Box<TOut>> MapAsync<TOut>(global::System.Func<T, TOut> map)
                        where TOut : notnull
                    {
                        global::Waystone.Monads.Options.Extensions.Box<T> box = await boxTask.ConfigureAwait(false);

                        return box.Map<TOut>(map);
                    }
                }
            }

            """));
    }


    private static string NormaliseNewlines(string text) =>
        text.Replace("\r\n", "\n");
}
