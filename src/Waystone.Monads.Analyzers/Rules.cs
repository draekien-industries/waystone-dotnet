namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;

public static class Rules
{
    private const string Reliability = nameof(Reliability);
    private const string Usage = nameof(Usage);
    private const string Design = nameof(Design);

    /// <remarks>
    /// Narrowed to the null case. The value-type half moved to WM1010, which
    /// forecasts the v6 relaxation, because no single messageFormat is true of
    /// both: in v6 a null still throws and a value-type default becomes an
    /// ordinary Some. This rule survives v6 with only its exception type
    /// changing, so keep the message about null rather than about default(T).
    /// </remarks>
    public static readonly DiagnosticDescriptor SomeFromDefaultValue = Bug(
        "WM1001",
        "Do not pass null to Some",
        "'Option.Some' throws at runtime when the value is null. Use 'Option.None<{0}>()' to express the absence of a value.",
        "The constructor of Some rejects null, so this call always throws an InvalidOperationException.");

    /// <remarks>
    /// Warning survives the false-positive question, examined in DRA-77. The
    /// worst case found is a null reaching a target the analyzer cannot prove is
    /// non-nullable, which happens in a tuple element, a collection-initializer
    /// element and a local function return, because
    /// <see cref="NullAndDefaultAnalyzer.TargetIsExplicitlyNullable" /> matches
    /// an enumerated set of positions and defaults to reporting. None of those
    /// is working code: reaching them requires declaring the monad as
    /// <c>Option&lt;T&gt;?</c>, which WM1008 forbids outright. So the rule fires
    /// on code that is already wrong, and reports alongside WM1008 where that
    /// rule sees the declaration. Do not lower the tier to accommodate them, and
    /// do not widen the exclusion either — it would silence a real null.
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
    /// WM1008 forbids. The generic case that prompted the question is safe —
    /// <c>default</c> written for an unconstrained <c>T</c> has <c>T</c> as its
    /// type rather than the monad, so the rule stays quiet however <c>T</c> is
    /// instantiated.
    /// </remarks>
    public static readonly DiagnosticDescriptor DefaultOfMonad = Bug(
        "WM1003",
        "Do not use the default of an Option or Result",
        "'default({0})' is null, not an empty value. Construct the absent or failed case explicitly.",
        "Option and Result are reference types, so their default is null rather than None or Err.");

    /// <remarks>
    /// Not narrowed alongside WM1001, because it never covered the null case in
    /// the first place: <see cref="NullAndDefaultAnalyzer" /> returns early on a
    /// null-constant operand, so this rule only ever fires on the default of a
    /// value type. That is why the v6 forecast goes in this message rather than
    /// into WM1010 as a second position — one sentence is true of everything the
    /// rule reports. It follows that the rule becomes wholly false in v6 and is
    /// retired there, rather than surviving narrowed as WM1001 does.
    /// </remarks>
    public static readonly DiagnosticDescriptor DefaultValueConvertsToNone = Bug(
        "WM1004",
        "Do not convert a default value to an Option implicitly",
        "The implicit conversion maps the default of '{0}' to None, so this produces None rather than a Some holding {1} today. In v6 it produces a Some holding {1}.",
        "Option<T>'s implicit conversion returns None when the value equals default(T), so a default value silently becomes an absent one. In v6 only null maps onto None, and this expression changes meaning rather than failing to compile.");

    public static readonly DiagnosticDescriptor PossiblyNullPassedToSome = Bug(
        "WM1005",
        "Prefer FromNullable over Some for a possibly null value",
        "'Option.Some' throws when this value is null. Use 'Option.FromNullable' to map null onto None.",
        "Some rejects a value equal to default(T), and the default of a reference type is null.");

    public static readonly DiagnosticDescriptor ResultDiscarded = Bug(
        "WM1006",
        "Do not discard a Result",
        "'{0}' returns '{1}' and the value is unused, so a failure is silently ignored",
        "A discarded Result throws nothing and reports nothing. Match on it, or propagate it to a caller that will.");

    /// <remarks>
    /// Warning severity with no code fix is deliberate, and is the exception to
    /// the rule that the two go together: the correction rewrites the type and
    /// cascades to every caller, which a fixer cannot see. It is earned because
    /// both hierarchies close in v6, so the marked code stops compiling. Do not
    /// treat it as precedent for another rule.
    /// </remarks>
    public static readonly DiagnosticDescriptor DerivesFromMonad = Bug(
        "WM1007",
        "Do not derive from Option or Result",
        "'{0}' derives from '{1}', which has exactly two cases. Compose the monad instead of inheriting from it.",
        "Option and Result exist to have two states, and every member that switches on them handles two. A third case is invisible to Match, so it takes whichever branch the base case falls into. Both hierarchies close in v6, and a derived type will not compile against it.");

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
    /// Scoped to bool and enums with a zero member deliberately. The hazard is
    /// general — <c>Option&lt;int&gt;</c> breaks on 0 and
    /// <c>Option&lt;Guid&gt;</c> on <c>Guid.Empty</c> — but widening a
    /// declaration rule to every value type would fire across code that works
    /// today, and a Warning ships enabled to every consumer on upgrade. WM1001
    /// and WM1004 already cover the call site that provably passes a default;
    /// this rule covers the declaration.
    /// </remarks>
    public static readonly DiagnosticDescriptor OptionOfZeroValuedType = Bug(
        "WM1009",
        "Do not use Option over bool or a zero-member enum",
        "'{0}' cannot hold the default of '{1}', because 'Option.Some' throws on it. {2}.",
        "Option.Some rejects a value equal to default(T), so an Option over a type whose zero is a meaningful value cannot represent part of its own domain.");

    /// <remarks>
    /// Exists only to forecast the v6 relaxation, and is retired there rather
    /// than reworded — see DRA-92. It is a separate id from WM1001 rather than a
    /// reworded one because a descriptor has exactly one messageFormat and no
    /// single sentence covers both a null and a value-type default once v6
    /// treats them differently. It covers the Option.Some argument position
    /// only: the implicit conversion is WM1004's, which never reported a null
    /// and so carries its own forecast, and Option.FromNullable is deliberately
    /// left out because the analyzer would only see a literal nobody writes.
    /// Warning is earned on the present tense of the message — every span
    /// reported throws today — and it is the severity these spans already
    /// reported at under WM1001, so no consumer sees a new count.
    /// </remarks>
    public static readonly DiagnosticDescriptor DefaultOfValueTypeInOption =
        Bug(
            "WM1010",
            "Do not use the default of a value type as an Option value",
            "{0} is the default of '{1}', so 'Option.Some' throws today. In v6 it returns a Some holding {0}. Use 'Option.None<{1}>()' if you mean the absent case.",
            "Option.Some rejects a value equal to default(T). That changes in v6, where only null is rejected and the default of a value type becomes an ordinary Some, so code relying on the current behaviour changes meaning on upgrade rather than failing to compile.");

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

    public static readonly DiagnosticDescriptor RenamedToAndThen = Idiom(
        "WM2014",
        "Prefer AndThen over the obsolete FlatMap",
        "'{0}' is obsolete and will be removed in v6. Use '{1}'.",
        "Rust names this operation and_then, and Result already spelled it AndThen. FlatMap remains only as a forwarding member until the next major version.");

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
        "'{0}' hands back the default of '{1}' when there is no value, which is indistinguishable from a real one. '{2}' returns null instead.",
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
