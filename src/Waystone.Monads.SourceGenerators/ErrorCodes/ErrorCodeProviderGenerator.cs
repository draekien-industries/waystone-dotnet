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
/// marked with <c>[ErrorCodeProvider]</c>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ErrorCodeProviderGenerator : IIncrementalGenerator
{
    internal const string AttributeMetadataName =
        "Waystone.Monads.Results.Errors.ErrorCodeProviderAttribute";

    internal const string ErrorCodeMetadataName =
        "Waystone.Monads.Results.Errors.ErrorCode";

    internal const string ErrorMetadataName =
        "Waystone.Monads.Results.Errors.Error";

    private const string FlagsMetadataName = "System.FlagsAttribute";

    private static readonly string[] ReservedMemberNames =
    [
        ErrorCodeProviderWriter.ErrorCodeStringsClass,
        ErrorCodeProviderWriter.ErrorCodesClass,
        ErrorCodeProviderWriter.ErrorsClass,
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
        string providerName = ProviderNameFor(enumType.Name);
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
                    providerName));
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
                : ErrorCodeProviderWriter.Emit(enumType, providerName, members),
            new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
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
    /// The generated class is named after the enum with a trailing <c>Error</c> or
    /// <c>ErrorCode</c> deduplicated, so <c>OrderError</c> and <c>OrderErrorCode</c>
    /// both produce <c>OrderErrorProvider</c>.
    /// </summary>
    internal static string ProviderNameFor(string enumName)
    {
        foreach (string suffix in new[] { "ErrorCode", "Error" })
        {
            if (enumName.Length > suffix.Length
             && enumName.EndsWith(suffix, StringComparison.Ordinal))
            {
                return enumName.Substring(0, enumName.Length - suffix.Length)
                     + "ErrorProvider";
            }
        }

        return enumName + "ErrorProvider";
    }

    private static string HintNameFor(INamedTypeSymbol enumType) =>
        enumType.ContainingNamespace.IsGlobalNamespace
            ? $"{enumType.Name}.ErrorCodes.cs"
            : $"{enumType.ContainingNamespace.ToDisplayString()}.{enumType.Name}.ErrorCodes.cs";
}
