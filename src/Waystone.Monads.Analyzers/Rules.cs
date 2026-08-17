namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;

public static class Rules
{
    private const string Reliability = nameof(Reliability);
    private const string Usage = nameof(Usage);
    private const string Design = nameof(Design);

    public static readonly DiagnosticDescriptor SomeFromDefaultValue = Bug(
        "WM1001",
        "Some cannot hold a default value",
        "'Option.Some' throws at runtime when the value equals the default of '{0}'. Use 'Option.None<{0}>()' to express the absence of a value.",
        "The constructor of Some rejects a value equal to default(T), so this call always throws an InvalidOperationException.");

    public static readonly DiagnosticDescriptor NullAssignedToMonad = Bug(
        "WM1002",
        "Null assigned where an Option or Result is expected",
        "'{0}' is never null in correct use. Null here defeats the type and throws on the next member access.",
        "Option and Result are records, so the compiler permits null where one is expected. None and Err express absence and failure; null expresses neither.");

    public static readonly DiagnosticDescriptor DefaultOfMonad = Bug(
        "WM1003",
        "The default of an Option or Result is null",
        "'default({0})' is null, not an empty value. Construct the absent or failed case explicitly.",
        "Option and Result are reference types, so their default is null rather than None or Err.");

    public static readonly DiagnosticDescriptor DefaultValueConvertsToNone = Bug(
        "WM1004",
        "A default value converts to None",
        "The implicit conversion maps the default of '{0}' to None, so this produces None rather than a Some holding {1}",
        "Option<T>'s implicit conversion returns None when the value equals default(T). A default value silently becomes an absent one.");

    public static readonly DiagnosticDescriptor PossiblyNullPassedToSome = Bug(
        "WM1005",
        "A possibly null value is passed to Some",
        "'Option.Some' throws when this value is null. Use 'Option.FromNullable' to map null onto None.",
        "Some rejects a value equal to default(T), and the default of a reference type is null.");

    public static readonly DiagnosticDescriptor ResultDiscarded = Bug(
        "WM1006",
        "The Result of this call is discarded",
        "This call returns '{0}' and the value is unused, so a failure is silently ignored",
        "A discarded Result throws nothing and reports nothing. Match on it, or propagate it to a caller that will.");

    public static readonly DiagnosticDescriptor UnwrapUsed = Idiom(
        "WM2001",
        "Unwrap throws on the failure case",
        "'{0}' throws when there is no value. Prefer 'UnwrapOr', 'UnwrapOrElse', 'UnwrapOrDefault' or 'Match'.",
        "Unwrap converts a handled absence back into an unhandled exception, which is the thing Option and Result exist to avoid.");

    public static readonly DiagnosticDescriptor ExpectUsed = Idiom(
        "WM2002",
        "Expect throws on the failure case",
        "'{0}' throws when there is no value. Prefer 'UnwrapOr', 'UnwrapOrElse', 'UnwrapOrDefault' or 'Match'.",
        "Expect states an invariant and throws when it does not hold. That is defensible where the invariant is genuine, which is why this rule is separate from WM2001 and can be left disabled on its own.");

    public static readonly DiagnosticDescriptor ThrowInResultMember = Idiom(
        "WM2003",
        "Throw inside a member that returns Result",
        "This member returns '{0}', so a thrown exception bypasses the failure channel its signature promises",
        "A member returning Result declares that its failures are values. Throwing from it leaves callers with two failure mechanisms to handle.");

    public static readonly DiagnosticDescriptor GuardedUnwrap = Idiom(
        "WM2004",
        "A guarded unwrap can be a Match",
        "The '{0}' check and the unwrap inside it duplicate the same test. 'Match' or 'Inspect' expresses both branches once.",
        "Checking IsSome and then unwrapping asks the same question twice and relies on the reader to see that the answers agree.");

    public static readonly DiagnosticDescriptor MapThenFlatten = Idiom(
        "WM2005",
        "Map followed by Flatten is FlatMap",
        "'Map' followed by 'Flatten' is 'FlatMap'",
        "FlatMap exists for this composition and avoids materialising the nested monad.");

    public static readonly DiagnosticDescriptor CheckCombinedWithUnwrap = Idiom(
        "WM2006",
        "A check combined with an unwrap can be IsSomeAnd",
        "'{0}' combined with an unwrap of the same instance is '{1}'",
        "IsSomeAnd, IsNoneOr, IsOkAnd and IsErrAnd take the predicate and supply the value, removing the unwrap.");

    public static readonly DiagnosticDescriptor UnwrapOrWithDefault = Idiom(
        "WM2007",
        "UnwrapOr with a default is UnwrapOrDefault",
        "'UnwrapOr' given the default of '{0}' is 'UnwrapOrDefault'",
        "UnwrapOrDefault states the intent directly and does not repeat the type.");

    public static readonly DiagnosticDescriptor MonadComparedToNull = Idiom(
        "WM2008",
        "An Option or Result is compared to null",
        "'{0}' is never null in correct use, so this comparison tests the wrong thing. Use '{1}'.",
        "A null check on an Option or Result reads as an absence check but is not one. The absent case is None, and the failed case is Err.");

    public static readonly DiagnosticDescriptor NestedOption = Idiom(
        "WM2009",
        "A nested Option carries no more information than a flat one",
        "'{0}' has three states where two are meaningful. Flatten it.",
        "Option<Option<T>> distinguishes an absent outer from an absent inner, a distinction callers almost never act on.");

    public static readonly DiagnosticDescriptor ResultWithIdenticalTypeArguments = Idiom(
        "WM2010",
        "Result with identical type arguments cannot convert implicitly",
        "'{0}' has the same type for its Ok and its Err, which makes both implicit conversions ambiguous",
        "Result declares an implicit conversion from TOk and another from TErr. When those are the same type the compiler cannot choose, so every implicit conversion becomes a compile error and Ok and Err become indistinguishable to a reader.");

    public static readonly DiagnosticDescriptor DerivedMonadTypeDeclared = Idiom(
        "WM2011",
        "Declare the Option or Result base rather than one of its cases",
        "'{0}' names one case of '{1}'. Declare '{1}' so both cases are representable.",
        "A declaration naming Some, None, Ok or Err can only hold one of the two states, which defeats the point of the type.");

    public static readonly DiagnosticDescriptor NullableMemberAlongsideMonads = Idiom(
        "WM2012",
        "A nullable member sits alongside Option or Result members",
        "'{0}' returns a nullable type while '{1}' expresses absence through Option. Two conventions for absence in one type leaves callers guessing which applies.",
        "This type has already adopted Option or Result. A nullable return here is a second, weaker way of saying the same thing.");

    public static readonly DiagnosticDescriptor OptionDiscarded = Idiom(
        "WM2013",
        "The Option of this call is discarded",
        "This call returns '{0}' and the value is unused, so the call has no observable effect beyond its side effects",
        "Discarding an Option is less harmful than discarding a Result, but it is usually a sign the return value was meant to be handled.");

    public static readonly DiagnosticDescriptor NullableReturnCouldBeOption = Migration(
        "WM3001",
        "A nullable return could be an Option",
        "'{0}' returns '{1}'. An 'Option<{2}>' makes the absent case impossible to ignore.",
        "Disabled by default. Enable it while migrating a codebase onto Option; it fires on every nullable-returning member, not only those already using the library.");

    public static readonly DiagnosticDescriptor ThrowCouldBeResult = Migration(
        "WM3002",
        "A throw could be a Result",
        "This throw makes a failure invisible in the signature of '{0}'. A 'Result<TOk, Error>' return states it.",
        "Disabled by default. Enable it while migrating a codebase onto Result; it fires on every throw statement, including those a Result would not improve.");

    private static DiagnosticDescriptor Bug(
        string id,
        string title,
        string messageFormat,
        string description) =>
        Create(
            id,
            title,
            messageFormat,
            description,
            Reliability,
            DiagnosticSeverity.Warning,
            true);

    private static DiagnosticDescriptor Idiom(
        string id,
        string title,
        string messageFormat,
        string description) =>
        Create(
            id,
            title,
            messageFormat,
            description,
            Usage,
            DiagnosticSeverity.Info,
            true);

    private static DiagnosticDescriptor Migration(
        string id,
        string title,
        string messageFormat,
        string description) =>
        Create(
            id,
            title,
            messageFormat,
            description,
            Design,
            DiagnosticSeverity.Info,
            false);

    private static DiagnosticDescriptor Create(
        string id,
        string title,
        string messageFormat,
        string description,
        string category,
        DiagnosticSeverity severity,
        bool enabledByDefault) =>
        new DiagnosticDescriptor(
            id,
            title,
            messageFormat,
            category,
            severity,
            enabledByDefault,
            description,
            "https://draekien-industries.wpei.me/using-the-library/analyzer-rules#"
          + id.ToLowerInvariant());
}
