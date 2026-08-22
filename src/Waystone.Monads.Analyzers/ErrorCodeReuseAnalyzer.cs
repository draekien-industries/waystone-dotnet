namespace Waystone.Monads.Analyzers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Waystone.Monads.SourceGenerators.ErrorCodes;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorCodeReuseAnalyzer : MonadAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.ErrorCodeReusedAcrossEnums);

    protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols)
    {
        if (symbols.ErrorCodeProviderAttribute is null) return;

        var providers = new ConcurrentBag<Provided>();

        string? assemblyFormat = AssemblyFormat(context.Compilation);

        context.RegisterSymbolAction(
            symbol => Collect(
                (INamedTypeSymbol)symbol.Symbol,
                symbols,
                assemblyFormat,
                providers),
            SymbolKind.NamedType);

        context.RegisterCompilationEndAction(end => Report(end, providers));
    }

    private static void Collect(
        INamedTypeSymbol type,
        MonadSymbols symbols,
        string? assemblyFormat,
        ConcurrentBag<Provided> providers)
    {
        if (type.TypeKind != TypeKind.Enum) return;

        AttributeData? provider = type.GetAttributes()
           .FirstOrDefault(
                attribute => SymbolEqualityComparer.Default.Equals(
                    attribute.AttributeClass,
                    symbols.ErrorCodeProviderAttribute));

        if (provider is null) return;

        if (!ErrorCodeFormat.TryParse(
                DeclaredFormat(provider) ?? assemblyFormat ?? ErrorCodeFormat.Default,
                out ErrorCodeFormat? format,
                out _))
        {
            return;
        }

        foreach (IFieldSymbol member in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (!member.IsConst) continue;

            providers.Add(
                new Provided(member, format!.Apply(type.Name, member.Name)));
        }
    }

    /// <summary>
    /// The format the enum declares, resolved the same way the generator resolves
    /// it.
    /// </summary>
    /// <remarks>
    /// The rule keys on the generated code rather than on the enum name, so the
    /// format has to be applied here too. Without it the rule is wrong in both
    /// directions once anyone sets a format: two enums with different names and one
    /// shared format collide and would go unreported, and two enums sharing a name
    /// with different formats do not collide and would be reported anyway.
    /// <c>ErrorCodeFormat</c> is compiled into this assembly from the generator's
    /// copy rather than duplicated, so the two can only ever agree.
    /// </remarks>
    private static string? DeclaredFormat(AttributeData provider)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in
                 provider.NamedArguments)
        {
            if (argument.Key == "Format" && argument.Value.Value is string declared)
            {
                return declared;
            }
        }

        return null;
    }

    private static string? AssemblyFormat(Compilation compilation)
    {
        foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString()
             != MonadSymbols.ErrorCodeFormatAttributeMetadataName)
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 1
             && attribute.ConstructorArguments[0].Value is string format)
            {
                return format;
            }
        }

        return null;
    }

    private static void Report(
        CompilationAnalysisContext context,
        ConcurrentBag<Provided> providers)
    {
        foreach (IGrouping<string, Provided> collision in providers
                    .GroupBy(provided => provided.Code, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1))
        {
            List<Provided> ordered = collision
                                    .OrderBy(
                                         provided =>
                                             provided.Member.ToDisplayString(),
                                         StringComparer.Ordinal)
                                    .ToList();

            Provided first = ordered[0];

            foreach (Provided later in ordered.Skip(1))
            {
                if (SymbolEqualityComparer.Default.Equals(
                        first.Member.ContainingType,
                        later.Member.ContainingType))
                {
                    continue;
                }

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Rules.ErrorCodeReusedAcrossEnums,
                        later.Member.Locations.FirstOrDefault(
                            location => location.IsInSource),
                        first.Member.ToDisplayString(),
                        later.Member.ToDisplayString(),
                        collision.Key));
            }
        }
    }

    private readonly struct Provided
    {
        public Provided(IFieldSymbol member, string code)
        {
            Member = member;
            Code = code;
        }

        public IFieldSymbol Member { get; }

        public string Code { get; }
    }
}
