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
    #region configuration-error-code-factory
    internal sealed class ShoutingErrorCodeFactory : ErrorCodeFactory
    {
        public override ErrorCode FromException(Exception exception) =>
            new(exception.GetType().Name.ToUpperInvariant());
    }
    #endregion

    internal static void TheUsualCall()
    {
        #region configuration-the-usual-call
        MonadOptions.Configure(options => options
            .UseFallbackErrorCode("Unknown")
            .UseFallbackErrorMessage("Something went wrong."));
        #endregion
    }

    internal static void DoNotKeepTheBuilder()
    {
        #region configuration-do-not-keep-the-builder
        // Wrong. The second call does nothing.
        MonadOptionsBuilder? stashed = null;
        MonadOptions.Configure(options => stashed = options.UseFallbackErrorCode("A"));
        stashed!.UseFallbackErrorMessage("B");
        #endregion
    }

    internal static void TheCallShapeIsUnchangedFromSixDotX()
    {
        #region configuration-call-shape-unchanged
        MonadOptions.Configure(options => options.UseFallbackErrorCode("Unknown"));
        #endregion
    }

    internal static void CancellationAsFailure()
    {
        #region configuration-cancellation-as-failure
        MonadOptions.Configure(options => options.UseCancellationAsFailure());
        #endregion
    }

    internal static void PuttingCancellationAsFailureBack()
    {
        #region configuration-cancellation-as-failure-off
        MonadOptions.Configure(options => options.UseCancellationAsFailure(false));
        #endregion
    }

    internal static void AnErrorCodeFactory()
    {
        #region configuration-use-error-code-factory
        MonadOptions.Configure(
            options => options.UseErrorCodeFactory(new ShoutingErrorCodeFactory()));
        #endregion
    }

    internal static void Fallbacks()
    {
        #region configuration-fallbacks
        MonadOptions.Configure(options => options
            .UseFallbackErrorCode("unknown")                     // default: Unspecified
            .UseFallbackErrorMessage("Something went wrong!"));  // default: An unexpected error occurred.
        #endregion
    }

    internal static void AScope(string input)
    {
        #region configuration-a-scope
        using (MonadOptions.BeginScope(options => options.UseFallbackErrorCode("Debug")))
        {
            Result<int, Error> result = Result.Try<int>(() => int.Parse(input));

            _ = result;
        }

        // out here, your global configuration applies again
        #endregion
    }

    internal static void AScopeCanOverrideAnything()
    {
        #region configuration-scope-overrides
        using (MonadOptions.BeginScope(options => options
                   .UseErrorCodeFactory(new ShoutingErrorCodeFactory())
                   .UseFallbackErrorMessage("Something went wrong while debugging.")))
        {
            // ...
        }
        #endregion
    }
}
