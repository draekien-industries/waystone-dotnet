; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WM1001 | Reliability | Warning | Some cannot hold a default value
WM1002 | Reliability | Warning | Null assigned where an Option or Result is expected
WM1003 | Reliability | Warning | The default of an Option or Result is null
WM1004 | Reliability | Warning | A default value converts to None
WM1005 | Reliability | Warning | A possibly null value is passed to Some
WM1006 | Reliability | Warning | The Result of this call is discarded
WM2001 | Usage | Info | Unwrap throws on the failure case
WM2002 | Usage | Info | Expect throws on the failure case
WM2003 | Usage | Info | Throw inside a member that returns Result
WM2004 | Usage | Info | A guarded unwrap can be a Match
WM2005 | Usage | Info | Map followed by Flatten is FlatMap
WM2006 | Usage | Info | A check combined with an unwrap can be IsSomeAnd
WM2007 | Usage | Info | UnwrapOr with a default is UnwrapOrDefault
WM2008 | Usage | Info | An Option or Result is compared to null
WM2009 | Usage | Info | A nested Option carries no more information than a flat one
WM2010 | Usage | Info | Result with identical type arguments cannot convert implicitly
WM2011 | Usage | Info | Declare the Option or Result base rather than one of its cases
WM2012 | Usage | Info | A nullable member sits alongside Option or Result members
WM2013 | Usage | Info | The Option of this call is discarded
WM3001 | Design | Disabled | A nullable return could be an Option
WM3002 | Design | Disabled | A throw could be a Result
