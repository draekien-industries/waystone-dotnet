namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;

public static class Rules
{
    private const string Reliability = nameof(Reliability);
    private const string Usage = nameof(Usage);
    private const string Design = nameof(Design);

    /// <remarks>
    /// Reports the null case alone, because <c>Some</c> rejects null but
    /// accepts the default of a value type as the ordinary value it is. Keep
    /// the message about null rather than about <c>default(T)</c>.
    /// </remarks>
    public static readonly DiagnosticDescriptor SomeFromDefaultValue = Bug(
        "WM1001",
        "Do not pass null to Some",
        "'Option.Some' throws at runtime when the value is null. Use 'Option.None<{0}>()' to express the absence of a value.",
        "The constructor of Some rejects null, so this call always throws an ArgumentNullException.");

    /// <remarks>
    /// Warning survives the false-positive question. The worst case found is a
    /// null reaching a target the analyzer cannot prove is non-nullable, which
    /// happens in a tuple element, a collection-initializer element and a local
    /// function return, because
    /// <see cref="NullAndDefaultAnalyzer.TargetIsExplicitlyNullable" /> matches
    /// an enumerated set of positions and defaults to reporting. None of those
    /// is working code: reaching them requires declaring the monad as
    /// <c>Option&lt;T&gt;?</c>, which WM1008 forbids. WM1008 reports alongside
    /// this rule in every one of them but the tuple element, where
    /// <see cref="Semantics.IsDeclarationTypePosition" /> has no case and so
    /// never sees the declaration. That gap is WM1008's, not a reason to change
    /// this rule. Do not lower the tier to accommodate them, and do not widen
    /// the exclusion either — it would silence a real null.
    /// </remarks>
    public static readonly DiagnosticDescriptor NullAssignedToMonad = Bug(
        "WM1002",
        "Do not assign null to an Option or Result",
        "'{0}' is never null in correct use. Null here defeats the type and throws on the next member access.",
        "Option and Result are records, so the compiler permits null where one is expected. None and Err express absence and failure; null expresses neither.");

    /// <remarks>
    /// Warning survives the false-positive question on the same reasoning as
    /// WM1002: every position where the target's nullability cannot be proven
    /// requires an <c>Option&lt;T&gt;?</c> declaration, which WM1008 forbids —
    /// though it does not see the declaration in a tuple element, so there this
    /// rule reports alone, which is WM1008's gap rather than this rule's. The
    /// generic case that prompted the question is safe — <c>default</c> written
    /// for an unconstrained <c>T</c> has <c>T</c> as its type rather than the
    /// monad, so the rule stays quiet however <c>T</c> is instantiated.
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
    /// Exists because the break that produces the <c>Try</c> case is silent.
    /// Removing the dedicated async <c>Try</c> overload does not fail the call,
    /// it rebinds it to <c>Try&lt;T&gt;(Func&lt;T&gt;)</c> with <c>T</c>
    /// inferred as the task, which satisfies <c>notnull</c>. Only a call site
    /// that assigns to an explicitly typed local gets a compiler error, so the
    /// rule carries the whole migration for every other shape.
    /// The test is that the awaitable ends up <em>inside</em> the monad, not
    /// merely that a delegate returned one. <c>Match</c> and <c>MapOr</c> hand
    /// the task straight back, so the caller awaits it and nothing is lost;
    /// <c>Map</c> and <c>MapErr</c> trap it where it cannot be awaited without
    /// unwrapping first. A delegate parameter is required so that
    /// <c>Option.Some(FetchAsync())</c>, where the caller built the task
    /// deliberately, stays silent.
    /// No code fix: renaming to the <c>Async</c> sibling alone leaves the caller
    /// with an unawaited task, and where the await belongs is not something the
    /// fix can decide.
    /// </remarks>
    public static readonly DiagnosticDescriptor AsyncDelegatePassedToSyncMethod = Bug(
        "WM1011",
        "Do not pass an async delegate to a synchronous method",
        "'{0}' never awaits this delegate, so the task is trapped inside the '{1}' it returns. Use '{2}'.",
        "A synchronous method invokes its delegate and stores whatever comes back. Hand it one that returns a task and the monad holds the task rather than its result: the work has not finished, anything it throws is unobserved, and Try in particular catches nothing. The Async sibling awaits the delegate and holds the result.");

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

    /// <remarks>
    /// Fires on what is not provably free rather than on what is provably
    /// expensive, because only the first is decidable. A constant, a bare
    /// local, parameter, field or property read and a <c>default</c> cost
    /// nothing to evaluate twice, so the rule stays silent on all of them; a
    /// call, a <c>new</c> or an <c>await</c> might be cheap or might not, and
    /// no static test tells the two apart. The residual imprecision is a fire
    /// on a call that turns out to be cheap, where the caller pays a delegate
    /// allocation to avoid nothing; Info severity is what makes that
    /// affordable. A bare property read is skipped whatever the receiver,
    /// including one whose getter computes: auto-versus-computed is only
    /// decidable for a symbol declared in the current compilation, and a rule
    /// that fired on the metadata half would make two identical call sites
    /// behave differently on nothing but which assembly declared the property.
    /// </remarks>
    public static readonly DiagnosticDescriptor EagerArgumentNotFree = Idiom(
        "WM2016",
        "Prefer the lazy variant when the argument is not free",
        "'{0}' evaluates its argument even when '{1}' discards it. Use '{2}' if computing it is expensive or has a side effect.",
        "And, Or, UnwrapOr, MapOr and OkOr evaluate their argument before checking whether the receiver even needs it, so an expensive computation or a side effect runs unconditionally. Their AndThen, OrElse, UnwrapOrElse, MapOrElse and OkOrElse siblings take a delegate instead and only invoke it when the receiver's other branch is taken.");

    /// <remarks>
    /// Keyed on the capture rather than on the lambda: a lambda that captures
    /// nothing is already cached by the compiler into a static field, so the
    /// state overload would buy it nothing. Only a captured local or parameter
    /// forces a display class per call, and that is what the rule reports.
    /// Capturing <c>this</c> alone is excluded — it allocates a delegate rather
    /// than a display class, a smaller cost that would fire on most ordinary
    /// instance-method code and drown the signal.
    /// The overload set is discovered from the containing type rather than
    /// listed here. A hardcoded list would name an overload that does not
    /// exist: <c>ZipWith</c> and <c>Reduce</c> take a delegate and will never
    /// gain a state overload, their delegates already receiving every operand
    /// as an argument of the call. The lookup has paid for itself twice — two
    /// rounds of adding state overloads have moved the set without touching
    /// <see cref="StateOverloadAnalyzer" />.
    /// No code fix ships: the natural rewrite reuses the captured name as the
    /// new delegate parameter, which shadows the enclosing local and is CS0136
    /// before C# 8.
    /// </remarks>
    public static readonly DiagnosticDescriptor DelegateCapturesInsteadOfState = Idiom(
        "WM2017",
        "Prefer the state overload when the delegate captures",
        "The delegate passed to '{0}' captures '{1}', so a closure is allocated on every call. Pass the value to the '{0}' overload that takes state instead.",
        "Nearly every delegate-taking member of Option and Result has an overload that takes a state argument and hands it to the delegate. A lambda that captures a local or a parameter allocates a display class every time the call site runs; passing the value as state lets the delegate close over nothing, so the compiler caches it. Match is the most expensive of them to call with a closure, because its two branches share one display class but need a delegate each. Where more than one value is captured, pass them as a tuple.");

    /// <remarks>
    /// Keyed on the generated code string rather than on the enum name, because
    /// the name is only half of it: two enums called <c>OrderError</c> in
    /// different namespaces collide on the members they share and on nothing
    /// else, so reporting the type would name a pair that may generate no
    /// overlapping code at all. Reported on the second declaration in ordinal
    /// order rather than on both, so a concurrent compilation does not vary in
    /// which one it blames. There is no code fix: the fix is a rename, and which
    /// of the two enums should keep the code is not derivable from the source.
    /// </remarks>
    public static readonly DiagnosticDescriptor ErrorCodeReusedAcrossEnums = Idiom(
        "WM2018",
        "Do not reuse an error code across enums",
        "'{0}' and '{1}' both generate the error code '{2}', so the two errors are indistinguishable to anything reading the code",
        "An [ErrorCodeCatalog] enum generates one code per member from the enum's own name and the member's, so two enums sharing a name in different namespaces generate the same code for every member name they share. No two independent error taxonomies legitimately share a wire code, and consumers reading the code cannot tell which error occurred. Rename one of the enums or the colliding member.",
        WellKnownDiagnosticTags.CompilationEnd);

    /// <remarks>
    /// Reported on the enum member rather than on the registry, because the member is
    /// the thing a reader can act on and the registry line does not exist yet. Reported
    /// from a symbol action rather than from the compilation end action that WM2020
    /// uses, even though the two read the same set: a diagnostic reported at the end of
    /// a compilation is a non-local diagnostic, and neither Roslyn's code fix service
    /// nor the analyzer testing library offers a fix for one. Reporting from the end
    /// action would leave the fix unreachable.
    /// </remarks>
    public static readonly DiagnosticDescriptor ErrorCodeMissingFromRegistry = Idiom(
        "WM2019",
        "Add a generated error code to the registry",
        "'{0}' generates the error code '{1}', which '{2}' does not list",
        "A project with an ErrorCodes.txt has opted into reviewing its error codes as a committed list, the way PublicAPI.Shipped.txt makes the public API reviewable. A code missing from the list is a wire contract that reached consumers without anyone reading the diff. Invoke the code fix, then read the added line before committing it.");

    /// <remarks>
    /// Reported at the registry's own line, which is an external file location rather
    /// than a location in a syntax tree. That is what the public API analyzers do for
    /// RS0017 and it is the only honest place to point: nothing in the source
    /// corresponds to an entry no enum generates. Whether an entry is stale cannot be
    /// known until every enum has been seen, so this has to come from the compilation
    /// end action, and that makes it non-local and unfixable. The WM2019 fix rewrites
    /// the whole file including the removals, so the two travel together in practice;
    /// a project whose only divergence is a stale entry deletes the named line by hand.
    /// </remarks>
    public static readonly DiagnosticDescriptor StaleErrorCodeRegistryEntry = Idiom(
        "WM2020",
        "Remove an error code the project no longer generates",
        "'{1}' lists the error code '{0}', which no error code catalog in this project generates",
        "An entry left behind by a rename or a deletion claims a code the project no longer produces, so the list stops describing the project and the review it exists for stops being worth reading. Delete the line, or restore the member that generated it if the code was removed by mistake.",
        WellKnownDiagnosticTags.CompilationEnd);

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
