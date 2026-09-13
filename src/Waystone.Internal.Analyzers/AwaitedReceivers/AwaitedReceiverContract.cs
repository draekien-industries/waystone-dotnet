namespace Waystone.Internal.Analyzers.AwaitedReceivers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// The part of the awaited receivers generator's contract this rule has to know:
/// which classes it reads, which of their members it emits a shape for, and which
/// parameter attributes reach that shape.
/// </summary>
/// <remarks>
/// Held by name rather than through a reference, because the generator is loaded as an
/// analyzer and is on nobody's compile line. The carried set has to stay in step with
/// <c>CallerInfo</c> over there: teach one to carry a fifth attribute without the
/// other and this reports one that is in fact carried.
/// </remarks>
internal sealed class AwaitedReceiverContract
{
    private const string ReceiversAttribute =
        "Waystone.Internal.SourceGenerators.GenerateAwaitedReceiversAttribute";

    private const string ExcludeAttribute =
        "Waystone.Internal.SourceGenerators.ExcludeFromAwaitedReceiversAttribute";

    private static readonly ImmutableHashSet<string> Carried =
        ImmutableHashSet.Create(
            "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute",
            "System.Runtime.CompilerServices.CallerMemberNameAttribute",
            "System.Runtime.CompilerServices.CallerFilePathAttribute",
            "System.Runtime.CompilerServices.CallerLineNumberAttribute");

    /// <summary>
    /// Names a type without its type arguments, so <c>Task&lt;Option&lt;T&gt;&gt;</c>
    /// and <c>Task&lt;int&gt;</c> both read as <c>System.Threading.Tasks.Task</c>.
    /// </summary>
    private static readonly SymbolDisplayFormat TypeName =
        SymbolDisplayFormat.FullyQualifiedFormat
                           .WithGlobalNamespaceStyle(
                                SymbolDisplayGlobalNamespaceStyle.Omitted)
                           .WithGenericsOptions(SymbolDisplayGenericsOptions.None);

    /// <summary>
    /// Whether <paramref name="compilation" /> uses the generator at all.
    /// </summary>
    /// <remarks>
    /// The marker attribute is injected by the generator, so a project that does not
    /// import its props has no such type and this is false. That is what keeps the rule
    /// silent in a compilation with nothing for it to act on rather than noisy.
    /// </remarks>
    public static bool IsInUse(Compilation compilation) =>
        compilation.GetTypeByMetadataName(ReceiversAttribute) is not null;

    /// <summary>
    /// Whether <paramref name="type" /> is a class the generator reads.
    /// </summary>
    public static bool IsMarked(INamedTypeSymbol type) =>
        HasAttribute(type, ReceiversAttribute);

    /// <summary>
    /// The members of <paramref name="type" /> the generator emits an awaited shape
    /// for, and therefore the ones an attribute it cannot carry costs something on.
    /// </summary>
    /// <remarks>
    /// Reads the marked class's own members rather than walking up from each method,
    /// which is both what the generator does and the only thing that works. A C# 14
    /// <c>extension</c> block member is declared inside a compiler-generated container,
    /// so its containing type is that container and not the marked class, and it does
    /// not report itself as an extension method either — it is
    /// <c>GetMembers</c> on the marked class that hands back the
    /// compatibility static form, which does.
    /// <para>
    /// A member the generator skips — one that is not a public extension, one already
    /// on an awaited receiver, one carrying <c>[ExcludeFromAwaitedReceivers]</c> — has
    /// no generated counterpart, so nothing is dropped and nothing is reported.
    /// </para>
    /// </remarks>
    public static IEnumerable<IMethodSymbol> Lifted(INamedTypeSymbol type) =>
        type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(
                 method => method.IsExtensionMethod
                        && method.DeclaredAccessibility == Accessibility.Public
                        && method.Parameters.Length > 0
                        && !IsAwaitable(method.Parameters[0].Type)
                        && !HasAttribute(method, ExcludeAttribute));

    /// <summary>
    /// Whether <paramref name="attribute" /> reaches the generated parameter.
    /// </summary>
    public static bool Carries(AttributeData attribute) =>
        attribute.AttributeClass is { } attributeClass
     && Carried.Contains(attributeClass.ToDisplayString());

    private static bool IsAwaitable(ITypeSymbol type) =>
        type is INamedTypeSymbol named
     && named.OriginalDefinition.ToDisplayString(TypeName)
            is "System.Threading.Tasks.Task"
            or "System.Threading.Tasks.ValueTask";

    private static bool HasAttribute(ISymbol symbol, string metadataName) =>
        symbol.GetAttributes()
              .Any(
                   attribute => attribute.AttributeClass?.ToDisplayString()
                             == metadataName);
}
