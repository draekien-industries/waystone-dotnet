namespace Waystone.Internal.SourceGenerators.AwaitedReceivers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        if (Target(attribute) is not { } target) return string.Empty;

        string retargeted = target == sourceReceiver ? generatedReceiver : target;

        return $"[global::{name}(\"{retargeted}\")] ";
    }

    /// <summary>
    /// The parameter name a <c>CallerArgumentExpression</c> points at, or
    /// <see langword="null" /> where it points at nothing readable.
    /// </summary>
    /// <remarks>
    /// Falls back to the syntax, because the attribute is routinely an error type
    /// here and an error type has no bound constructor arguments. A generator cannot
    /// see another generator's output, so on a target framework where
    /// <c>CallerArgumentExpression</c> is polyfilled — every <c>netstandard2.0</c>
    /// project in this repository, through PolySharp — the attribute does not resolve
    /// in this view of the compilation even though it resolves perfectly well in the
    /// one the compiler finally emits.
    /// <para>
    /// The two spellings the fallback reads are the two the attribute is written
    /// with: <c>nameof(parameter)</c> and a string literal. Anything else yields
    /// null and the attribute is left off rather than guessed at.
    /// </para>
    /// <para>
    /// The application is read as an <c>AttributeSyntax</c> outright rather than
    /// tested for one. <paramref name="attribute" /> always sits on a parameter of
    /// the marked class, which is source in the compilation being generated into, so
    /// it has an application and that application is an attribute.
    /// </para>
    /// </remarks>
    private static string? Target(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 1
         && attribute.ConstructorArguments[0].Value is string bound)
        {
            return bound;
        }

        if ((AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax()
                is not { ArgumentList.Arguments: [{ Expression: var argument }] })
        {
            return null;
        }

        return argument switch
        {
            InvocationExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" },
                ArgumentList.Arguments: [{ Expression: IdentifierNameSyntax named }],
            } => named.Identifier.ValueText,
            LiteralExpressionSyntax { Token.Value: string literal } => literal,
            _ => null,
        };
    }

    private static string? Name(AttributeData attribute) =>
        attribute.AttributeClass?.ToDisplayString();
}
