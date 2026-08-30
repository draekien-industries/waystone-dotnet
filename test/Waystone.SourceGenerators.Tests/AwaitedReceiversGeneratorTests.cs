namespace Waystone.SourceGenerators;

using System;
using System.Linq;
using Shouldly;
using Waystone.SourceGenerators.AwaitedReceivers;
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
    public void KeepsTwoBlocksApartWhenTheyShareAReceiverButNotItsConstraints()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : struct
                {
                    /// <summary>Reads the value or null.</summary>
                    public T? OrNull() => box.Get();
                }
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        const string taskReceiver =
            "    extension<T>(global::System.Threading.Tasks.Task<global::Waystone.Monads.Options.Extensions.Box<T>> boxTask)\n";

        run.Source.ShouldContain(taskReceiver + "        where T : struct\n");
        run.Source.ShouldContain(taskReceiver + "        where T : notnull\n");

        run.Source.ShouldContain(
            "public async global::System.Threading.Tasks.ValueTask<T?> OrNullAsync()");
        run.Source.ShouldContain(
            "public async global::System.Threading.Tasks.ValueTask<T> GetAsync()");
    }

    [Fact]
    public void LeavesAnExtensionBlockMembersOwnTypeArgumentsToInference()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : notnull
                {
                    /// <summary>Reshapes the value.</summary>
                    /// <param name="shape">The shaper.</param>
                    /// <typeparam name="TOut">The reshaped type.</typeparam>
                    public ValueTask<TOut> ReshapeAsync<TOut>(Func<T, TOut> shape)
                        where TOut : struct =>
                        new(shape.Invoke(box.Get()));
                }
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("public async global::System.Threading.Tasks.ValueTask<TOut> ReshapeAsync<TOut>(");
        run.Source.ShouldContain("where TOut : struct");
        run.Source.ShouldContain("return await box.ReshapeAsync(shape).ConfigureAwait(false);");
    }

    [Fact]
    public void WritesAnInstanceMembersOwnTypeArgumentsOnTheCall()
    {
        GeneratorRun run = Verify.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember(nameof(Box<>.Map))]
            public static partial class BoxExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("return box.Map<TOut>(map);");
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


    [Fact]
    public void RendersEveryKindOfTypeParameterConstraint()
    {
        GeneratorRun run = Verify.Run(
            """
            public abstract class Crate<T> where T : notnull
            {
                /// <summary>Constrains every way the language allows.</summary>
                public abstract void Exotic<TStruct, TClass, TMaybe, TFace, TNew>()
                    where TStruct : struct
                    where TClass : class
                    where TMaybe : class?
                    where TFace : IDisposable
                    where TNew : new();
            }

            [GenerateAwaitedReceivers(typeof(Crate<>))]
            [GenerateAwaitedMember("Exotic")]
            public static partial class CrateExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("where TStruct : struct");
        run.Source.ShouldContain("where TClass : class");
        run.Source.ShouldContain("where TMaybe : class?");
        run.Source.ShouldContain("where TFace : global::System.IDisposable");
        run.Source.ShouldContain("where TNew : new()");
    }

    [Fact]
    public void RendersAnUnmanagedConstraint()
    {
        GeneratorRun run = Verify.Run(
            """
            public abstract class Crate<T> where T : notnull
            {
                /// <summary>Takes an unmanaged type.</summary>
                public abstract void Raw<TRaw>() where TRaw : unmanaged;
            }

            [GenerateAwaitedReceivers(typeof(Crate<>))]
            [GenerateAwaitedMember("Raw")]
            public static partial class CrateExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("where TRaw : unmanaged");
    }

    [Fact]
    public void FindsTypeParametersBehindAnArrayAndANestedGeneric()
    {
        GeneratorRun run = Verify.Run(
            """
            public abstract class Crate<T> where T : notnull
            {
                /// <summary>Takes the parameter through an array.</summary>
                /// <param name="values">The values.</param>
                public abstract void Many(T[] values);

                /// <summary>Takes the parameter nested twice.</summary>
                /// <param name="nested">The nested values.</param>
                public abstract void Deep(Func<List<T[]>, int> nested);
            }

            [GenerateAwaitedReceivers(typeof(Crate<>))]
            [GenerateAwaitedMember("Many")]
            [GenerateAwaitedMember("Deep")]
            public static partial class CrateExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("ManyAsync(T[] values)");
        run.Source.ShouldContain("crate.Many(values);");
        run.Source.ShouldContain("global::System.Collections.Generic.List<T[]>");
    }

    [Fact]
    public void EscapesAKeywordParameterAndWritesItsDefault()
    {
        GeneratorRun run = Verify.Run(
            """
            public abstract class Crate<T> where T : notnull
            {
                /// <summary>Names a parameter after a keyword.</summary>
                /// <param name="else">The fallback.</param>
                /// <param name="count">How many.</param>
                /// <param name="label">The label.</param>
                /// <param name="other">The other crate.</param>
                public abstract void Fallback(
                    T @else,
                    int count = 3,
                    string label = "x",
                    Crate<T>? other = null);
            }

            [GenerateAwaitedReceivers(typeof(Crate<>))]
            [GenerateAwaitedMember("Fallback")]
            public static partial class CrateExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("T @else");
        run.Source.ShouldContain("int count = 3");
        run.Source.ShouldContain("string label = \"x\"");
        run.Source.ShouldContain("other = null");
        run.Source.ShouldContain("crate.Fallback(@else, count, label, other);");
    }

    [Fact]
    public void ReusesTheCachedOutputWhenTheDriverRunsTwiceOverTheSameInput()
    {
        (string first, string second) = Verify.RunTwice(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            [GenerateAwaitedMember("Map")]
            public static partial class BoxExtensions
            {
            }
            """);

        second.ShouldBe(first);
    }

    [Fact]
    public void ReusesTheCachedDiagnosticWhenTheDriverRunsTwice()
    {
        (string first, string second) = Verify.RunTwice(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("NoSuchMember")]
            public static partial class BoxExtensions
            {
            }
            """);

        second.ShouldBe(first);
    }

    [Fact]
    public void RebuildsWhenTheSecondRunAsksForADifferentMember()
    {
        (string first, string second) = Verify.RunTwice(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            public static partial class BoxExtensions
            {
            }
            """,
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("Get")]
            [GenerateAwaitedMember("Map")]
            public static partial class BoxExtensions
            {
            }
            """);

        first.ShouldContain("GetAsync()");
        first.ShouldNotContain("MapAsync");

        second.ShouldContain("GetAsync()");
        second.ShouldContain("MapAsync");
    }

    [Fact]
    public void RebuildsWhenTheSecondRunReportsADifferentDiagnostic()
    {
        (string first, string second) = Verify.RunTwice(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("NoSuchMember")]
            public static partial class BoxExtensions
            {
            }
            """,
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            [GenerateAwaitedMember("StillMissing")]
            public static partial class BoxExtensions
            {
            }
            """);

        first.ShouldContain("NoSuchMember");
        second.ShouldContain("StillMissing");
        second.ShouldNotBe(first);
    }

    [Fact]
    public void FindsTypeParametersInsideAnArrayReturnType()
    {
        GeneratorRun run = Verify.Run(
            """
            public abstract class Crate<T> where T : notnull
            {
                /// <summary>Returns every value.</summary>
                public abstract T[] All();
            }

            [GenerateAwaitedReceivers(typeof(Crate<>))]
            [GenerateAwaitedMember("All")]
            public static partial class CrateExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("AllAsync()");
        run.Source.ShouldContain("ValueTask<T[]>");
    }

    [Fact]
    public void UsesTheSummaryOverrideOnAnUndocumentedMember()
    {
        GeneratorRun run = Verify.Run(
            """
            public abstract class Crate<T> where T : notnull
            {
                public abstract T Bare();
            }

            [GenerateAwaitedReceivers(typeof(Crate<>))]
            [GenerateAwaitedMember("Bare", Summary = "Written by hand.")]
            public static partial class CrateExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("Written by hand.");
        run.Source.ShouldContain("BareAsync()");
    }

    [Fact]
    public void EmitsNoDocumentationForAnUndocumentedMemberWithNoOverride()
    {
        GeneratorRun run = Verify.Run(
            """
            public abstract class Crate<T> where T : notnull
            {
                public abstract T Bare();
            }

            [GenerateAwaitedReceivers(typeof(Crate<>))]
            [GenerateAwaitedMember("Bare")]
            public static partial class CrateExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("BareAsync()");
        run.Source.ShouldNotContain("<summary>");
    }

    [Fact]
    public void ForwardsDocumentationThatCarriesNoSummary()
    {
        GeneratorRun run = Verify.Run(
            """
            public abstract class Crate<T> where T : notnull
            {
                /// <param name="value">The value.</param>
                public abstract void Put(T value);
            }

            [GenerateAwaitedReceivers(typeof(Crate<>))]
            [GenerateAwaitedMember("Put")]
            public static partial class CrateExtensions
            {
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();

        run.Source.ShouldContain("PutAsync(T value)");
        run.Source.ShouldContain("<param name=\"value\">");
        run.Source.ShouldNotContain("<summary>");
    }

    [Fact]
    public void DoesNotMarkTheEmittedSourceAsGeneratedCode()
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

        var receiverFiles = run.HintNames
                              .Where(
                                   hintName => !hintName.EndsWith(
                                       GeneratedAttributes.HintName,
                                       StringComparison.Ordinal))
                              .ToList();

        receiverFiles.ShouldNotBeEmpty();

        foreach (string hintName in receiverFiles)
        {
            hintName.ShouldEndWith(".AwaitedReceivers.cs");
            hintName.ShouldNotEndWith(".g.cs");
        }

        run.Source.ShouldNotContain("GeneratedCode");
        run.Source.ShouldNotContain("autogenerated");
        run.Source.ShouldNotContain("auto-generated");
    }

    private static string NormaliseNewlines(string text) =>
        text.Replace("\r\n", "\n");
}
