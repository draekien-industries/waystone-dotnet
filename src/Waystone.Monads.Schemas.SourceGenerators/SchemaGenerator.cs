namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Generates the members a schema declared as a set of fields cannot write for
/// itself: the shared <c>Instance</c>, and the <c>Schema.Fields</c> ladder at
/// every arity its <c>Configure</c> body uses.
/// </summary>
/// <remarks>
/// Silent on a compilation with no schema deriving from
/// <c>SchemaConfig&lt;TIn, TOut&gt;</c>, and silent on an abstract one, which
/// exists to be derived from rather than used. For a concrete schema it emits
/// nothing and reports instead when the schema or a type containing it is not
/// <c>partial</c> (<c>WMSC0001</c>), when it has no parameterless constructor to
/// call (<c>WMSC0002</c>), or when it already declares a member under one of the
/// names the generator writes (<c>WMSC0003</c>). It emits <i>and</i> reports when an
/// <c>Into</c> lambda does not match its field count (<c>WMSC0004</c>) or a
/// <c>Refine</c> argument yields a value nobody will see (<c>WMSC0005</c>).
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class SchemaGenerator : IIncrementalGenerator
{
    internal const string SchemaConfigMetadataName = "SchemaConfig`2";

    internal const string SchemaConfigNamespace = "Waystone.Monads.Schemas";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<bool> constrained =
            context.ParseOptionsProvider.Select(
                static (options, _) => Constrained(options));

        IncrementalValuesProvider<Analysis> analyses =
            context.SyntaxProvider.CreateSyntaxProvider(
                        static (node, _) => node is ClassDeclarationSyntax
                        {
                            BaseList: not null,
                        },
                        static (ctx, _) => Analyse(ctx))
                   .Where(static analysis => analysis is not null)
                   .Select(static (analysis, _) => analysis!);

        context.RegisterSourceOutput(
            analyses.Combine(constrained),
            static (ctx, pair) =>
            {
                Analysis analysis = pair.Left;

                foreach (DiagnosticInfo diagnostic in analysis.Diagnostics.Values)
                {
                    ctx.ReportDiagnostic(diagnostic.ToDiagnostic());
                }

                if (analysis.Model is null) return;

                ctx.AddSource(
                    analysis.HintName,
                    SourceText.From(
                        SchemaWriter.Emit(analysis.Model, pair.Right),
                        Encoding.UTF8));
            });
    }

    /// <summary>
    /// Whether the consumer's language version can spell the <c>notnull</c>
    /// constraint the emitted generics need.
    /// </summary>
    /// <remarks>
    /// Before C# 8 the word parses as a type name that does not exist, so emitting
    /// it is a build failure. Omitting it there costs nothing, because a compiler
    /// that cannot spell the constraint does not check nullability either. Read
    /// from the parse options rather than the compilation, which changes on every
    /// keystroke and would defeat the cache.
    /// </remarks>
    private static bool Constrained(ParseOptions options) =>
        options is not CSharpParseOptions csharp
     || csharp.LanguageVersion >= LanguageVersion.CSharp8;

    private static Analysis? Analyse(GeneratorSyntaxContext context)
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
            return Analysis.Failed(
                hintName,
                DiagnosticInfo.Create(
                    Rules.NotPartial,
                    notPartial.Locations[0],
                    schema.Name,
                    notPartial.Name));
        }

        if (Declares(schema, SchemaWriter.InstanceMember))
        {
            return Analysis.Failed(
                hintName,
                DiagnosticInfo.Create(
                    Rules.NameAlreadyDeclared,
                    location,
                    schema.Name,
                    SchemaWriter.InstanceMember));
        }

        if (!schema.InstanceConstructors.Any(
                static constructor => constructor.Parameters.Length == 0))
        {
            return Analysis.Failed(
                hintName,
                DiagnosticInfo.Create(
                    Rules.NoParameterlessConstructor,
                    location,
                    schema.Name));
        }

        var diagnostics = new List<DiagnosticInfo>();

        int[] arities =
            Ladder.Discover(schema, context.SemanticModel, diagnostics);

        string? taken = arities.Length == 0 ? null : LadderNameTaken(schema);

        if (taken is not null)
        {
            diagnostics.Add(
                DiagnosticInfo.Create(
                    Rules.NameAlreadyDeclared,
                    location,
                    schema.Name,
                    taken));

            return new Analysis(
                hintName,
                null,
                new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
        }

        return new Analysis(
            hintName,
            ModelOf(schema, containers, arities),
            new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
    }

    /// <summary>
    /// The first of the two names the ladder needs that the schema has already
    /// spent, or null where both are free.
    /// </summary>
    /// <remarks>
    /// Called only where a ladder is being emitted. Neither name is written into a
    /// schema that makes no <c>Schema.Fields</c> call, so reporting them there would
    /// fail a build over a collision that never happens.
    /// </remarks>
    private static string? LadderNameTaken(INamedTypeSymbol schema)
    {
        if (Declares(schema, SchemaWriter.EntryPointType))
        {
            return SchemaWriter.EntryPointType;
        }

        return Declares(schema, SchemaWriter.LadderType)
            ? SchemaWriter.LadderType
            : null;
    }

    /// <summary>
    /// Whether the schema already has a member of this name. Arity is not part of
    /// the question: a nested generic type collides with an existing member of the
    /// same name however many type parameters it takes.
    /// </summary>
    private static bool Declares(INamedTypeSymbol schema, string name) =>
        schema.GetMembers(name).Length > 0;

    private static SchemaModel ModelOf(
        INamedTypeSymbol schema,
        IReadOnlyList<INamedTypeSymbol> containers,
        int[] arities)
    {
        var declarations = new string[containers.Count];

        for (var index = 0; index < containers.Count; index++)
        {
            declarations[index] = Symbols.Declaration(containers[index]);
        }

        return new SchemaModel(
            schema.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : schema.ContainingNamespace.ToDisplayString(),
            new EquatableArray<string>(declarations),
            Symbols.Declaration(schema),
            schema.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            schema.Name,
            schema.DeclaredAccessibility == Accessibility.Public
                ? "public"
                : "internal",
            new EquatableArray<int>(arities));
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
        name.Append(".Schema.g.cs");

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
