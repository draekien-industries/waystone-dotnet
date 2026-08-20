namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;

public static class Rules
{
    private const string Reliability = nameof(Reliability);
    private const string Usage = nameof(Usage);
    private const string Design = nameof(Design);

    /// <remarks>
    /// The only survivor of the rules that encoded the pre-v6 default-value
    /// invariant. It reports the null case alone, which is what Some still
    /// rejects; the value-type half retired with the relaxation, because a
    /// default is now an ordinary value. Keep the message about null rather
    /// than about default(T).
    /// </remarks>
    public static readonly DiagnosticDescriptor SomeFromDefaultValue = Bug(
        "WM1001",
        "Do not pass null to Some",
        "'Option.Some' throws at runtime when the value is null. Use 'Option.None<{0}>()' to express the absence of a value.",
        "The constructor of Some rejects null, so this call always throws an ArgumentNullException.");

    /// <remarks>
    /// Warning survives the false-positive question, examined in DRA-77. The
    /// worst case found is a null reaching a target the analyzer cannot prove is
    /// non-nullable, which happens in a tuple element, a collection-initializer
    /// element and a local function return, because
    /// <see cref="NullAndDefaultAnalyzer.TargetIsExplicitlyNullable" /> matches
    /// an enumerated set of positions and defaults to reporting. None of those
    /// is working code: reaching them requires declaring the monad as
    /// <c>Option&lt;T&gt;?</c>, which WM1008 forbids. It reports alongside
    /// WM1008 for the collection-initializer element and the local-function
    /// return, but not for the tuple element:
    /// <see cref="Semantics.IsDeclarationTypePosition" /> has no case for a
    /// tuple element, so WM1008 never sees that declaration and this rule is the
    /// only one that fires there. That gap is WM1008's, tracked in DRA-98, not a
    /// reason to change this rule. Do not lower the tier to accommodate them,
    /// and do not widen the exclusion either — it would silence a real null.
    /// </remarks>
    public static readonly DiagnosticDescriptor NullAssignedToMonad = Bug(
        "WM1002",
        "Do not assign null to an Option or Result",
        "'{0}' is never null in correct use. Null here defeats the type and throws on the next member access.",
        "Option and Result are records, so the compiler permits null where one is expected. None and Err express absence and failure; null expresses neither.");

    /// <remarks>
    /// Warning survives the false-positive question, examined in DRA-77, on the
    /// same reasoning as WM1002: every position where the target's nullability
    /// cannot be proven requires an <c>Option&lt;T&gt;?</c> declaration, which
    /// WM1008 forbids — though it does not see the declaration in a tuple
    /// element, so there this rule reports alone, which is DRA-98's gap rather
    /// than this rule's. The generic case that prompted the question is safe —
    /// <c>default</c> written for an unconstrained <c>T</c> has <c>T</c> as its
    /// type rather than the monad, so the rule stays quiet however <c>T</c> is
    /// instantiated.
    /// </remarks>
    public static readonly DiagnosticDescriptor DefaultOfMonad = Bug(
        "WM1003",
        "Do not use the default of an Option or Result",
        "'default({0})' is null, not an empty value. Construct the absent or failed case explicitly.",
        "Option and Result are reference types, so their default is null rather than None or Err.");

    public static readonly DiagnosticDescriptor PossiblyNullPassedToSome = Bug(
        "WM1005",
        "Prefer FromNullable over Some for a possibly null value",
        "'Option.Some' throws when this value is null. Use 'Option.FromNullable' to map null onto None.",
        "Some rejects null, and a value the compiler cannot prove is non-null may be one.");

    public static readonly DiagnosticDescriptor ResultDiscarded = Bug(
        "WM1006",
        "Do not discard a Result",
        "'{0}' returns '{1}' and the value is unused, so a failure is silently ignored",
        "A discarded Result throws nothing and reports nothing. Match on it, or propagate it to a caller that will.");

    /// <remarks>
    /// Reports alongside WM2011 on a nullable derived case such as
    /// <c>Some&lt;int&gt;?</c>. The overlap is deliberate — both statements are
    /// independently true and each has its own code fix — so the report is made
    /// ahead of the early returns in <see cref="DeclaredTypeAnalyzer" /> that
    /// would otherwise suppress one of them.
    /// </remarks>
    public static readonly DiagnosticDescriptor NullableMonadDeclared = Bug(
        "WM1008",
        "Do not declare an Option or Result as nullable",
        "'{0}?' has three states where two are meaningful. Drop the annotation — '{1}' already expresses the case you are reaching for.",
        "Option and Result are records, so the compiler accepts a nullable annotation on one. The annotation adds a third state to a type whose whole purpose is to have exactly two, and the null it admits throws on the next member access rather than being handled as an absence.");

    /// <remarks>
    /// Exists because the break that produces this code is silent. Until v6 an
    /// async factory bound to a dedicated <c>Try</c> overload; removing that
    /// overload does not fail the call, it rebinds it to
    /// <c>Try&lt;T&gt;(Func&lt;T&gt;)</c> with <c>T</c> inferred as the task,
    /// which satisfies <c>notnull</c>. Only a call site that assigns to an
    /// explicitly typed local gets a compiler error, so the rule carries the
    /// whole migration for every other shape. No code fix: renaming to
    /// <c>TryAsync</c> alone leaves the caller with an unawaited
    /// <c>Task&lt;Option&lt;T&gt;&gt;</c>, and where the await belongs is not
    /// something the fix can decide.
    /// </remarks>
    public static readonly DiagnosticDescriptor AsyncFactoryPassedToTry = Bug(
        "WM1011",
        "Do not pass an async factory to Try",
        "'Try' invokes this factory without awaiting it, so it produces '{0}' and catches nothing the asynchronous work throws. Use 'TryAsync'.",
        "Try catches what its factory throws while invoking it. A factory that returns a task returns before its work has run, so the exception surfaces later on the caller's await with Try's handling already bypassed, and the value is the task rather than its result.");

    public static readonly DiagnosticDescriptor UnwrapUsed = Idiom(
        "WM2001",
        "Prefer UnwrapOr or Match over Unwrap",
        "'{0}' throws when there is no value. Prefer 'UnwrapOr', 'UnwrapOrElse', 'UnwrapOrDefault' or 'Match'.",
        "Unwrap converts a handled absence back into an unhandled exception, which is the thing Option and Result exist to avoid.");

    /// <remarks>
    /// Separate from WM2001 so that a codebase can leave this one disabled on
    /// its own. Expect states an invariant, which is defensible where the
    /// invariant is genuine, and that is a different judgement from the one
    /// WM2001 asks for.
    /// </remarks>
    public static readonly DiagnosticDescriptor ExpectUsed = Idiom(
        "WM2002",
        "Prefer UnwrapOr or Match over Expect",
        "'{0}' throws when there is no value. Prefer 'UnwrapOr', 'UnwrapOrElse', 'UnwrapOrDefault' or 'Match'.",
        "Expect states an invariant and throws when it does not hold, so it converts a handled absence into an unhandled exception wherever the invariant turns out not to be genuine.");

    public static readonly DiagnosticDescriptor ThrowInResultMember = Idiom(
        "WM2003",
        "Do not throw from a member that returns Result",
        "'{0}' returns '{1}', so a thrown exception bypasses the failure channel its signature promises",
        "A member returning Result declares that its failures are values. Throwing from it leaves callers with two failure mechanisms to handle.");

    public static readonly DiagnosticDescriptor GuardedUnwrap = Idiom(
        "WM2004",
        "Prefer Match over a guarded unwrap",
        "The '{0}' check and the unwrap inside it duplicate the same test. 'Match' or 'Inspect' expresses both branches once.",
        "Checking IsSome and then unwrapping asks the same question twice and relies on the reader to see that the answers agree.");

    public static readonly DiagnosticDescriptor MapThenFlatten = Idiom(
        "WM2005",
        "Prefer AndThen over Map followed by Flatten",
        "'Map' followed by 'Flatten' is 'AndThen'",
        "AndThen exists for this composition and avoids materialising the nested monad.",
        WellKnownDiagnosticTags.Unnecessary);

    public static readonly DiagnosticDescriptor CheckCombinedWithUnwrap = Idiom(
        "WM2006",
        "Prefer IsSomeAnd over a check combined with an unwrap",
        "'{0}' combined with an unwrap of the same instance is '{1}'",
        "IsSomeAnd, IsNoneOr, IsOkAnd and IsErrAnd take the predicate and supply the value, removing the unwrap.");

    public static readonly DiagnosticDescriptor UnwrapOrWithDefault = Idiom(
        "WM2007",
        "Prefer UnwrapOrDefault over UnwrapOr with a default",
        "'UnwrapOr' given the default of '{0}' is 'UnwrapOrDefault'",
        "UnwrapOrDefault states the intent directly and does not repeat the type.");

    public static readonly DiagnosticDescriptor MonadComparedToNull = Idiom(
        "WM2008",
        "Do not compare an Option or Result to null",
        "'{0}' is never null in correct use, so this comparison tests the wrong thing. Use '{1}'.",
        "A null check on an Option or Result reads as an absence check but is not one. The absent case is None, and the failed case is Err.");

    public static readonly DiagnosticDescriptor NestedOption = Idiom(
        "WM2009",
        "Flatten a nested Option",
        "'{0}' has three states where two are meaningful. Flatten it.",
        "Option<Option<T>> distinguishes an absent outer from an absent inner, a distinction callers almost never act on.");

    public static readonly DiagnosticDescriptor ResultWithIdenticalTypeArguments = Idiom(
        "WM2010",
        "Do not give a Result identical type arguments",
        "'{0}' has the same type for its Ok and its Err, which makes both implicit conversions ambiguous",
        "Result declares an implicit conversion from TOk and another from TErr. When those are the same type the compiler cannot choose, so every implicit conversion becomes a compile error and Ok and Err become indistinguishable to a reader.");

    public static readonly DiagnosticDescriptor DerivedMonadTypeDeclared = Idiom(
        "WM2011",
        "Prefer the Option or Result base over one of its cases",
        "'{0}' names one case of '{1}'. Declare '{1}' so both cases are representable.",
        "A declaration naming Some, None, Ok or Err can only hold one of the two states, which defeats the point of the type.");

    public static readonly DiagnosticDescriptor NullableMemberAlongsideMonads = Idiom(
        "WM2012",
        "Do not mix nullable members with Option or Result members",
        "'{0}' returns a nullable type while '{1}' expresses absence through '{2}'. Two conventions for absence in one type leaves callers guessing which applies.",
        "This type has already adopted Option or Result. A nullable return here is a second, weaker way of saying the same thing.");

    public static readonly DiagnosticDescriptor OptionDiscarded = Idiom(
        "WM2013",
        "Do not discard an Option",
        "'{0}' returns '{1}' and the value is unused, so the call has no observable effect beyond its side effects",
        "Discarding an Option is less harmful than discarding a Result, but it is usually a sign the return value was meant to be handled.");

    /// <remarks>
    /// Points the opposite way to WM2007 for a value type, deliberately: that
    /// rule removes a repeated type from UnwrapOr, this one asks whether the
    /// default was meant as a value. Both inform rather than warn so the
    /// context around each is surfaced and the caller decides. Applying
    /// WM2007's code fix therefore produces code this rule reports, which
    /// <c>CodeFixTests.ReplacesUnwrapOrOfADefaultWithUnwrapOrDefault</c>
    /// asserts in its fixed state rather than hides.
    /// </remarks>
    public static readonly DiagnosticDescriptor OrDefaultOnAValueType = Idiom(
        "WM2015",
        "Prefer UnwrapOrNull over UnwrapOrDefault on a value type",
        "'{0}' hands back {3}, the default of '{1}', when there is no value, and nothing distinguishes that from a real {3}. '{2}' returns null instead.",
        "T? on a type parameter constrained only to notnull is an annotation rather than a Nullable<T>, so for a value type UnwrapOrDefault and MapOrDefault return 0, false or default(Guid) for the absent case. That is legitimate where the caller genuinely wants the default.");

    public static readonly DiagnosticDescriptor NullableReturnCouldBeOption = Migration(
        "WM3001",
        "Prefer an Option over a nullable return",
        "'{0}' returns '{1}'. An 'Option<{2}>' makes the absent case impossible to ignore.",
        "Disabled by default. Enable it while migrating a codebase onto Option; it fires on every nullable-returning member, not only those already using the library.");

    public static readonly DiagnosticDescriptor ThrowCouldBeResult = Migration(
        "WM3002",
        "Prefer a Result over a throw",
        "This throw makes a failure invisible in the signature of '{0}'. A 'Result<TOk, Error>' return states it.",
        "Disabled by default. Enable it while migrating a codebase onto Result; it fires on every throw statement, including those a Result would not improve.");

    private static DiagnosticDescriptor Bug(
        string id,
        string title,
        string messageFormat,
        string description,
        params string[] tags) =>
        Create(
            id,
            title,
            messageFormat,
            description,
            Reliability,
            DiagnosticSeverity.Warning,
            true,
            tags);

    private static DiagnosticDescriptor Idiom(
        string id,
        string title,
        string messageFormat,
        string description,
        params string[] tags) =>
        Create(
            id,
            title,
            messageFormat,
            description,
            Usage,
            DiagnosticSeverity.Info,
            true,
            tags);

    private static DiagnosticDescriptor Migration(
        string id,
        string title,
        string messageFormat,
        string description,
        params string[] tags) =>
        Create(
            id,
            title,
            messageFormat,
            description,
            Design,
            DiagnosticSeverity.Info,
            false,
            tags);

    private static DiagnosticDescriptor Create(
        string id,
        string title,
        string messageFormat,
        string description,
        string category,
        DiagnosticSeverity severity,
        bool enabledByDefault,
        params string[] tags) =>
        new DiagnosticDescriptor(
            id,
            title,
            messageFormat,
            category,
            severity,
            enabledByDefault,
            description,
            "https://draekien-industries.wpei.me/using-the-library/analyzer-rules#"
          + id.ToLowerInvariant(),
            tags);
}
