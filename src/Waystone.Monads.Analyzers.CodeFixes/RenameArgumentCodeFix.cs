namespace Waystone.Monads.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class RenameArgumentCodeFix : MonadCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("CS1739");

    protected override void Register(
        CodeFixContext context,
        Diagnostic diagnostic,
        SyntaxNode node,
        SemanticModel model,
        MonadSymbols symbols)
    {
        if (node.FirstAncestorOrSelf<ArgumentSyntax>() is not
            {
                NameColon: { } nameColon,
            } argument
         || argument.Parent is not ArgumentListSyntax list
         || list.Parent is not ExpressionSyntax call)
        {
            return;
        }

        foreach (var name in ReplacementNames(
                     call,
                     list,
                     argument,
                     model,
                     symbols))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    $"Rename argument to '{name}'",
                    token => ReplaceAsync(
                        context.Document,
                        nameColon.Name,
                        SyntaxFactory.IdentifierName(name),
                        token),
                    $"{nameof(RenameArgumentCodeFix)}.{name}"),
                diagnostic);
        }
    }

    private static IEnumerable<string> ReplacementNames(
        ExpressionSyntax call,
        ArgumentListSyntax list,
        ArgumentSyntax argument,
        SemanticModel model,
        MonadSymbols symbols)
    {
        int position = list.Arguments.IndexOf(argument);

        var spokenFor = list.Arguments
           .Where(other => other != argument)
           .Select(other => other.NameColon)
           .OfType<NameColonSyntax>()
           .Select(named => named.Name.Identifier.ValueText)
           .ToImmutableHashSet();

        var receiver = ReceiverTypeOf(call, model);
        var offered = new HashSet<string>();

        foreach (var candidate in model.GetSymbolInfo(call).CandidateSymbols)
        {
            if (candidate is not IMethodSymbol method
             || !IsOurs(method, receiver, symbols))
            {
                continue;
            }

            var parameters = ParametersNamedBy(method, list, receiver);

            if (position >= parameters.Length)
            {
                continue;
            }

            string name = parameters[position].Name;

            if (!spokenFor.Contains(name) && offered.Add(name))
            {
                yield return name;
            }
        }
    }

    /// <summary>
    /// The parameters an argument list can name, which excludes the receiver of a
    /// call written in reduced form.
    /// </summary>
    /// <remarks>
    /// Roslyn hands back the unreduced compatibility method for an <c>extension</c>
    /// block member, so its first parameter is the receiver and the argument list
    /// cannot name it. <see cref="IMethodSymbol.ReducedFrom" /> is null on that
    /// symbol and <see cref="IMethodSymbol.IsExtensionMethod" /> disagrees between
    /// Roslyn versions, so the arity gap is what identifies the shape.
    /// </remarks>
    private static ImmutableArray<IParameterSymbol> ParametersNamedBy(
        IMethodSymbol method,
        ArgumentListSyntax list,
        ITypeSymbol? receiver) =>
        receiver is not null
     && method.ReducedFrom is null
     && method.Parameters.Length == list.Arguments.Count + 1
            ? method.Parameters.RemoveAt(0)
            : method.Parameters;

    /// <summary>
    /// Checks whether the call is one of this library's, reading the receiver rather
    /// than asking the method what kind of member it is.
    /// </summary>
    /// <remarks>
    /// <see cref="MonadSymbols.IsMonadMethod" /> is deliberately not used here. It
    /// reaches an extension through <see cref="IMethodSymbol.IsExtensionMethod" />,
    /// which is false for an <c>extension</c> block member on the Roslyn the tests
    /// run against and true on the one the analyzer builds against, so a gate built
    /// on it goes quiet on the awaited receivers in exactly one of the two. The two
    /// clauses below cover what it would have covered: the type before the dot in
    /// reduced form, and the first parameter in the compatibility static form.
    /// </remarks>
    private static bool IsOurs(
        IMethodSymbol method,
        ITypeSymbol? receiver,
        MonadSymbols symbols) =>
        symbols.IsMonad(symbols.UnwrapAwaitable(receiver))
     || (method.Parameters.Length > 0
      && symbols.IsMonad(
             symbols.UnwrapAwaitable(method.Parameters[0].Type)));

    /// <summary>
    /// Gets the type of the expression before the dot, or null when the call names
    /// its receiver as an argument instead.
    /// </summary>
    /// <remarks>
    /// The symbol is what separates the two forms, not the type: <c>GetTypeInfo</c>
    /// answers with the type itself for a bare type name, so a static-form call would
    /// otherwise look like a reduced one whose receiver happens not to be a monad.
    /// </remarks>
    private static ITypeSymbol? ReceiverTypeOf(
        ExpressionSyntax call,
        SemanticModel model) =>
        MemberInvocationAt(call) is var (_, access)
     && model.GetSymbolInfo(access.Expression).Symbol is not (null
            or ITypeSymbol
            or INamespaceSymbol)
            ? model.GetTypeInfo(access.Expression).Type
            : null;
}
