namespace Vagrant.Staffing;

using Waystone.Monads.Results.Errors;

/// <summary>Every way a clerk can fail to take on a piece of work.</summary>
/// <remarks>
/// <para>
/// The generated codes read <c>vagrant.staffing.no_clerk_free</c> and
/// <c>vagrant.staffing.clerk_already_engaged</c>. A code is derived from the member's
/// own name, so renaming a member here changes what a patron's client sees.
/// </para>
/// <para>
/// Internal, and reachable from <c>Vagrant.Host</c> only through
/// <c>InternalsVisibleTo</c>. The host reads
/// <c>StaffingErrorCatalog.Codes.NoClerkFree</c> to choose a status code; nothing
/// branches on the message.
/// </para>
/// </remarks>
[ErrorCodeCatalog(Format = "vagrant.staffing.{member:snake}")]
internal enum StaffingError
{
    /// <summary>Every clerk in the shop is already holding an errand.</summary>
    NoClerkFree,

    /// <summary>The clerk asked was already holding an errand.</summary>
    ClerkAlreadyEngaged,
}
