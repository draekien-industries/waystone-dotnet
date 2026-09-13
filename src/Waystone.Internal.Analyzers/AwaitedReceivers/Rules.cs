namespace Waystone.Internal.Analyzers.AwaitedReceivers;

using Microsoft.CodeAnalysis;

internal static class Rules
{
    /// <summary>
    /// Reported at the parameter rather than at the generated member, because the
    /// generated member is where the attribute is <em>missing</em> and a diagnostic
    /// there would name a file the author cannot edit.
    /// </summary>
    /// <remarks>
    /// The two fixes the message offers are not equal work.
    /// <c>[ExcludeFromAwaitedReceivers]</c> is one line and costs a caller holding a
    /// task the ability to reach the member in a single chain; teaching the generator
    /// to carry the attribute is the right fix wherever the attribute means the same
    /// thing on both sides of a forward, and the caller-info four are the ones that
    /// already qualified.
    /// </remarks>
    public static readonly DiagnosticDescriptor UncarriedParameterAttribute = new(
        "WA0004",
        "Do not apply a parameter attribute the generator drops",
        "'{0}' on parameter '{1}' of '{2}' is not carried onto the generated "
      + "awaited shapes, so keep the member off them with "
      + "[ExcludeFromAwaitedReceivers] or teach the generator to carry it",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "The awaited receivers generator writes a parameter's modifiers, type, "
      + "name and default value, and of its attributes only the caller-info "
      + "four, whose values the compiler supplies at the outer call site and the "
      + "forward passes straight on. Every other attribute is left off the "
      + "generated member, which still compiles and still ships - and the public "
      + "API baseline does not record parameter attributes, so RS0016 and RS0017 "
      + "do not report the difference either.");
}
