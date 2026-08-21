namespace Waystone.SourceGenerators.AwaitedReceivers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

internal sealed record AwaitedMember(
    IMethodSymbol Source,
    ITypeSymbol ReceiverType,
    string ReceiverParameterName,
    ImmutableArray<ITypeParameterSymbol> BlockTypeParameters,
    ImmutableArray<ITypeParameterSymbol> MemberTypeParameters,
    ImmutableArray<IParameterSymbol> Parameters,
    string? SummaryOverride = null);

internal sealed record GenerationResult(
    string HintName,
    string? Source,
    EquatableArray<DiagnosticInfo> Diagnostics);
