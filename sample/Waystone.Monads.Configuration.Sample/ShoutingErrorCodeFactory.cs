namespace Waystone.Monads.Configuration.Sample;

using Configs;
using Results.Errors;

internal sealed class ShoutingErrorCodeFactory : ErrorCodeFactory
{
    public override ErrorCode FromException(Exception exception) =>
        new(base.FromException(exception).Value.ToUpperInvariant());
}
