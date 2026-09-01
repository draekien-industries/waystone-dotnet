namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Lives here rather than in <c>Waystone.Monads.Schema.Tests</c>, which has
/// <c>InternalsVisibleTo</c> and would compile a derived field happily, proving
/// nothing.
/// </summary>
/// <remarks>
/// Both members that keep the hierarchy shut are internal and abstract, so an
/// outside type fails once per member it cannot see. Adding another internal
/// abstract member to <c>Field</c> means adding its CS0534 here in the same
/// change.
/// </remarks>
public class SchemaClosedHierarchyTests
{
    [Fact]
    public Task AnOutsideAssemblyCannotDeriveFromField() =>
        Verify.SchemaCompilerDiagnosticsAsync(
            """
            using Waystone.Monads.Schemas;

            public sealed class {|#0:Evil|} : Field
            {
            }
            """,
            DiagnosticResult.CompilerError("CS0534").WithLocation(0),
            DiagnosticResult.CompilerError("CS0534").WithLocation(0));

    [Fact]
    public Task AnOutsideAssemblyCannotDeriveFromFieldOfT() =>
        Verify.SchemaCompilerDiagnosticsAsync(
            """
            using Waystone.Monads.Schemas;

            public sealed class {|#0:Evil|} : Field<int>
            {
            }
            """,
            DiagnosticResult.CompilerError("CS0534").WithLocation(0),
            DiagnosticResult.CompilerError("CS0534").WithLocation(0));

    [Fact]
    public Task AnOutsideAssemblyMayDeriveAComposedSchema() =>
        Verify.SchemaCompilerDiagnosticsAsync(
            """
            using Waystone.Monads.Results;
            using Waystone.Monads.Schemas;

            public sealed class Fine : Schema<string, string>
            {
                protected override Result<string, SchemaViolation> Configure(
                    string subject) =>
                    Result.Ok<string, SchemaViolation>(subject);
            }
            """);
}
