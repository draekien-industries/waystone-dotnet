namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Reads the <c>Schema.Fields</c> calls a schema makes and reports what is wrong
/// with them.
/// </summary>
/// <remarks>
/// The arity is found syntactically, and it has to be: <c>Schema.Fields</c> is the
/// member being generated, so it binds to nothing while the generator is deciding
/// whether to generate it. Everything else about the chain — what a <c>Refine</c>
/// argument actually yields — binds normally, because those members already exist.
/// <para>
/// It also drives <c>Asynchrony</c> and <c>FieldNames</c>, neither of which has
/// anything to do with the ladder. Both need the same walk over the same
/// declarations, and walking them three times to keep them apart would cost a
/// consumer's build more than the tidier shape is worth.
/// </para>
/// </remarks>
internal static class Ladder
{
    private const string SchemaReceiver = "Schema";

    private const string IntoMember = "Into";

    private const string RefineMember = "Refine";

    private const string FieldMetadataName = "Field`1";

    private const string CheckedName = "Checked";

    public static int[] Discover(
        INamedTypeSymbol schema,
        SemanticModel current,
        List<DiagnosticInfo> diagnostics)
    {
        var arities = new SortedSet<int>();

        foreach (SyntaxReference reference in schema.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not ClassDeclarationSyntax part) continue;

            SemanticModel model = ModelFor(part, current);

            foreach (InvocationExpressionSyntax invocation in
                     part.DescendantNodes()
                         .OfType<InvocationExpressionSyntax>())
            {
                Asynchrony.Check(
                    invocation,
                    model,
                    schema.Name,
                    diagnostics);

                FieldNames.Check(
                    invocation,
                    model,
                    schema.Name,
                    diagnostics);

                if (!IsFieldsCall(invocation))
                {
                    CheckSpelling(
                        invocation,
                        model,
                        schema.Name,
                        diagnostics);

                    continue;
                }

                int arity = invocation.ArgumentList.Arguments.Count;

                if (arity == 0) continue;

                arities.Add(arity);

                CheckInto(invocation, arity, schema.Name, diagnostics);
                CheckRefine(invocation, model, diagnostics);
            }
        }

        var found = new int[arities.Count];

        arities.CopyTo(found);

        return found;
    }

    /// <summary>
    /// A partial schema may declare <c>Configure</c> in a file other than the one
    /// carrying its base clause, and the context supplies a model for that one file
    /// alone. Asking the compilation for another is not free, so the common case —
    /// a schema written in a single file — never does.
    /// </summary>
    private static SemanticModel ModelFor(
        ClassDeclarationSyntax part,
        SemanticModel current) =>
        part.SyntaxTree == current.SyntaxTree
            ? current
            : current.Compilation.GetSemanticModel(part.SyntaxTree);

    private static bool IsFieldsCall(InvocationExpressionSyntax invocation) =>
        invocation.Expression is MemberAccessExpressionSyntax access
     && access.Name.Identifier.ValueText == SchemaWriter.FieldsMember
     && NameOf(access.Expression) == SchemaReceiver;

    /// <summary>
    /// Reports a call that looks like a field set and was not recognised as one, so
    /// that the receiver having to be spelled <c>Schema</c> is said somewhere rather
    /// than only implied by a member that never appeared.
    /// </summary>
    /// <remarks>
    /// The unbound test is what keeps this off a consumer's own <c>Fields</c> method.
    /// A call that binds is somebody else's and no business of this generator; one
    /// that binds badly — the right member with the wrong arguments — is the
    /// compiler's to explain and comes back through <c>CandidateSymbols</c>. What is
    /// left is a name that resolved to nothing, which for a member named
    /// <c>Fields</c> inside a schema is nearly always this mistake.
    /// </remarks>
    private static void CheckSpelling(
        InvocationExpressionSyntax invocation,
        SemanticModel model,
        string schemaName,
        List<DiagnosticInfo> diagnostics)
    {
        if (NameOf(invocation.Expression) != SchemaWriter.FieldsMember) return;

        SymbolInfo bound = model.GetSymbolInfo(invocation);

        if (bound.Symbol is not null || bound.CandidateSymbols.Length > 0)
        {
            return;
        }

        diagnostics.Add(
            DiagnosticInfo.Create(
                Rules.FieldsNotRecognised,
                invocation.Expression.GetLocation(),
                schemaName,
                invocation.Expression.ToString()));
    }

    /// <summary>
    /// The right-most simple name of an expression, which for a member access is the
    /// member and for a qualified name is its last segment. Null where the expression
    /// ends in something that is not a name at all, such as an indexer or a call.
    /// </summary>
    private static string? NameOf(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case SimpleNameSyntax simple:
                return simple.Identifier.ValueText;
            case MemberAccessExpressionSyntax access:
                return access.Name.Identifier.ValueText;
            default:
                return null;
        }
    }

    private static void CheckInto(
        InvocationExpressionSyntax fields,
        int arity,
        string schemaName,
        List<DiagnosticInfo> diagnostics)
    {
        InvocationExpressionSyntax? into = Chained(fields, IntoMember);

        if (into is null || into.ArgumentList.Arguments.Count == 0) return;

        ExpressionSyntax argument = into.ArgumentList.Arguments[0].Expression;

        int? taken = ParameterCountOf(argument);

        if (taken is null || taken == arity) return;

        diagnostics.Add(
            DiagnosticInfo.Create(
                Rules.IntoArityMismatch,
                argument.GetLocation(),
                schemaName,
                arity.ToString(),
                taken.Value.ToString()));
    }

    private static int? ParameterCountOf(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case SimpleLambdaExpressionSyntax:
                return 1;
            case ParenthesizedLambdaExpressionSyntax parenthesised:
                return parenthesised.ParameterList.Parameters.Count;
            case AnonymousMethodExpressionSyntax anonymous:
                return anonymous.ParameterList?.Parameters.Count;
            default:
                return null;
        }
    }

    private static void CheckRefine(
        InvocationExpressionSyntax fields,
        SemanticModel model,
        List<DiagnosticInfo> diagnostics)
    {
        InvocationExpressionSyntax? refine = Chained(fields, RefineMember);

        if (refine is null) return;

        foreach (ArgumentSyntax argument in refine.ArgumentList.Arguments)
        {
            ITypeSymbol? yielded = YieldedBy(argument.Expression, model);

            if (yielded is null) continue;

            diagnostics.Add(
                DiagnosticInfo.Create(
                    Rules.RefineDiscardsAValue,
                    argument.GetLocation(),
                    argument.Expression.ToString(),
                    yielded.ToDisplayString(
                        SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }
    }

    /// <summary>
    /// What a refinement argument contributes to the constructed value, or null
    /// where it contributes nothing and so belongs here.
    /// </summary>
    /// <remarks>
    /// Null covers three cases that all mean "leave it alone": the expression is a
    /// rule that only gates and yields <c>Checked</c>, it is the non-generic
    /// <c>Field</c> and has already erased its value, or it did not bind at all and
    /// the compiler has a better message than this one.
    /// </remarks>
    private static ITypeSymbol? YieldedBy(
        ExpressionSyntax expression,
        SemanticModel model)
    {
        if (model.GetTypeInfo(expression).Type is not INamedTypeSymbol type)
        {
            return null;
        }

        for (INamedTypeSymbol? current = type;
             current is not null;
             current = current.BaseType)
        {
            if (current.MetadataName != FieldMetadataName
             || !Symbols.IsSchemaNamespace(current.ContainingNamespace))
            {
                continue;
            }

            ITypeSymbol yielded = current.TypeArguments[0];

            return yielded.Name == CheckedName
                && Symbols.IsSchemaNamespace(yielded.ContainingNamespace)
                    ? null
                    : yielded;
        }

        return null;
    }

    /// <summary>
    /// The invocation of a named member further along the same fluent chain, or
    /// null where the chain does not reach one.
    /// </summary>
    public static InvocationExpressionSyntax? Chained(
        InvocationExpressionSyntax invocation,
        string member)
    {
        SyntaxNode node = invocation;

        while (node.Parent is MemberAccessExpressionSyntax access
            && access.Parent is InvocationExpressionSyntax next)
        {
            if (access.Name.Identifier.ValueText == member) return next;

            node = next;
        }

        return null;
    }
}
