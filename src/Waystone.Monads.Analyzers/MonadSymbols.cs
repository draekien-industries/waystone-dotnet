namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

public sealed class MonadSymbols
{
    public const string OptionMetadataName = "Waystone.Monads.Options.Option`1";
    public const string SomeMetadataName = "Waystone.Monads.Options.Some`1";
    public const string NoneMetadataName = "Waystone.Monads.Options.None`1";
    public const string OptionFactoryMetadataName =
        "Waystone.Monads.Options.Option";
    public const string ResultMetadataName =
        "Waystone.Monads.Results.Result`2";
    public const string OkMetadataName = "Waystone.Monads.Results.Ok`2";
    public const string ErrMetadataName = "Waystone.Monads.Results.Err`2";
    public const string ResultFactoryMetadataName =
        "Waystone.Monads.Results.Result";
    public const string ErrorMetadataName =
        "Waystone.Monads.Results.Errors.Error";
    public const string ErrorCodeCatalogAttributeMetadataName =
        "Waystone.Monads.Results.Errors.ErrorCodeCatalogAttribute";
    public const string ErrorCodeFormatAttributeMetadataName =
        "Waystone.Monads.Results.Errors.ErrorCodeFormatAttribute";

    private MonadSymbols(
        INamedTypeSymbol option,
        INamedTypeSymbol some,
        INamedTypeSymbol none,
        INamedTypeSymbol optionFactory,
        INamedTypeSymbol result,
        INamedTypeSymbol ok,
        INamedTypeSymbol err,
        INamedTypeSymbol resultFactory,
        INamedTypeSymbol? error,
        INamedTypeSymbol? errorCodeCatalogAttribute,
        INamedTypeSymbol? task,
        INamedTypeSymbol? valueTask)
    {
        Option = option;
        Some = some;
        None = none;
        OptionFactory = optionFactory;
        Result = result;
        Ok = ok;
        Err = err;
        ResultFactory = resultFactory;
        Error = error;
        ErrorCodeCatalogAttribute = errorCodeCatalogAttribute;
        Task = task;
        ValueTask = valueTask;
    }

    public INamedTypeSymbol Option { get; }

    public INamedTypeSymbol Some { get; }

    public INamedTypeSymbol None { get; }

    public INamedTypeSymbol OptionFactory { get; }

    public INamedTypeSymbol Result { get; }

    public INamedTypeSymbol Ok { get; }

    public INamedTypeSymbol Err { get; }

    public INamedTypeSymbol ResultFactory { get; }

    public INamedTypeSymbol? Error { get; }

    public INamedTypeSymbol? ErrorCodeCatalogAttribute { get; }

    public INamedTypeSymbol? Task { get; }

    public INamedTypeSymbol? ValueTask { get; }

    public static MonadSymbols? TryCreate(Compilation compilation)
    {
        var option = compilation.GetTypeByMetadataName(OptionMetadataName);
        var some = compilation.GetTypeByMetadataName(SomeMetadataName);
        var none = compilation.GetTypeByMetadataName(NoneMetadataName);
        var optionFactory =
            compilation.GetTypeByMetadataName(OptionFactoryMetadataName);
        var result = compilation.GetTypeByMetadataName(ResultMetadataName);
        var ok = compilation.GetTypeByMetadataName(OkMetadataName);
        var err = compilation.GetTypeByMetadataName(ErrMetadataName);
        var resultFactory =
            compilation.GetTypeByMetadataName(ResultFactoryMetadataName);

        if (option is null
         || some is null
         || none is null
         || optionFactory is null
         || result is null
         || ok is null
         || err is null
         || resultFactory is null)
        {
            return null;
        }

        return new MonadSymbols(
            option,
            some,
            none,
            optionFactory,
            result,
            ok,
            err,
            resultFactory,
            compilation.GetTypeByMetadataName(ErrorMetadataName),
            compilation.GetTypeByMetadataName(
                ErrorCodeCatalogAttributeMetadataName),
            compilation.GetTypeByMetadataName(
                "System.Threading.Tasks.Task`1"),
            compilation.GetTypeByMetadataName(
                "System.Threading.Tasks.ValueTask`1"));
    }

    public bool IsOption(ITypeSymbol? type) =>
        IsConstructedFrom(type, Option)
     || IsConstructedFrom(type, Some)
     || IsConstructedFrom(type, None);

    public bool IsResult(ITypeSymbol? type) =>
        IsConstructedFrom(type, Result)
     || IsConstructedFrom(type, Ok)
     || IsConstructedFrom(type, Err);

    public bool IsMonad(ITypeSymbol? type) => IsOption(type) || IsResult(type);

    public bool IsDerivedCase(ITypeSymbol? type) =>
        IsConstructedFrom(type, Some)
     || IsConstructedFrom(type, None)
     || IsConstructedFrom(type, Ok)
     || IsConstructedFrom(type, Err);

    public INamedTypeSymbol? BaseCaseOf(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol named)
        {
            return null;
        }

        if (IsConstructedFrom(named, Some) || IsConstructedFrom(named, None))
        {
            return Option.Construct(named.TypeArguments[0]);
        }

        if (IsConstructedFrom(named, Ok) || IsConstructedFrom(named, Err))
        {
            return Result.Construct(
                named.TypeArguments[0],
                named.TypeArguments[1]);
        }

        return null;
    }

    public ITypeSymbol? UnwrapAwaitable(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol { IsGenericType: true } named)
        {
            return type;
        }

        var definition = named.OriginalDefinition;

        if (SymbolEqualityComparer.Default.Equals(definition, Task)
         || SymbolEqualityComparer.Default.Equals(definition, ValueTask))
        {
            return named.TypeArguments[0];
        }

        return type;
    }

    public ImmutableArray<ITypeSymbol> TypeArgumentsOf(ITypeSymbol? type)
    {
        if (type is INamedTypeSymbol named
         && (IsOption(named) || IsResult(named)))
        {
            return named.TypeArguments;
        }

        return ImmutableArray<ITypeSymbol>.Empty;
    }

    /// <summary>
    /// Checks whether a candidate recovered from a failed overload resolution is one
    /// of this library's, reading the receiver rather than asking the method what kind
    /// of member it is.
    /// </summary>
    /// <remarks>
    /// <see cref="IsMonadMethod(IMethodSymbol)" /> cannot answer this. It reaches an
    /// extension through <see cref="IMethodSymbol.IsExtensionMethod" />, which is
    /// false for a C# 14 <c>extension</c> block member on the Roslyn the tests run
    /// against and true on the one the analyzers build against, so a gate built on it
    /// goes quiet on the awaited receivers in exactly one of the two.
    /// <para>
    /// The two clauses are the two forms a call can take. <paramref name="receiver" />
    /// is the type before the dot, which is the only place the receiver survives on an
    /// <c>extension</c> block candidate — such a candidate carries no receiver
    /// parameter at all, and its containing type is the extension grouping type. The
    /// first parameter covers the compatibility static form, where there is no dot.
    /// </para>
    /// </remarks>
    public bool IsMonadCandidate(IMethodSymbol method, ITypeSymbol? receiver) =>
        IsMonad(UnwrapAwaitable(receiver))
     || (method.Parameters.Length > 0
      && IsMonad(UnwrapAwaitable(method.Parameters[0].Type)));

    public bool IsMonadMethod(IMethodSymbol method)
    {
        if (IsMonad(method.ContainingType))
        {
            return true;
        }

        var declared = method.ReducedFrom ?? method;

        return declared.IsExtensionMethod
            && declared.Parameters.Length > 0
            && IsMonad(UnwrapAwaitable(declared.Parameters[0].Type));
    }

    public bool IsMonadInvocation(IInvocationOperation invocation)
    {
        if (IsMonadMethod(invocation.TargetMethod))
        {
            return true;
        }

        var receiver = Semantics.ReceiverOf(invocation);

        if (receiver?.Type is not null
         && IsMonad(UnwrapAwaitable(receiver.Type)))
        {
            return true;
        }

        if (invocation.Syntax is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax access,
            }
         || invocation.SemanticModel is null)
        {
            return false;
        }

        return IsMonad(
            UnwrapAwaitable(
                invocation.SemanticModel.GetTypeInfo(access.Expression).Type));
    }

    private static bool IsConstructedFrom(
        ITypeSymbol? type,
        INamedTypeSymbol definition) =>
        type is INamedTypeSymbol named
     && SymbolEqualityComparer.Default.Equals(
            named.OriginalDefinition,
            definition);
}
