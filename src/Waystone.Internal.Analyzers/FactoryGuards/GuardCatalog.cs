namespace Waystone.Internal.Analyzers.FactoryGuards;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// The guards a compilation declares, indexed by the type each one guards.
/// </summary>
/// <remarks>
/// Built once per compilation by walking the assembly for methods matching
/// <see cref="GuardConvention" />. The guarded type is the guard's own return type
/// with <c>Task</c> or <c>ValueTask</c> unwrapped, so the synchronous and
/// asynchronous guards for one type land under the same key and the rule can name
/// whichever of them suits the delegate it is reporting.
/// </remarks>
internal sealed class GuardCatalog
{
    private readonly Dictionary<INamedTypeSymbol, GuardPair> _byGuardedType;
    private readonly HashSet<IMethodSymbol> _guards;
    private readonly INamedTypeSymbol? _task;
    private readonly INamedTypeSymbol? _valueTask;

    private GuardCatalog(
        INamedTypeSymbol? task,
        INamedTypeSymbol? valueTask)
    {
        _byGuardedType =
            new Dictionary<INamedTypeSymbol, GuardPair>(
                SymbolEqualityComparer.Default);

        _guards = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        _task = task;
        _valueTask = valueTask;
    }

    public bool IsEmpty => _byGuardedType.Count == 0;

    public static GuardCatalog Read(Compilation compilation)
    {
        var catalog = new GuardCatalog(
            compilation.GetTypeByMetadataName(GuardConvention.TaskMetadataName),
            compilation.GetTypeByMetadataName(
                GuardConvention.ValueTaskMetadataName));

        catalog.Collect(compilation.Assembly.GlobalNamespace);

        return catalog;
    }

    /// <summary>
    /// Names the guard for <paramref name="returned" />, or returns
    /// <see langword="false" /> when nothing guards it.
    /// </summary>
    public bool TryFind(
        ITypeSymbol returned,
        out ITypeSymbol guarded,
        out string guard)
    {
        guarded = Unwrap(returned);
        guard = string.Empty;

        if (guarded is not INamedTypeSymbol named
         || !_byGuardedType.TryGetValue(
                named.OriginalDefinition,
                out GuardPair pair))
        {
            return false;
        }

        guard = pair.For(IsAwaitable(returned));

        return guard.Length > 0;
    }

    public bool IsGuard(IMethodSymbol method) =>
        _guards.Contains(method.OriginalDefinition);

    private void Collect(INamespaceOrTypeSymbol container)
    {
        foreach (ISymbol member in container.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol space:
                    Collect(space);

                    break;

                case INamedTypeSymbol type:
                    Collect(type);

                    break;

                case IMethodSymbol method:
                    Register(method);

                    break;
            }
        }
    }

    private void Register(IMethodSymbol method)
    {
        if (!GuardConvention.NamesAGuard(method.Name)
         || !GuardConvention.TakesAValueAndItsSource(method)
         || Unwrap(method.ReturnType) is not INamedTypeSymbol guarded
         || !SymbolEqualityComparer.Default.Equals(
                guarded,
                Unwrap(method.Parameters[0].Type)))
        {
            return;
        }

        _guards.Add(method.OriginalDefinition);

        INamedTypeSymbol key = guarded.OriginalDefinition;

        GuardPair pair = _byGuardedType.TryGetValue(key, out GuardPair found)
            ? found
            : GuardPair.None;

        _byGuardedType[key] = pair.With(
            $"{method.ContainingType.Name}.{method.Name}",
            method.Name == GuardConvention.AsynchronousName);
    }

    private bool IsAwaitable(ITypeSymbol type) =>
        type is INamedTypeSymbol named
     && named.IsGenericType
     && (SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, _task)
      || SymbolEqualityComparer.Default.Equals(
             named.OriginalDefinition,
             _valueTask));

    private ITypeSymbol Unwrap(ITypeSymbol type) =>
        IsAwaitable(type) && type is INamedTypeSymbol named
            ? named.TypeArguments[0]
            : type;

    /// <remarks>
    /// A type can be guarded on one side only, and the empty name is how that is
    /// carried. The rule stays quiet rather than naming the guard from the other
    /// side, which would not compile if a reader followed it.
    /// </remarks>
    private readonly struct GuardPair
    {
        private GuardPair(string synchronous, string asynchronous)
        {
            Synchronous = synchronous;
            Asynchronous = asynchronous;
        }

        public static GuardPair None =>
            new GuardPair(string.Empty, string.Empty);

        private string Synchronous { get; }

        private string Asynchronous { get; }

        public GuardPair With(string guard, bool asynchronous) =>
            asynchronous
                ? new GuardPair(Synchronous, guard)
                : new GuardPair(guard, Asynchronous);

        public string For(bool asynchronous) =>
            asynchronous ? Asynchronous : Synchronous;
    }
}
