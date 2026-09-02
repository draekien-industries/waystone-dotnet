namespace Waystone.Monads.Schemas.SourceGenerators;

internal sealed record GenerationResult(
    string HintName,
    string? Source,
    DiagnosticInfo? Diagnostic)
{
    public static GenerationResult Emitted(string hintName, string source) =>
        new GenerationResult(hintName, source, null);

    public static GenerationResult Failed(
        string hintName,
        DiagnosticInfo diagnostic) =>
        new GenerationResult(hintName, null, diagnostic);
}
