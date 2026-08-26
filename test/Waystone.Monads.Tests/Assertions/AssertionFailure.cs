namespace Waystone.Monads.Assertions;

using Shouldly;
using System;
using System.Threading.Tasks;

/// <remarks>
/// The asynchronous half is written out rather than delegating to
/// <c>Should.ThrowAsync</c>, which does not catch what the awaited assertions throw:
/// the exception escapes and fails the test carrying the very message the test meant
/// to inspect. The synchronous half is a thin wrapper over <c>Should.Throw</c> and
/// exists only so both shapes read the same at a call site.
/// </remarks>
internal static class AssertionFailure
{
    internal static string From(Action act) =>
        Should.Throw<ShouldAssertException>(act).Message;

    internal static async Task<string> FromAsync(Func<Task> act)
    {
        ShouldAssertException? caught = null;

        try
        {
            await act().ConfigureAwait(false);
        }
        catch (ShouldAssertException exception)
        {
            caught = exception;
        }

        if (caught is null)
        {
            throw new ShouldAssertException(
                "Expected the assertion to fail but the call succeeded");
        }

        return caught.Message;
    }
}
