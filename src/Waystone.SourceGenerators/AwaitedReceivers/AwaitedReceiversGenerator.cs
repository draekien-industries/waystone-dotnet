namespace Waystone.SourceGenerators.AwaitedReceivers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Generates the <c>Task&lt;TReceiver&gt;</c> and <c>ValueTask&lt;TReceiver&gt;</c>
/// extension blocks for a static class marked with
/// <c>[GenerateAwaitedReceivers]</c>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class AwaitedReceiversGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(
            static ctx => ctx.AddSource(
                GeneratedAttributes.HintName,
                SourceText.From(GeneratedAttributes.Source, Encoding.UTF8)));

        IncrementalValuesProvider<GenerationResult> results =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                        GeneratedAttributes.ReceiversAttributeMetadataName,
                        static (node, _) => node is ClassDeclarationSyntax,
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

    private static GenerationResult? Analyse(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol target) return null;

        var declaration = (ClassDeclarationSyntax)context.TargetNode;
        var diagnostics = new List<DiagnosticInfo>();

        if (!declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            diagnostics.Add(
                DiagnosticInfo.Create(
                    Rules.MustBePartial,
                    declaration.Identifier.GetLocation(),
                    target.Name));

            return new GenerationResult(
                HintNameFor(target),
                null,
                new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
        }

        AttributeData receivers = context.Attributes[0];

        if (receivers.ConstructorArguments.Length != 1
         || receivers.ConstructorArguments[0].Value is not INamedTypeSymbol receiver)
        {
            return null;
        }

        receiver = receiver.OriginalDefinition;

        var members = new List<AwaitedMember>();

        members.AddRange(FromExtensionBlocks(target));

        foreach (AttributeData attribute in target.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString()
             != GeneratedAttributes.MemberAttributeMetadataName)
            {
                continue;
            }

            members.AddRange(
                FromReceiverMember(
                    receiver,
                    attribute,
                    declaration,
                    diagnostics));
        }

        string? source = members.Count == 0
            ? null
            : AwaitedReceiverWriter.Emit(
                target,
                members,
                context.SemanticModel.Compilation);

        return new GenerationResult(
            HintNameFor(target),
            source,
            new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
    }

    private static IEnumerable<AwaitedMember> FromExtensionBlocks(INamedTypeSymbol target)
    {
        foreach (IMethodSymbol method in target.GetMembers().OfType<IMethodSymbol>())
        {
            if (!method.IsExtensionMethod
             || method.DeclaredAccessibility != Accessibility.Public
             || method.Parameters.Length == 0)
            {
                continue;
            }

            ITypeSymbol receiverType = method.Parameters[0].Type;

            if (Awaitables.IsAwaitable(receiverType)) continue;

            ImmutableArray<ITypeParameterSymbol> blockTypeParameters =
                TypeParameters.ReferencedBy(receiverType, method.TypeParameters);

            yield return new AwaitedMember(
                method,
                receiverType,
                method.Parameters[0].Name,
                blockTypeParameters,
                method.TypeParameters.Where(p => !blockTypeParameters.Contains(p))
                      .ToImmutableArray(),
                method.Parameters.RemoveAt(0));
        }
    }

    private static IEnumerable<AwaitedMember> FromReceiverMember(
        INamedTypeSymbol receiver,
        AttributeData member,
        ClassDeclarationSyntax declaration,
        List<DiagnosticInfo> diagnostics)
    {
        if (member.ConstructorArguments.Length != 1
         || member.ConstructorArguments[0].Value is not string memberName)
        {
            yield break;
        }

        string? summary = member.NamedArguments
                              .FirstOrDefault(
                                   argument => argument.Key == "Summary")
                              .Value.Value as string;

        string receiverParameterName = Identifiers.CamelCase(receiver.Name);
        var matched = false;

        foreach (IMethodSymbol method in receiver.GetMembers(memberName)
                                                 .OfType<IMethodSymbol>())
        {
            if (method.DeclaredAccessibility != Accessibility.Public
             || method.IsStatic
             || method.MethodKind != MethodKind.Ordinary)
            {
                continue;
            }

            matched = true;

            yield return new AwaitedMember(
                method,
                receiver,
                receiverParameterName,
                receiver.TypeParameters,
                method.TypeParameters,
                method.Parameters,
                summary);
        }

        if (matched) yield break;

        diagnostics.Add(
            DiagnosticInfo.Create(
                Rules.UnknownMember,
                declaration.Identifier.GetLocation(),
                receiver.Name,
                memberName));
    }

    private static string HintNameFor(INamedTypeSymbol target) =>
        $"{target.ContainingNamespace.ToDisplayString()}.{target.Name}.AwaitedReceivers.cs";
}
