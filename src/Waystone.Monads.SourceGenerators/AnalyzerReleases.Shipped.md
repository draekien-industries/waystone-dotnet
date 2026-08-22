; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 6.2.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WMG0001 | Usage | Error | An error code provider enum cannot be a flags enum
WMG0002 | Usage | Error | An error code provider enum cannot alias a value
WMG0003 | Usage | Error | An error code provider member name collides with a generated type
WMG0004 | Usage | Error | The Waystone.Monads error types are not resolvable
WMG0005 | Usage | Error | The error code format cannot be used
WMG0006 | Usage | Error | The error code format does not distinguish members
