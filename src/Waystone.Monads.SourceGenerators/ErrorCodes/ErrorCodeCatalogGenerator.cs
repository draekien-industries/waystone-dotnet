namespace Waystone.Monads.SourceGenerators.ErrorCodes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Generates the error code constants, error codes and error factories of an enum
/// marked with <c>[ErrorCodeCatalog]</c>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ErrorCodeCatalogGenerator : IIncrementalGenerator
{
    internal const string AttributeMetadataName =
        "Waystone.Monads.Results.Errors.ErrorCodeCatalogAttribute";

    internal const string ErrorCodeMetadataName =
        "Waystone.Monads.Results.Errors.ErrorCode";

    internal const string ErrorMetadataName =
        "Waystone.Monads.Results.Errors.Error";

    internal const string FormatAttributeMetadataName =
        "Waystone.Monads.Results.Errors.ErrorCodeFormatAttribute";

    private const string FlagsMetadataName = "System.FlagsAttribute";

    private static readonly string[] ReservedMemberNames =
    [
        ErrorCodeCatalogWriter.NamesClass,
        ErrorCodeCatalogWriter.CodesClass,
        ErrorCodeCatalogWriter.ErrorsClass,
    ];

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<GenerationResult> results =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                        AttributeMetadataName,
                        static (node, _) => node is EnumDeclarationSyntax,
                        static (ctx, _) => Analyse(ctx))
                   .Where(static result => result is not null)
                   .Select(static (result, _) => result!);

        context.RegisterSourceOutput(
            results,
            static (ctx, result) =>
            {
                foreach (DiagnosticInfo info in result.Diagnostics.Values)
                {
                    ctx.ReportDiagnostic(info.ToDiagnostic());
                }

                if (result.Source is not null)
                {
                    ctx.AddSource(
                        result.HintName,
                        SourceText.From(result.Source, Encoding.UTF8));
                }
            });
    }

    private static GenerationResult? Analyse(
        GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
        {
            return null;
        }

        Location location =
            ((EnumDeclarationSyntax)context.TargetNode).Identifier.GetLocation();

        string hintName = HintNameFor(enumType);
        string catalogName = CatalogNameFor(enumType.Name);
        Compilation compilation = context.SemanticModel.Compilation;

        string? missing = MissingErrorType(compilation);

        if (missing is not null)
        {
            return Failure(
                hintName,
                Rules.MissingErrorTypes,
                location,
                enumType.Name,
                missing);
        }

        if (enumType.GetAttributes()
                    .Any(
                         attribute => attribute.AttributeClass?.ToDisplayString()
                                   == FlagsMetadataName))
        {
            return Failure(
                hintName,
                Rules.FlagsEnum,
                location,
                enumType.Name);
        }

        string? requested = RequestedFormat(context.Attributes[0], compilation);

        if (!ErrorCodeFormat.TryParse(
                requested ?? ErrorCodeFormat.Default,
                out ErrorCodeFormat? format,
                out string? formatError))
        {
            return Failure(
                hintName,
                Rules.UnusableFormat,
                location,
                enumType.Name,
                formatError!);
        }

        if (!format!.UsesMember)
        {
            return Failure(
                hintName,
                Rules.FormatOmitsMember,
                location,
                enumType.Name);
        }

        List<IFieldSymbol> members = enumType.GetMembers()
                                             .OfType<IFieldSymbol>()
                                             .Where(
                                                  field => field.IsConst
                                                        && field.ConstantValue
                                                        is not null)
                                             .ToList();

        var diagnostics = new List<DiagnosticInfo>();

        foreach (IFieldSymbol member in members)
        {
            if (!ReservedMemberNames.Contains(member.Name)) continue;

            diagnostics.Add(
                DiagnosticInfo.Create(
                    Rules.ReservedMemberName,
                    LocationOf(member, location),
                    enumType.Name,
                    member.Name,
                    catalogName));
        }

        var seen = new Dictionary<string, IFieldSymbol>(StringComparer.Ordinal);

        foreach (IFieldSymbol member in members)
        {
            string value = member.ConstantValue!.ToString()!;

            if (seen.TryGetValue(value, out IFieldSymbol declared))
            {
                diagnostics.Add(
                    DiagnosticInfo.Create(
                        Rules.AliasedValue,
                        LocationOf(member, location),
                        declared.Name,
                        member.Name,
                        value));
            }
            else
            {
                seen.Add(value, member);
            }
        }

        return new GenerationResult(
            hintName,
            diagnostics.Count > 0
                ? null
                : ErrorCodeCatalogWriter.Emit(
                    enumType,
                    catalogName,
                    members,
                    format),
            new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
    }

    /// <summary>
    /// The format the enum asks for, then the one the assembly asks for, then null
    /// for the built-in default.
    /// </summary>
    private static string? RequestedFormat(
        AttributeData catalog,
        Compilation compilation)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in
                 catalog.NamedArguments)
        {
            if (argument.Key == "Format" && argument.Value.Value is string declared)
            {
                return declared;
            }
        }

        foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString()
             != FormatAttributeMetadataName)
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 1
             && attribute.ConstructorArguments[0].Value is string assemblyWide)
            {
                return assemblyWide;
            }
        }

        return null;
    }

    private static string? MissingErrorType(Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName(ErrorCodeMetadataName) is null)
        {
            return ErrorCodeMetadataName;
        }

        return compilation.GetTypeByMetadataName(ErrorMetadataName) is null
            ? ErrorMetadataName
            : null;
    }

    private static GenerationResult Failure(
        string hintName,
        DiagnosticDescriptor descriptor,
        Location location,
        params string[] messageArgs) =>
        new GenerationResult(
            hintName,
            null,
            new EquatableArray<DiagnosticInfo>(
                [DiagnosticInfo.Create(descriptor, location, messageArgs)]));

    private static Location LocationOf(ISymbol symbol, Location fallback) =>
        symbol.Locations.FirstOrDefault(location => location.IsInSource)
     ?? fallback;

    /// <summary>
    /// The generated class is the enum's own name with <c>Catalog</c> appended, so
    /// <c>OrderFailure</c> produces <c>OrderFailureCatalog</c> and
    /// <c>OrderError</c> produces <c>OrderErrorCatalog</c>.
    /// </summary>
    /// <remarks>
    /// Nothing is trimmed off the enum's name. An earlier version deduplicated a
    /// trailing <c>Error</c> or <c>ErrorCode</c>, which gave two different enums in
    /// one namespace — <c>OrderError</c> and <c>OrderErrorCode</c> — the same
    /// generated name and a collision the generator did not report.
    /// </remarks>
    internal static string CatalogNameFor(string enumName) => enumName + "Catalog";

    private static string HintNameFor(INamedTypeSymbol enumType) =>
        enumType.ContainingNamespace.IsGlobalNamespace
            ? $"{enumType.Name}.ErrorCodes.cs"
            : $"{enumType.ContainingNamespace.ToDisplayString()}.{enumType.Name}.ErrorCodes.cs";
}
