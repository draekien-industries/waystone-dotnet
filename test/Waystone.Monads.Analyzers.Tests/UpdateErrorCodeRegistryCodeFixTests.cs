namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public sealed class UpdateErrorCodeRegistryCodeFixTests
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

    [Fact]
    public Task WritesAnEmptyRegistry() =>
        Verify.RegistryCodeFixAsync<ErrorCodeRegistryAnalyzer,
            UpdateErrorCodeRegistryCodeFix>(
            Enum,
            "",
            """
            OrderErrorCode.AlreadyShipped
            OrderErrorCode.NotFound

            """,
            Verify.Diagnostic(Rules.ErrorCodeMissingFromRegistry)
                  .WithSpan(8, 5, 8, 13)
                  .WithArguments(
                       "Ordering.OrderErrorCode.NotFound",
                       "OrderErrorCode.NotFound",
                       "ErrorCodes.txt"),
            Verify.Diagnostic(Rules.ErrorCodeMissingFromRegistry)
                  .WithSpan(9, 5, 9, 19)
                  .WithArguments(
                       "Ordering.OrderErrorCode.AlreadyShipped",
                       "OrderErrorCode.AlreadyShipped",
                       "ErrorCodes.txt"));

    /// <summary>
    /// One invocation writes the whole file, so a run started from a missing code
    /// takes the stale entry out at the same time. That is what makes the absence of a
    /// fix-all provider harmless.
    /// </summary>
    [Fact]
    public Task AddsAndRemovesInOnePass() =>
        Verify.RegistryCodeFixAsync<ErrorCodeRegistryAnalyzer,
            UpdateErrorCodeRegistryCodeFix>(
            Enum,
            """
            OrderErrorCode.Cancelled
            OrderErrorCode.NotFound

            """,
            """
            OrderErrorCode.AlreadyShipped
            OrderErrorCode.NotFound

            """,
            Verify.Diagnostic(Rules.ErrorCodeMissingFromRegistry)
                  .WithSpan(9, 5, 9, 19)
                  .WithArguments(
                       "Ordering.OrderErrorCode.AlreadyShipped",
                       "OrderErrorCode.AlreadyShipped",
                       "ErrorCodes.txt"),
            Verify.Diagnostic(Rules.StaleErrorCodeRegistryEntry)
                  .WithSpan("ErrorCodes.txt", 1, 1, 1, 25)
                  .WithArguments("OrderErrorCode.Cancelled", "ErrorCodes.txt"));

    /// <summary>
    /// The leading comment block survives the rewrite, so a header explaining what the
    /// file is for does not have to be restored after every fix.
    /// </summary>
    [Fact]
    public Task KeepsTheLeadingComment() =>
        Verify.RegistryCodeFixAsync<ErrorCodeRegistryAnalyzer,
            UpdateErrorCodeRegistryCodeFix>(
            Enum,
            """
            # Reviewed on change.
            OrderErrorCode.NotFound

            """,
            """
            # Reviewed on change.
            OrderErrorCode.AlreadyShipped
            OrderErrorCode.NotFound

            """,
            Verify.Diagnostic(Rules.ErrorCodeMissingFromRegistry)
                  .WithSpan(9, 5, 9, 19)
                  .WithArguments(
                       "Ordering.OrderErrorCode.AlreadyShipped",
                       "OrderErrorCode.AlreadyShipped",
                       "ErrorCodes.txt"));

    /// <summary>
    /// The written file is ordinally sorted whatever order the entries arrived in, so
    /// adding a code shows up as one added line rather than as a reordering.
    /// </summary>
    [Fact]
    public Task SortsWhatItWrites() =>
        Verify.RegistryCodeFixAsync<ErrorCodeRegistryAnalyzer,
            UpdateErrorCodeRegistryCodeFix>(
            Enum,
            """
            OrderErrorCode.NotFound

            """,
            """
            OrderErrorCode.AlreadyShipped
            OrderErrorCode.NotFound

            """,
            Verify.Diagnostic(Rules.ErrorCodeMissingFromRegistry)
                  .WithSpan(9, 5, 9, 19)
                  .WithArguments(
                       "Ordering.OrderErrorCode.AlreadyShipped",
                       "OrderErrorCode.AlreadyShipped",
                       "ErrorCodes.txt"));
}
