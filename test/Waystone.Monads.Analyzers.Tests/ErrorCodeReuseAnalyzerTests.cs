namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class ErrorCodeReuseAnalyzerTests
{
    [Fact]
    public Task FlagsTwoEnumsSharingANameAndAMember() =>
        Verify.RawAnalyzerAsync<ErrorCodeReuseAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            namespace Ordering
            {
                [ErrorCodeCatalog]
                internal enum OrderError
                {
                    NotFound,
                }
            }

            namespace Shipping
            {
                [ErrorCodeCatalog]
                internal enum OrderError
                {
                    {|#0:NotFound|},
                }
            }
            """,
            Verify.Diagnostic(Rules.ErrorCodeReusedAcrossEnums)
                  .WithLocation(0)
                  .WithArguments(
                       "Ordering.OrderError.NotFound",
                       "Shipping.OrderError.NotFound",
                       "OrderError.NotFound"));

    [Fact]
    public Task FlagsEveryColligingMemberOfThePair() =>
        Verify.RawAnalyzerAsync<ErrorCodeReuseAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            namespace Ordering
            {
                [ErrorCodeCatalog]
                internal enum OrderError
                {
                    NotFound,
                    Cancelled,
                }
            }

            namespace Shipping
            {
                [ErrorCodeCatalog]
                internal enum OrderError
                {
                    {|#0:NotFound|},
                    {|#1:Cancelled|},
                }
            }
            """,
            Verify.Diagnostic(Rules.ErrorCodeReusedAcrossEnums)
                  .WithLocation(0)
                  .WithArguments(
                       "Ordering.OrderError.NotFound",
                       "Shipping.OrderError.NotFound",
                       "OrderError.NotFound"),
            Verify.Diagnostic(Rules.ErrorCodeReusedAcrossEnums)
                  .WithLocation(1)
                  .WithArguments(
                       "Ordering.OrderError.Cancelled",
                       "Shipping.OrderError.Cancelled",
                       "OrderError.Cancelled"));

    [Fact]
    public Task IgnoresASharedNameWithNoSharedMember() =>
        Verify.RawAnalyzerAsync<ErrorCodeReuseAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            namespace Ordering
            {
                [ErrorCodeCatalog]
                internal enum OrderError
                {
                    NotFound,
                }
            }

            namespace Shipping
            {
                [ErrorCodeCatalog]
                internal enum OrderError
                {
                    AlreadyShipped,
                }
            }
            """);

    [Fact]
    public Task IgnoresASharedMemberUnderDifferentEnumNames() =>
        Verify.RawAnalyzerAsync<ErrorCodeReuseAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            namespace Ordering
            {
                [ErrorCodeCatalog]
                internal enum OrderError
                {
                    NotFound,
                }
            }

            namespace Shipping
            {
                [ErrorCodeCatalog]
                internal enum ShipmentError
                {
                    NotFound,
                }
            }
            """);

    [Fact]
    public Task IgnoresAnEnumThatIsNotAProvider() =>
        Verify.RawAnalyzerAsync<ErrorCodeReuseAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            namespace Ordering
            {
                [ErrorCodeCatalog]
                internal enum OrderError
                {
                    NotFound,
                }
            }

            namespace Shipping
            {
                internal enum OrderError
                {
                    NotFound,
                }
            }
            """);

    [Fact]
    public Task IgnoresAnAliasWithinOneEnum() =>
        Verify.RawAnalyzerAsync<ErrorCodeReuseAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            namespace Ordering
            {
                [ErrorCodeCatalog]
                internal enum OrderError
                {
                    NotFound = 1,
                    Missing = 1,
                }
            }
            """);

    /// <summary>
    /// The rule keys on the generated code, not the enum name, so a shared format
    /// makes differently named enums collide. This is the false negative the rule
    /// would have had if it kept deriving the code from the enum name.
    /// </summary>
    [Fact]
    public Task FlagsACollisionCausedByASharedFormat() =>
        Verify.RawAnalyzerAsync<ErrorCodeReuseAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            namespace Ordering
            {
                [ErrorCodeCatalog(Format = "order.{member:kebab}")]
                internal enum OrderError
                {
                    NotFound,
                }
            }

            namespace Shipping
            {
                [ErrorCodeCatalog(Format = "order.{member:kebab}")]
                internal enum ShipmentError
                {
                    {|#0:NotFound|},
                }
            }
            """,
            Verify.Diagnostic(Rules.ErrorCodeReusedAcrossEnums)
                  .WithLocation(0)
                  .WithArguments(
                       "Ordering.OrderError.NotFound",
                       "Shipping.ShipmentError.NotFound",
                       "order.not-found"));

    /// <summary>
    /// And the false positive: two enums sharing a name no longer collide once their
    /// formats differ.
    /// </summary>
    [Fact]
    public Task IgnoresASharedNameWhenTheFormatsDiffer() =>
        Verify.RawAnalyzerAsync<ErrorCodeReuseAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            namespace Ordering
            {
                [ErrorCodeCatalog(Format = "order.{member}")]
                internal enum OrderError
                {
                    NotFound,
                }
            }

            namespace Shipping
            {
                [ErrorCodeCatalog(Format = "shipping.{member}")]
                internal enum OrderError
                {
                    NotFound,
                }
            }
            """);

    [Fact]
    public Task AppliesTheAssemblyWideFormat() =>
        Verify.RawAnalyzerAsync<ErrorCodeReuseAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            [assembly: ErrorCodeFormat("{member:kebab}")]

            namespace Ordering
            {
                [ErrorCodeCatalog]
                internal enum OrderError
                {
                    NotFound,
                }
            }

            namespace Shipping
            {
                [ErrorCodeCatalog]
                internal enum ShipmentError
                {
                    {|#0:NotFound|},
                }
            }
            """,
            Verify.Diagnostic(Rules.ErrorCodeReusedAcrossEnums)
                  .WithLocation(0)
                  .WithArguments(
                       "Ordering.OrderError.NotFound",
                       "Shipping.ShipmentError.NotFound",
                       "not-found"));
}
