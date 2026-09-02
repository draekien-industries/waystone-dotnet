namespace Waystone.Monads.Schemas.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// A diagnostic reduced to values, so it survives the incremental pipeline. That
/// pipeline compares what it caches for equality, and a <c>Location</c> holds a
/// reference to a syntax tree it must not keep alive.
/// </summary>
internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    string? FilePath,
    TextSpan Span,
    LinePositionSpan LineSpan,
    EquatableArray<string> MessageArgs)
{
    public static DiagnosticInfo Create(
        DiagnosticDescriptor descriptor,
        Location location,
        params string[] messageArgs)
    {
        FileLinePositionSpan mapped = location.GetLineSpan();

        return new DiagnosticInfo(
            descriptor,
            location.SourceTree?.FilePath,
            location.SourceSpan,
            mapped.Span,
            new EquatableArray<string>(messageArgs));
    }

    public Diagnostic ToDiagnostic()
    {
        Location location = FilePath is null
            ? Location.None
            : Location.Create(FilePath, Span, LineSpan);

        return Diagnostic.Create(
            Descriptor,
            location,
            messageArgs: MessageArgs.Values);
    }
}
