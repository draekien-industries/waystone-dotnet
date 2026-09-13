namespace Waystone.Internal.SourceGenerators.AwaitedReceivers;

using Microsoft.CodeAnalysis;

/// <summary>
/// The parameter attributes a generated member carries over from the source parameter
/// it was read from.
/// </summary>
/// <remarks>
/// Only the caller-info four are carried, because they are the only ones whose meaning
/// survives the forward unchanged: the compiler fills them at the outer call site and
/// the generated member hands the value straight on. Anything else is dropped.
/// </remarks>
internal static class CallerInfo
{
    private const string ArgumentExpression =
        "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute";

    private const string MemberName =
        "System.Runtime.CompilerServices.CallerMemberNameAttribute";

    private const string FilePath =
        "System.Runtime.CompilerServices.CallerFilePathAttribute";

    private const string LineNumber =
        "System.Runtime.CompilerServices.CallerLineNumberAttribute";

    /// <summary>
    /// Whether <paramref name="attribute" /> reaches the generated parameter at all.
    /// </summary>
    public static bool IsCarried(AttributeData attribute) =>
        Name(attribute) is ArgumentExpression
                        or MemberName
                        or FilePath
                        or LineNumber;

    /// <summary>
    /// <paramref name="attribute" /> as it is written onto the generated parameter,
    /// trailing space included, or empty for one <see cref="IsCarried" /> rejects.
    /// </summary>
    /// <remarks>
    /// A <c>CallerArgumentExpression</c> naming <paramref name="sourceReceiver" /> is
    /// re-pointed at <paramref name="generatedReceiver" />, since the receiver is the
    /// one parameter the generated member renames. One naming any other parameter is
    /// written through, because that parameter keeps its name.
    /// <para>
    /// The target is written as a string literal rather than a <c>nameof</c> so that a
    /// name matching nothing surfaces the same way it does on the source member — as
    /// the compiler's own warning about an attribute that will have no effect, not as a
    /// <c>CS0103</c> in generated source.
    /// </para>
    /// </remarks>
    public static string Render(
        AttributeData attribute,
        string sourceReceiver,
        string generatedReceiver)
    {
        string? name = Name(attribute);

        if (name != ArgumentExpression)
        {
            return IsCarried(attribute) ? $"[global::{name}] " : string.Empty;
        }

        if (attribute.ConstructorArguments.Length != 1
         || attribute.ConstructorArguments[0].Value is not string target)
        {
            return string.Empty;
        }

        string retargeted = target == sourceReceiver ? generatedReceiver : target;

        return $"[global::{name}(\"{retargeted}\")] ";
    }

    private static string? Name(AttributeData attribute) =>
        attribute.AttributeClass?.ToDisplayString();
}
