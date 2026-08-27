; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 7.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WMS2001 | Usage | Info | A monad is asserted through a bool or an unwrapped value
WMS2002 | Usage | Info | An await is parenthesised to assert synchronously
