namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public sealed class ErrorCodeRegistryAnalyzerTests
{
    private const string Enum = """
        using Waystone.Monads.Results.Errors;

        namespace Ordering;

        [ErrorCodeProvider]
        public enum OrderErrorCode
        {
            NotFound,
            AlreadyShipped,
        }
        """;

    private const string Complete = """
        OrderErrorCode.AlreadyShipped
        OrderErrorCode.NotFound

        """;

    [Fact]
    public Task SaysNothingWhenTheRegistryMatches() =>
        Verify.RegistryAnalyzerAsync<ErrorCodeRegistryAnalyzer>(Enum, Complete);

    /// <summary>
    /// A project without the file has not opted in, so neither rule may fire however
    /// many attributed enums it declares.
    /// </summary>
    [Fact]
    public Task SaysNothingWithoutARegistry() =>
        Verify.RawAnalyzerAsync<ErrorCodeRegistryAnalyzer>(Enum);

    [Fact]
    public Task ReportsACodeTheRegistryDoesNotList() =>
        Verify.RegistryAnalyzerAsync<ErrorCodeRegistryAnalyzer>(
            Enum,
            """
            OrderErrorCode.NotFound

            """,
            Verify.Diagnostic(Rules.ErrorCodeMissingFromRegistry)
                  .WithSpan(9, 5, 9, 19)
                  .WithArguments(
                       "Ordering.OrderErrorCode.AlreadyShipped",
                       "OrderErrorCode.AlreadyShipped",
                       "ErrorCodes.txt"));

    [Fact]
    public Task ReportsEveryCodeWhenTheRegistryIsEmpty() =>
        Verify.RegistryAnalyzerAsync<ErrorCodeRegistryAnalyzer>(
            Enum,
            "",
            Verify.Diagnostic(Rules.ErrorCodeMissingFromRegistry)
                  .WithSpan(9, 5, 9, 19)
                  .WithArguments(
                       "Ordering.OrderErrorCode.AlreadyShipped",
                       "OrderErrorCode.AlreadyShipped",
                       "ErrorCodes.txt"),
            Verify.Diagnostic(Rules.ErrorCodeMissingFromRegistry)
                  .WithSpan(8, 5, 8, 13)
                  .WithArguments(
                       "Ordering.OrderErrorCode.NotFound",
                       "OrderErrorCode.NotFound",
                       "ErrorCodes.txt"));

    [Fact]
    public Task ReportsAnEntryNothingGenerates() =>
        Verify.RegistryAnalyzerAsync<ErrorCodeRegistryAnalyzer>(
            Enum,
            """
            OrderErrorCode.AlreadyShipped
            OrderErrorCode.Cancelled
            OrderErrorCode.NotFound

            """,
            Verify.Diagnostic(Rules.StaleErrorCodeRegistryEntry)
                  .WithSpan("ErrorCodes.txt", 2, 1, 2, 25)
                  .WithArguments("OrderErrorCode.Cancelled", "ErrorCodes.txt"));

    /// <summary>
    /// The registry holds the generated code, so a format changes every line of it.
    /// A file listing the default codes is entirely stale once the enum declares a
    /// format, and entirely missing the new ones.
    /// </summary>
    [Fact]
    public Task FollowsTheDeclaredFormat() =>
        Verify.RegistryAnalyzerAsync<ErrorCodeRegistryAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            namespace Ordering;

            [ErrorCodeProvider(Format = "order.{member:kebab}")]
            public enum OrderErrorCode
            {
                NotFound,
            }
            """,
            """
            order.not-found

            """);

    [Fact]
    public Task IgnoresCommentsAndBlankLines() =>
        Verify.RegistryAnalyzerAsync<ErrorCodeRegistryAnalyzer>(
            Enum,
            """
            # Every error code this project publishes. Reviewed on change.

            OrderErrorCode.AlreadyShipped
            OrderErrorCode.NotFound

            """);

    /// <summary>
    /// A registry in a project with no attributed enum at all is entirely stale,
    /// which is the shape of a project that removed its last provider.
    /// </summary>
    [Fact]
    public Task ReportsEveryEntryWhenNothingIsAttributed() =>
        Verify.RegistryAnalyzerAsync<ErrorCodeRegistryAnalyzer>(
            """
            namespace Ordering;

            public enum OrderErrorCode
            {
                NotFound,
            }
            """,
            """
            OrderErrorCode.NotFound

            """,
            Verify.Diagnostic(Rules.StaleErrorCodeRegistryEntry)
                  .WithSpan("ErrorCodes.txt", 1, 1, 1, 24)
                  .WithArguments("OrderErrorCode.NotFound", "ErrorCodes.txt"));

    /// <summary>
    /// Two enums generating one code report twice, once on each member. Both point at
    /// the same line to add, which reads as redundant until you notice that either
    /// member alone is enough to require the line and neither is the one to remove —
    /// which of the two should keep the code is WM2018's report, not this one's.
    /// </summary>
    [Fact]
    public Task ReportsOnEveryMemberBehindASharedCode() =>
        Verify.RegistryAnalyzerAsync<ErrorCodeRegistryAnalyzer>(
            """
            using Waystone.Monads.Results.Errors;

            namespace Ordering;

            [ErrorCodeProvider(Format = "order.{member:kebab}")]
            public enum OrderErrorCode
            {
                NotFound,
            }

            [ErrorCodeProvider(Format = "order.{member:kebab}")]
            public enum ShipmentErrorCode
            {
                NotFound,
            }
            """,
            "",
            Verify.Diagnostic(Rules.ErrorCodeMissingFromRegistry)
                  .WithSpan(8, 5, 8, 13)
                  .WithArguments(
                       "Ordering.OrderErrorCode.NotFound",
                       "order.not-found",
                       "ErrorCodes.txt"),
            Verify.Diagnostic(Rules.ErrorCodeMissingFromRegistry)
                  .WithSpan(14, 5, 14, 13)
                  .WithArguments(
                       "Ordering.ShipmentErrorCode.NotFound",
                       "order.not-found",
                       "ErrorCodes.txt"));
}
