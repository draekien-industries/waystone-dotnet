namespace Waystone.Monads.Schemas.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    string? FilePath,
    TextSpan Span,
    LinePositionSpan LineSpan,
    string Subject,
    string? Offender)
{
    public static DiagnosticInfo Create(
        DiagnosticDescriptor descriptor,
        Location location,
        string subject,
        string? offender = null)
    {
        FileLinePositionSpan mapped = location.GetLineSpan();

        return new DiagnosticInfo(
            descriptor,
            location.SourceTree?.FilePath,
            location.SourceSpan,
            mapped.Span,
            subject,
            offender);
    }

    public Diagnostic ToDiagnostic()
    {
        Location location = FilePath is null
            ? Location.None
            : Location.Create(FilePath, Span, LineSpan);

        return Offender is null
            ? Diagnostic.Create(Descriptor, location, Subject)
            : Diagnostic.Create(Descriptor, location, Subject, Offender);
    }
}
