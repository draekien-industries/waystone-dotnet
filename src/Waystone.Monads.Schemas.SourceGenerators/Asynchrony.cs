namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Finds asynchronous rules written where only the synchronous path can reach
/// them.
/// </summary>
/// <remarks>
/// Only a rule spelled out inside the schema is visible here. One held in a static
/// field, or built by a method in another assembly, looks like any other schema to
/// the compiler, which is what the runtime throw is for.
/// </remarks>
internal static class Asynchrony
{
    private const string CheckAsyncMember = "CheckAsync";

    private const string SchemaMetadataName = "Schema`2";

    /// <summary>
    /// Reports <c>WMSC0006</c> where the invocation is a <c>CheckAsync</c> call on
    /// a schema.
    /// </summary>
    /// <remarks>
    /// The name is matched first because it is a string already in the syntax, and
    /// almost no invocation in a schema is called this. Only then does the symbol
    /// get resolved, which is what rules out a <c>CheckAsync</c> belonging to
    /// somebody else.
    /// </remarks>
    public static void Check(
        InvocationExpressionSyntax invocation,
        SemanticModel model,
        string schemaName,
        List<DiagnosticInfo> diagnostics)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax
            {
                Name.Identifier.ValueText: CheckAsyncMember,
            } access)
        {
            return;
        }

        if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol
            {
                ContainingType: { } declaring,
            }
         || declaring.MetadataName != SchemaMetadataName
         || !Symbols.IsSchemaNamespace(declaring.ContainingNamespace))
        {
            return;
        }

        diagnostics.Add(
            DiagnosticInfo.Create(
                Rules.AsyncRuleInAFieldSet,
                access.Name.GetLocation(),
                schemaName));
    }
}
