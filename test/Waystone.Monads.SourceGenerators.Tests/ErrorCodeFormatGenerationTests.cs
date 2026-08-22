namespace Waystone.Monads.SourceGenerators;

using Shouldly;
using Xunit;

public sealed class ErrorCodeFormatGenerationTests
{
    private const string Enum = """
        [ErrorCodeProvider]
        public enum OrderError
        {
            NotFound,
        }
        """;

    [Fact]
    public void DefaultsToTheFactoryScheme()
    {
        GeneratorRun run = Verify.Run(Enum);

        run.CompilationDiagnostics.ShouldBeEmpty();
        run.GeneratorDiagnostics.ShouldBeEmpty();
        run.Source.ShouldContain("NotFound = \"OrderError.NotFound\";");
    }

    [Fact]
    public void TakesTheFormatFromTheEnum()
    {
        GeneratorRun run = Verify.Run(
            """
            [ErrorCodeProvider(Format = "order.{member:kebab}")]
            public enum OrderError
            {
                NotFound,
                AlreadyShipped,
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();
        run.Source.ShouldContain("NotFound = \"order.not-found\";");
        run.Source.ShouldContain("AlreadyShipped = \"order.already-shipped\";");
    }

    [Fact]
    public void TakesTheFormatFromTheAssembly()
    {
        GeneratorRun run = Verify.RunWithAssemblyAttributes(
            """[assembly: ErrorCodeFormat("{enum:kebab}/{member:snake}")]""",
            Enum);

        run.CompilationDiagnostics.ShouldBeEmpty();
        run.Source.ShouldContain("NotFound = \"order-error/not_found\";");
    }

    [Fact]
    public void TheEnumOverridesTheAssembly()
    {
        GeneratorRun run = Verify.RunWithAssemblyAttributes(
            """[assembly: ErrorCodeFormat("{enum:kebab}/{member:snake}")]""",
            """
            [ErrorCodeProvider(Format = "{member:upper}")]
            public enum OrderError
            {
                NotFound,
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();
        run.Source.ShouldContain("NotFound = \"NOTFOUND\";");
    }

    [Fact]
    public void AppliesTheFormatToTheUndeclaredValueArmToo()
    {
        GeneratorRun run = Verify.Run(
            """
            [ErrorCodeProvider(Format = "order.{member:kebab}")]
            public enum OrderError
            {
                NotFound,
            }
            """);

        run.CompilationDiagnostics.ShouldBeEmpty();
        run.Source.ShouldContain("return \"order.\" + value.ToString();");

        run.Source.ShouldContain(
            "return new global::Waystone.Monads.Results.Errors.ErrorCode(\"order.\" + value.ToString());");
    }

    [Fact]
    public void ReportsAnUnusableFormat()
    {
        GeneratorRun run = Verify.Run(
            """
            [ErrorCodeProvider(Format = "{member:pascal}")]
            public enum OrderError
            {
                NotFound,
            }
            """);

        run.DiagnosticIds.ShouldBe(["WMG0005"]);
        run.Source.ShouldBeEmpty();

        run.GeneratorDiagnostics[0]
           .GetMessage()
           .ShouldContain("'pascal' is not a casing");
    }

    [Fact]
    public void ReportsAFormatThatDoesNotDistinguishMembers()
    {
        GeneratorRun run = Verify.Run(
            """
            [ErrorCodeProvider(Format = "{enum:kebab}")]
            public enum OrderError
            {
                NotFound,
                AlreadyShipped,
            }
            """);

        run.DiagnosticIds.ShouldBe(["WMG0006"]);
        run.Source.ShouldBeEmpty();
    }

    /// <summary>
    /// A custom format changes what <c>WM2018</c> sees, since that rule keys on the
    /// generated code rather than on the enum name. Two enums with different names
    /// can now collide, and two enums sharing a name need not.
    /// </summary>
    [Fact]
    public void AFormatCanMakeDifferentlyNamedEnumsShareACode()
    {
        GeneratorRun first = Verify.Run(
            """
            [ErrorCodeProvider(Format = "order.{member:kebab}")]
            public enum OrderError
            {
                NotFound,
            }
            """);

        GeneratorRun second = Verify.Run(
            """
            [ErrorCodeProvider(Format = "order.{member:kebab}")]
            public enum ShipmentError
            {
                NotFound,
            }
            """);

        first.Source.ShouldContain("\"order.not-found\"");
        second.Source.ShouldContain("\"order.not-found\"");
    }
}
