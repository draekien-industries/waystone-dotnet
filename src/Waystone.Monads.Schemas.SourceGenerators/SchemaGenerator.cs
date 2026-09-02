namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Generates the shared <c>Instance</c> of every concrete schema that derives from
/// <c>SchemaConfig&lt;TIn, TOut&gt;</c>.
/// </summary>
/// <remarks>
/// Silent on a compilation with no such schema, and silent on an abstract one,
/// which exists to be derived from rather than used. For a concrete schema it emits
/// nothing and reports instead when the schema or a type containing it is not
/// <c>partial</c> (<c>WMSC0001</c>), when it has no parameterless constructor to call
/// (<c>WMSC0002</c>), or when it already declares a member named <c>Instance</c>
/// (<c>WMSC0003</c>).
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class SchemaGenerator : IIncrementalGenerator
{
    internal const string SchemaConfigMetadataName = "SchemaConfig`2";

    internal const string SchemaConfigNamespace = "Waystone.Monads.Schemas";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<GenerationResult> results =
            context.SyntaxProvider.CreateSyntaxProvider(
                        static (node, _) => node is ClassDeclarationSyntax
                        {
                            BaseList: not null,
                        },
                        static (ctx, _) => Analyse(ctx))
                   .Where(static result => result is not null)
                   .Select(static (result, _) => result!);

        context.RegisterSourceOutput(
            results,
            static (ctx, result) =>
            {
                if (result.Diagnostic is not null)
                {
                    ctx.ReportDiagnostic(result.Diagnostic.ToDiagnostic());
                }

                if (result.Source is not null)
                {
                    ctx.AddSource(
                        result.HintName,
                        SourceText.From(result.Source, Encoding.UTF8));
                }
            });
    }

    private static GenerationResult? Analyse(GeneratorSyntaxContext context)
    {
        var declaration = (ClassDeclarationSyntax)context.Node;

        var schema =
            context.SemanticModel.GetDeclaredSymbol(declaration) as
                INamedTypeSymbol;

        if (schema is null || !DerivesFromSchemaConfig(schema)) return null;

        if (schema.IsAbstract) return null;

        if (!IsFirstDeclaration(schema, declaration)) return null;

        IReadOnlyList<INamedTypeSymbol> containers = Symbols.Containers(schema);

        string hintName = HintNameFor(schema, containers);
        Location location = declaration.Identifier.GetLocation();

        INamedTypeSymbol? notPartial = FirstNonPartial(schema, containers);

        if (notPartial is not null)
        {
            return GenerationResult.Failed(
                hintName,
                DiagnosticInfo.Create(
                    Rules.NotPartial,
                    notPartial.Locations[0],
                    schema.Name,
                    notPartial.Name));
        }

        if (schema.GetMembers(SchemaInstanceWriter.InstanceMember).Length > 0)
        {
            return GenerationResult.Failed(
                hintName,
                DiagnosticInfo.Create(
                    Rules.InstanceAlreadyDeclared,
                    location,
                    schema.Name));
        }

        if (!schema.InstanceConstructors.Any(
                static constructor => constructor.Parameters.Length == 0))
        {
            return GenerationResult.Failed(
                hintName,
                DiagnosticInfo.Create(
                    Rules.NoParameterlessConstructor,
                    location,
                    schema.Name));
        }

        return GenerationResult.Emitted(
            hintName,
            SchemaInstanceWriter.Emit(schema, containers));
    }

    /// <summary>
    /// Whether the type derives from the runtime's field-set base class, resolved by
    /// metadata name rather than by referencing the package. A generator that
    /// referenced its own runtime would load a second copy of it into every
    /// consumer's compiler.
    /// </summary>
    /// <remarks>
    /// The arity-bearing metadata name is compared first because it is a comparison
    /// against a string the symbol already holds. Only a type that clears it pays for
    /// its namespace, which has to be rendered. This runs over every class with a base
    /// list in the consumer's compilation, most of which are nothing to do with
    /// schemas.
    /// </remarks>
    private static bool DerivesFromSchemaConfig(INamedTypeSymbol schema)
    {
        for (INamedTypeSymbol? current = schema.BaseType;
             current is not null;
             current = current.BaseType)
        {
            if (IsSchemaConfig(current.OriginalDefinition)) return true;
        }

        return false;
    }

    private static bool IsSchemaConfig(INamedTypeSymbol type) =>
        type.MetadataName == SchemaConfigMetadataName
     && !type.ContainingNamespace.IsGlobalNamespace
     && type.ContainingNamespace.ToDisplayString() == SchemaConfigNamespace;

    /// <summary>
    /// The outermost type in the nesting chain that is missing the <c>partial</c>
    /// modifier, or null when every one of them has it. Outermost first, because
    /// that is the declaration a reader has to reach before the inner ones can
    /// reopen at all.
    /// </summary>
    private static INamedTypeSymbol? FirstNonPartial(
        INamedTypeSymbol schema,
        IReadOnlyList<INamedTypeSymbol> containers)
    {
        foreach (INamedTypeSymbol container in containers)
        {
            if (!IsPartial(container)) return container;
        }

        return IsPartial(schema) ? null : schema;
    }

    private static bool IsPartial(INamedTypeSymbol type) =>
        type.DeclaringSyntaxReferences.Any(
            static reference => reference.GetSyntax() is TypeDeclarationSyntax
                                    syntax
                             && syntax.Modifiers.Any(
                                    static modifier => modifier.Text
                                                    == "partial"));

    /// <summary>
    /// Whether this declaration is the one the generator should act on. A partial
    /// class reaches the pipeline once per part that names a base list, and emitting
    /// from each would add the same hint name twice.
    /// </summary>
    /// <remarks>
    /// The comparison is against the first part carrying a base list rather than the
    /// first part at all. A part that names no base type never reaches this method,
    /// so anchoring on it would mean nothing is generated for a schema whose base
    /// clause is written on a later part. The one-part shortcut keeps the common case
    /// off the syntax-materialising path entirely.
    /// </remarks>
    private static bool IsFirstDeclaration(
        INamedTypeSymbol schema,
        ClassDeclarationSyntax declaration)
    {
        if (schema.DeclaringSyntaxReferences.Length == 1) return true;

        ClassDeclarationSyntax first =
            schema.DeclaringSyntaxReferences
                  .Select(static reference => reference.GetSyntax())
                  .OfType<ClassDeclarationSyntax>()
                  .First(static part => part.BaseList is not null);

        return first.SyntaxTree == declaration.SyntaxTree
            && first.Span == declaration.Span;
    }

    private static string HintNameFor(
        INamedTypeSymbol schema,
        IReadOnlyList<INamedTypeSymbol> containers)
    {
        var name = new StringBuilder();

        if (!schema.ContainingNamespace.IsGlobalNamespace)
        {
            name.Append(schema.ContainingNamespace.ToDisplayString());
            name.Append('.');
        }

        foreach (INamedTypeSymbol container in containers)
        {
            name.Append(NameOf(container));
            name.Append('.');
        }

        name.Append(NameOf(schema));
        name.Append(".Instance.g.cs");

        return name.ToString();
    }

    /// <summary>
    /// The type's name with its arity appended after an underscore, so that
    /// <c>Wrapper</c> and <c>Wrapper&lt;T&gt;</c> in one namespace do not produce the
    /// same hint name. The metadata name spells the arity with a backtick, which is
    /// not a character to put in a file name.
    /// </summary>
    private static string NameOf(INamedTypeSymbol type) =>
        type.TypeParameters.Length == 0
            ? type.Name
            : type.Name + "_" + type.TypeParameters.Length;
}
