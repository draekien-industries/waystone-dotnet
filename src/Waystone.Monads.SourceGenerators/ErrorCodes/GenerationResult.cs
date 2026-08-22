namespace Waystone.Monads.SourceGenerators.ErrorCodes;

internal sealed record GenerationResult(
    string HintName,
    string? Source,
    EquatableArray<DiagnosticInfo> Diagnostics);
