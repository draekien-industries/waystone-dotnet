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
                [ErrorCodeProvider]
                internal enum OrderError
                {
                    NotFound,
                }
            }

            namespace Shipping
            {
                [ErrorCodeProvider]
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
                [ErrorCodeProvider]
                internal enum OrderError
                {
                    NotFound,
                    Cancelled,
                }
            }

            namespace Shipping
            {
                [ErrorCodeProvider]
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
                [ErrorCodeProvider]
                internal enum OrderError
                {
                    NotFound,
                }
            }

            namespace Shipping
            {
                [ErrorCodeProvider]
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
                [ErrorCodeProvider]
                internal enum OrderError
                {
                    NotFound,
                }
            }

            namespace Shipping
            {
                [ErrorCodeProvider]
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
                [ErrorCodeProvider]
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
                [ErrorCodeProvider]
                internal enum OrderError
                {
                    NotFound = 1,
                    Missing = 1,
                }
            }
            """);
}
