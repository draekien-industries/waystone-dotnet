namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// The generator does not run in these tests, so each source declares the catalog
/// members by hand in the shape <c>ErrorCodeCatalogWriter</c> emits. That keeps
/// these tests about the rewrite rather than about the generator, which
/// <c>Waystone.Monads.SourceGenerators.Tests</c> already covers.
/// </summary>
public sealed class UseGeneratedErrorCodeCodeFixTests
{
    private const string Catalog = """
        [ErrorCodeCatalog]
        internal enum OrderError
        {
            NotFound,
        }

        internal static class OrderErrorCatalog
        {
            internal static class Codes
            {
                public static readonly ErrorCode NotFound = new ErrorCode("OrderError.NotFound");
            }

            internal static class Errors
            {
                public static Error NotFound(string message) =>
                    new Error(Codes.NotFound, message);
            }

            public static ErrorCode ToErrorCode(this OrderError value) =>
                Codes.NotFound;

            public static Error ToError(this OrderError value, string message) =>
                Errors.NotFound(message);
        }

        """;

    [Fact]
    public Task RewritesErrorCodeFromEnumToTheCatalogConstant() =>
        Verify.CompilerCodeFixAsync<UseGeneratedErrorCodeCodeFix>(
            Catalog
          + """
            internal class Subject
            {
                internal ErrorCode Code() => {|#0:ErrorCode.FromEnum(OrderError.NotFound)|};
            }
            """,
            Catalog
          + """
            internal class Subject
            {
                internal ErrorCode Code() => OrderErrorCatalog.Codes.NotFound;
            }
            """,
            DiagnosticResult.CompilerWarning("CS0618").WithLocation(0));

    [Fact]
    public Task RewritesErrorFromEnumToTheCatalogFactory() =>
        Verify.CompilerCodeFixAsync<UseGeneratedErrorCodeCodeFix>(
            Catalog
          + """
            internal class Subject
            {
                internal Error Make() => {|#0:Error.FromEnum(OrderError.NotFound, "gone")|};
            }
            """,
            Catalog
          + """
            internal class Subject
            {
                internal Error Make() => OrderErrorCatalog.Errors.NotFound("gone");
            }
            """,
            DiagnosticResult.CompilerWarning("CS0618").WithLocation(0));

    [Fact]
    public Task RewritesResultErrKeepingItsTypeArgument() =>
        Verify.CompilerCodeFixAsync<UseGeneratedErrorCodeCodeFix>(
            Catalog
          + """
            internal class Subject
            {
                internal Result<int, Error> Fail() =>
                    {|#0:Result.Err<int>(OrderError.NotFound, "gone")|};
            }
            """,
            Catalog
          + """
            internal class Subject
            {
                internal Result<int, Error> Fail() =>
                    Result.Err<int>(OrderErrorCatalog.Errors.NotFound("gone"));
            }
            """,
            DiagnosticResult.CompilerWarning("CS0618").WithLocation(0));

    /// <summary>
    /// A value only known at run time cannot name a catalog member, so the fix falls
    /// back to the generated extension. This is the case the catalog form cannot
    /// cover, and getting it wrong would produce code that does not compile.
    /// </summary>
    [Fact]
    public Task UsesTheExtensionWhenTheMemberIsNotNamed() =>
        Verify.CompilerCodeFixAsync<UseGeneratedErrorCodeCodeFix>(
            Catalog
          + """
            internal class Subject
            {
                internal Error Make(OrderError value) =>
                    {|#0:Error.FromEnum(value, "gone")|};
            }
            """,
            Catalog
          + """
            internal class Subject
            {
                internal Error Make(OrderError value) =>
                    value.ToError("gone");
            }
            """,
            DiagnosticResult.CompilerWarning("CS0618").WithLocation(0));
}
