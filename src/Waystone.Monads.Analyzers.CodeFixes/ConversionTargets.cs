namespace Waystone.Monads.Analyzers;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class ConversionTargets
{
    internal static IEnumerable<ITypeSymbol> Of(
        ExpressionSyntax expression,
        SemanticModel model)
    {
        if (model.GetTypeInfo(expression).ConvertedType is { } converted)
        {
            yield return converted;
        }

        if (expression.Parent is not ArgumentSyntax argument
         || argument.Parent is not ArgumentListSyntax arguments
         || arguments.Parent is not ExpressionSyntax call)
        {
            yield break;
        }

        int position = arguments.Arguments.IndexOf(argument);

        foreach (var method in model.GetSymbolInfo(call)
                    .CandidateSymbols
                    .OfType<IMethodSymbol>())
        {
            if (ParameterTypeAt(method, position, argument) is { } parameter)
            {
                yield return parameter;
            }
        }
    }

    private static ITypeSymbol? ParameterTypeAt(
        IMethodSymbol method,
        int position,
        ArgumentSyntax argument)
    {
        var named = argument.NameColon?.Name.Identifier.ValueText;

        var parameter = named is null
            ? position >= 0 && position < method.Parameters.Length
                ? method.Parameters[position]
                : method.Parameters.LastOrDefault(p => p.IsParams)
            : method.Parameters.FirstOrDefault(p => p.Name == named);

        return parameter switch
        {
            null => null,
            { IsParams: true, Type: IArrayTypeSymbol array } =>
                array.ElementType,
            _ => parameter.Type,
        };
    }
}
