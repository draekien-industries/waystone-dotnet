namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Finds fields whose reported path was taken from an expression that does not read
/// as a name.
/// </summary>
/// <remarks>
/// The runtime derives a field's path from
/// <c>CallerArgumentExpression</c> and keeps whatever follows the last dot, so the
/// check here is the same one, run at build time against the same text. Only a call
/// written out in the schema is visible; a field built by a helper somewhere else
/// carries a path this cannot see and the runtime cannot improve.
/// </remarks>
internal static class FieldNames
{
    private const string SchemaTypeName = "Schema";

    private const string NamedMember = "Named";

    private const string PathParameter = "valueExpression";

    private const string Fallback = "value";

    /// <summary>
    /// Reports <c>WMSC0008</c> where the invocation is a field factory whose first
    /// argument gives a path nobody would choose, and nothing renames it.
    /// </summary>
    /// <remarks>
    /// The order is deliberate and is all about cost: this runs over every
    /// invocation in every schema in a consumer's compilation. The member name is a
    /// string already in the syntax, the derived path is a substring of another, and
    /// only a call that fails both gets a symbol resolved.
    /// </remarks>
    public static void Check(
        InvocationExpressionSyntax invocation,
        SemanticModel model,
        string schemaName,
        List<DiagnosticInfo> diagnostics)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax access
         || !IsFactory(access.Name.Identifier.ValueText)
         || invocation.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        ExpressionSyntax value =
            invocation.ArgumentList.Arguments[0].Expression;

        string text = value.ToString();

        if (ReadsAsAName(text)) return;

        if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol
            {
                ContainingType: { } declaring,
            } method
         || declaring.Name != SchemaTypeName
         || !Symbols.IsSchemaNamespace(declaring.ContainingNamespace))
        {
            return;
        }

        if (SuppliesThePathItself(invocation, method)
         || Ladder.Chained(invocation, NamedMember) is not null)
        {
            return;
        }

        diagnostics.Add(
            DiagnosticInfo.Create(
                Rules.FieldPathNotDerivable,
                value.GetLocation(),
                schemaName,
                PathFrom(text)));
    }

    /// <summary>
    /// The three factories that take a path from their argument. <c>Extend</c> is
    /// absent because it reports at the subject's own path and has no segment to
    /// derive.
    /// </summary>
    private static bool IsFactory(string member)
    {
        switch (member)
        {
            case "Required":
            case "Optional":
            case "Forbidden":
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Whether the runtime would reduce this expression to something a caller can
    /// read. A member access reduces to the member; a call, an indexer, a literal or
    /// a null-forgiving operator keeps its punctuation and stops being a name.
    /// </summary>
    private static bool ReadsAsAName(string expression) =>
        SyntaxFacts.IsValidIdentifier(TailOf(expression));

    /// <summary>
    /// The path the runtime would report, so the message can show it rather than
    /// describe it. Mirrors <c>PathName.From</c>, which cannot be shared: the
    /// generator does not reference the runtime.
    /// </summary>
    private static string PathFrom(string expression)
    {
        string tail = TailOf(expression);

        if (tail.Length == 0) return Fallback;

        return char.IsUpper(tail[0])
            ? char.ToLowerInvariant(tail[0]) + tail.Substring(1)
            : tail;
    }

    private static string TailOf(string expression)
    {
        int dot = expression.LastIndexOf('.');

        return dot < 0 ? expression : expression.Substring(dot + 1);
    }

    /// <summary>
    /// Whether the call passes the path parameter by hand, in which case the
    /// argument text is not what the runtime reports and this rule has nothing to
    /// say about it.
    /// </summary>
    private static bool SuppliesThePathItself(
        InvocationExpressionSyntax invocation,
        IMethodSymbol method)
    {
        if (invocation.ArgumentList.Arguments.Count == method.Parameters.Length)
        {
            return true;
        }

        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            if (argument.NameColon?.Name.Identifier.ValueText == PathParameter)
            {
                return true;
            }
        }

        return false;
    }
}
