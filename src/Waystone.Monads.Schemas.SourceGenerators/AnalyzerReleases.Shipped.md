; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 7.1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WMSC0001 | Usage | Error | A generated schema must be declared partial
WMSC0002 | Usage | Error | Do not hide a schema's parameterless constructor
WMSC0003 | Usage | Error | Do not declare a member the generator emits
WMSC0004 | Usage | Error | Match the Into lambda to the number of fields
WMSC0005 | Usage | Warning | Do not pass a value-producing field to Refine
WMSC0006 | Usage | Error | Do not reach an asynchronous rule from a field set
WMSC0007 | Usage | Warning | Call Schema.Fields through the name Schema
WMSC0008 | Usage | Warning | Name a field whose path cannot be read from its argument
WMSC0009 | Usage | Info | Prefer a named schema over Schema.For
