namespace Waystone.Monads.Diagnostics;

using System.Diagnostics;
using Configs;

/// <summary>
/// The payload of the
/// <see cref="MonadDiagnostics.ConfigurationNotAppliedEventName" /> diagnostic
/// event.
/// </summary>
/// <remarks>
/// Subscribers receive this boxed as <see cref="object" />, since
/// <see cref="DiagnosticListener" /> is untyped; cast to this record to read it.
/// It carries no data on purpose. The event reports that a read of
/// <see cref="MonadOptions" /> happened before configuration registered with a
/// container reached the library, and the two things a subscriber would want —
/// which settings were expected, and who read early — are both unavailable here.
/// The expected settings are still sitting unbuilt in the container, and a read
/// carries no caller information.
/// <para>
/// The one thing that does identify the offending call site is the subscriber's
/// own stack. Subscribers run synchronously on the thread that read, so a stack
/// trace captured in the handler names the read that beat the configuration. That
/// is the intended use of this event, and the reason it is worth subscribing to in
/// a test suite even when nothing consumes it in production.
/// </para>
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed record ConfigurationNotApplied;
