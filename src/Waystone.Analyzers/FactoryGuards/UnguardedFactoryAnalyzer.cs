namespace Waystone.Analyzers.FactoryGuards;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

/// <summary>
/// Reports a delegate whose return type is guarded elsewhere being invoked without
/// its result passing through that guard.
/// </summary>
/// <remarks>
/// The rule names no type of its own. It reads the guarded types out of whichever
/// methods in the compilation match <see cref="GuardConvention" />, so writing a
/// guard extends the rule and a project that declares none is never reported.
/// <para>
/// Only an invocation is reported. A member that hands the delegate to another
/// member rather than calling it is inheriting that member's guard, which is the
/// shape every forwarding overload has, and there is nothing to check at the
/// forward.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnguardedFactoryAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.UnguardedFactoryReturn);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();

        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(
            start =>
            {
                GuardCatalog catalog = GuardCatalog.Read(start.Compilation);

                if (catalog.IsEmpty)
                {
                    return;
                }

                start.RegisterOperationAction(
                    operation => Analyze(operation, catalog),
                    OperationKind.Invocation);
            });
    }

    private static void Analyze(
        OperationAnalysisContext context,
        GuardCatalog catalog)
    {
        var invocation = (IInvocationOperation)context.Operation;

        if (invocation.TargetMethod.MethodKind != MethodKind.DelegateInvoke
         || !catalog.TryFind(
                invocation.TargetMethod.ReturnType,
                out ITypeSymbol guarded,
                out var guard)
         || IsGuarded(invocation, catalog))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.UnguardedFactoryReturn,
                invocation.Syntax.GetLocation(),
                Name(invocation),
                guarded.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat),
                guard));
    }

    /// <remarks>
    /// The guard takes the invocation as an argument, so the check is one step up
    /// the operation tree rather than a dataflow question, and nothing can sit
    /// between the two: the convention makes a guard's parameter the type it
    /// returns, so passing the call to it is an identity conversion, which Roslyn
    /// gives no operation of its own.
    /// </remarks>
    private static bool IsGuarded(
        IInvocationOperation invocation,
        GuardCatalog catalog) =>
        invocation.Parent is IArgumentOperation
        {
            Parent: IInvocationOperation outer,
        }
     && catalog.IsGuard(outer.TargetMethod);

    /// <remarks>
    /// The parameter name is what a reader has to go and change, so it beats the
    /// delegate's type. A delegate reached any other way — a field, a local — has
    /// no name worth printing, and its syntax reads well enough in its place.
    /// <para>
    /// <c>Instance</c> is the delegate being called and is never absent on a
    /// <see cref="MethodKind.DelegateInvoke" />, which is the only kind reaching
    /// here.
    /// </para>
    /// </remarks>
    private static string Name(IInvocationOperation invocation) =>
        invocation.Instance is IParameterReferenceOperation parameter
            ? parameter.Parameter.Name
            : invocation.Instance!.Syntax.ToString();
}
