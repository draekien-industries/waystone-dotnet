; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 7.3.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WA0001 | Reliability | Error | A delegate returning a guarded type is invoked without the guard
WA0002 | Usage | Error | A public delegate parameter must not return a Task of Option or Result
WA0003 | Usage | Error | A public member must not return a Task of Option or Result
