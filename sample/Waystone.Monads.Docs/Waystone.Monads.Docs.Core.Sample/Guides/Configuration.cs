using Waystone.Monads.Configs;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Core.Sample.Guides;

/// <summary>
/// guides/configuration.md. The Logging section is not pinned here — every
/// method it names ships in Waystone.Monads.Extensions.Logging, and this
/// project references only the packages that page tells a reader to install.
/// packages/logging.md owns those blocks.
/// </summary>
internal static class ConfigurationGuide
{
    internal sealed class ShoutingErrorCodeFactory : ErrorCodeFactory
    {
        public override ErrorCode FromException(Exception exception) =>
            new(exception.GetType().Name.ToUpperInvariant());
    }

    internal static void TheUsualCall()
    {
        MonadOptions.Configure(options => options
            .UseFallbackErrorCode("Unknown")
            .UseFallbackErrorMessage("Something went wrong."));
    }

    internal static void DoNotKeepTheBuilder()
    {
        // Wrong. The second call does nothing.
        MonadOptionsBuilder? stashed = null;
        MonadOptions.Configure(options => stashed = options.UseFallbackErrorCode("A"));
        stashed!.UseFallbackErrorMessage("B");
    }

    internal static void TheCallShapeIsUnchangedFromSixDotX()
    {
        MonadOptions.Configure(options => options.UseFallbackErrorCode("Unknown"));
    }

    internal static void CancellationAsFailure()
    {
        MonadOptions.Configure(options => options.UseCancellationAsFailure());
    }

    internal static void PuttingCancellationAsFailureBack()
    {
        MonadOptions.Configure(options => options.UseCancellationAsFailure(false));
    }

    internal static void AnErrorCodeFactory()
    {
        MonadOptions.Configure(
            options => options.UseErrorCodeFactory(new ShoutingErrorCodeFactory()));
    }

    internal static void Fallbacks()
    {
        MonadOptions.Configure(options => options
            .UseFallbackErrorCode("unknown")                     // default: Unspecified
            .UseFallbackErrorMessage("Something went wrong!"));  // default: An unexpected error occurred.
    }

    internal static void AScope(string input)
    {
        using (MonadOptions.BeginScope(options => options.UseFallbackErrorCode("Debug")))
        {
            Result<int, Error> result = Result.Try<int>(() => int.Parse(input));

            _ = result;
        }

        // out here, your global configuration applies again
    }

    internal static void AScopeCanOverrideAnything()
    {
        using (MonadOptions.BeginScope(options => options
                   .UseErrorCodeFactory(new ShoutingErrorCodeFactory())
                   .UseFallbackErrorMessage("Something went wrong while debugging.")))
        {
            // ...
        }
    }
}
