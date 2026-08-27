; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 5.3.0

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

## Release 5.4.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WM1007 | Reliability | Warning | A type derives from Option or Result
WM1008 | Reliability | Warning | An Option or Result is declared nullable
WM1009 | Reliability | Warning | Option of bool or of an enum with a zero member
WM2014 | Usage | Info | FlatMap has been renamed to AndThen
WM2015 | Usage | Info | OrDefault on a value type cannot express the absent case

## Release 5.5.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WM1010 | Reliability | Warning | The default of a value type is used as an Option value

## Release 6.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WM1011 | Reliability | Warning | An async delegate is passed to a synchronous method
WM2016 | Usage | Info | An eager argument is not provably free to evaluate
WM2017 | Usage | Info | A delegate captures where a state overload exists

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WM1004 | Reliability | Warning | A default value converts to None
WM1007 | Reliability | Warning | A type derives from Option or Result
WM1009 | Reliability | Warning | Option of bool or of an enum with a zero member
WM1010 | Reliability | Warning | The default of a value type is used as an Option value
WM2014 | Usage | Info | FlatMap has been renamed to AndThen

## Release 6.2.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WM2018 | Usage | Info | An error code is reused across enums
WM2019 | Usage | Info | A generated error code is not in the error code registry
WM2020 | Usage | Info | The error code registry lists a code nothing generates

## Release 6.5.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WM2021 | Usage | Info | Option or Result state is tested through a property pattern

## Release 7.0.0

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WM2010 | Usage | Info | Result with identical type arguments cannot convert implicitly
