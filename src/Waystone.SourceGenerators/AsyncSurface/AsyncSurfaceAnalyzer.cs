namespace Waystone.SourceGenerators.AsyncSurface;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

/// <summary>
/// Enforces that this library's own async members return <c>ValueTask</c>, so that
/// a chain can be handed to a step-shaped parameter by name.
/// </summary>
/// <remarks>
/// Declaration-site only, by design. Neither rule can see a <c>Task&lt;TOut&gt;</c>
/// instantiated as <c>Task&lt;Option&lt;U&gt;&gt;</c> at a call site, and that is
/// the correct limit — these rules govern what this repository declares, not what
/// a consumer substitutes into it.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncSurfaceAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rules.TaskReturningMonad);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();

        // The awaited receivers are deliberately not marked as generated code so
        // that RS0016/RS0017 keep running on them. That makes them visible here
        // too, which is what extends this rule over the generated surface.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(
            static start =>
            {
                var monads = MonadTypes.Load(start.Compilation);

                if (monads is null) return;

                start.RegisterSymbolAction(
                    symbol => Analyse(symbol, monads),
                    SymbolKind.Method);
            });
    }

    private static void Analyse(SymbolAnalysisContext context, MonadTypes monads)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (!IsPubliclyVisible(method)) return;

        if (!(method.ReturnType is INamedTypeSymbol returned)) return;

        if (!monads.IsTaskOfMonad(returned, out ITypeSymbol? monad)) return;

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.TaskReturningMonad,
                method.Locations[0],
                method.ToDisplayString(MemberFormat),
                Display(returned),
                "ValueTask<" + Display(monad!) + ">"));
    }

    /// <summary>
    /// Names a member as <c>Option.TryAsync&lt;T&gt;</c>. The minimally qualified
    /// format spells out every parameter, which buries the point of the message in
    /// a signature the reader is already looking at.
    /// </summary>
    private static readonly SymbolDisplayFormat MemberFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle
           .NameAndContainingTypes,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);

    private static bool IsPubliclyVisible(ISymbol symbol)
    {
        for (ISymbol? current = symbol; current is not null;
             current = current.ContainingType)
        {
            if (current.DeclaredAccessibility != Accessibility.Public) return false;
        }

        return true;
    }

    private static string Display(ISymbol symbol) =>
        symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
}
