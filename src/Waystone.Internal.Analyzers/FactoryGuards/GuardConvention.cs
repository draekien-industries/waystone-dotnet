namespace Waystone.Internal.Analyzers.FactoryGuards;

using Microsoft.CodeAnalysis;

/// <summary>
/// The shape a method has to have to count as a guard.
/// </summary>
/// <remarks>
/// Recognised by shape rather than by an attribute, because the signature already
/// says everything an attribute would: a guard returns what it was given, so the
/// type it guards is the one it takes and hands back, and a method that does that
/// under one of these names is doing nothing else.
/// <para>
/// The shape is deliberately narrow. A method merely named <c>NotNull</c> does not
/// qualify — it has to be static, take the value and the name of whatever produced
/// it, and return that value's own type. Widening any of those would start
/// admitting unrelated helpers, and every type admitted is a type the rule then
/// demands be guarded everywhere.
/// </para>
/// <para>
/// Renaming a guard therefore un-registers it, and nothing here would notice. That
/// is what <c>GuardConventionTests</c> in <c>Waystone.Monads.Tests</c> is for: it
/// asserts the four guards still match, so the rename fails a test rather than
/// quietly switching the rule off.
/// </para>
/// </remarks>
internal static class GuardConvention
{
    public const string SynchronousName = "NotNull";

    public const string AsynchronousName = "NotNullAsync";

    public const string TaskMetadataName = "System.Threading.Tasks.Task`1";

    public const string ValueTaskMetadataName =
        "System.Threading.Tasks.ValueTask`1";

    public static bool NamesAGuard(string name) =>
        name == SynchronousName || name == AsynchronousName;

    public static bool TakesAValueAndItsSource(IMethodSymbol method) =>
        method.IsStatic
     && method.Parameters.Length == 2
     && method.Parameters[1].Type.SpecialType == SpecialType.System_String;
}
