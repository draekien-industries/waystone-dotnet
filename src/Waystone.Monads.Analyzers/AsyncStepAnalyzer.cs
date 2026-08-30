namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncStepAnalyzer : MonadAnalyzer
{
    private static readonly ImmutableHashSet<string> StepNames =
        ImmutableHashSet.Create("AndThenAsync", "OrElseAsync");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.TaskReturningAsyncStep);

    private protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols) =>
        context.RegisterSyntaxNodeAction(
            node => Analyze(node, symbols),
            SyntaxKind.InvocationExpression);

    /// <remarks>
    /// A syntax action rather than an operation action, because the call this rule
    /// describes is one that failed overload resolution — there is no
    /// <c>IInvocationOperation</c> for a call that does not bind, so the candidates
    /// are the only record of what the caller was reaching for.
    /// <para>
    /// A syntax action pays for that: Roslyn filters an operation action by operation
    /// kind for free, and this one is handed every invocation in every file on every
    /// keystroke. So the member name is read off the syntax and matched first, and
    /// nothing touches the semantic model until it matches — binding an invocation is
    /// far dearer than a string comparison, and almost no invocation in a solution is
    /// named <c>AndThenAsync</c> or <c>OrElseAsync</c>.
    /// </para>
    /// <para>
    /// Reading the name from syntax is also what lets the rest stay loose. Nothing
    /// here matches an argument to a parameter position, which on the awaited-receiver
    /// extensions would mean unpicking the receiver from the argument list. It does
    /// not need to: a call that binds returns early, so any step-shaped member reached
    /// with a <c>Task</c>-returning method group is already a compiler error.
    /// </para>
    /// </remarks>
    private static void Analyze(
        SyntaxNodeAnalysisContext context,
        MonadSymbols symbols)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (StepNameOf(invocation) is not { } step)
        {
            return;
        }

        var info = context.SemanticModel.GetSymbolInfo(
            invocation,
            context.CancellationToken);

        if (info.Symbol is not null || !IsOurs(info, invocation, context, symbols))
        {
            return;
        }

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (MethodGroupAt(argument, context, symbols) is not var (group,
                    returned))
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rules.TaskReturningAsyncStep,
                    argument.GetLocation(),
                    group.Name,
                    Semantics.Display(returned),
                    step,
                    "ValueTask<"
                  + Semantics.Display(returned.TypeArguments[0])
                  + ">",
                    Wrap(group)));
        }
    }

    /// <summary>
    /// Gets the member name the call is reaching for when it is one of the chaining
    /// steps, or null for every other invocation.
    /// </summary>
    /// <remarks>
    /// This is the whole hot-path filter, and it is syntax only. A reduced call names
    /// the member after the dot; the compatibility static form and a
    /// <c>using static</c> both name it outright.
    /// </remarks>
    private static string? StepNameOf(InvocationExpressionSyntax invocation)
    {
        var name = invocation.Expression as SimpleNameSyntax
                ?? (invocation.Expression as MemberAccessExpressionSyntax)?.Name;

        return name is { } named
         && StepNames.Contains(named.Identifier.ValueText)
                ? named.Identifier.ValueText
                : null;
    }

    /// <summary>
    /// Checks whether any candidate the failed call named is one of this library's.
    /// </summary>
    private static bool IsOurs(
        SymbolInfo info,
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context,
        MonadSymbols symbols)
    {
        var receiver = ReceiverTypeOf(invocation, context);

        return info.CandidateSymbols
           .OfType<IMethodSymbol>()
           .Any(candidate => IsOurs(candidate, receiver, symbols));
    }

    /// <summary>
    /// Gets the type of the expression before the dot, or null when the call does not
    /// have one.
    /// </summary>
    private static ITypeSymbol? ReceiverTypeOf(
        InvocationExpressionSyntax invocation,
        SyntaxNodeAnalysisContext context) =>
        invocation.Expression is MemberAccessExpressionSyntax access
            ? context.SemanticModel
               .GetTypeInfo(access.Expression, context.CancellationToken)
               .Type
            : null;

    /// <remarks>
    /// The containing-type clause is what matches a core member — the receiver clause
    /// would too, but only where the call is written with a dot. The rest is
    /// <see cref="MonadSymbols.IsMonadCandidate" />, which the rename fix shares.
    /// </remarks>
    private static bool IsOurs(
        IMethodSymbol method,
        ITypeSymbol? receiver,
        MonadSymbols symbols) =>
        symbols.IsMonad(method.ContainingType)
     || symbols.IsMonadCandidate(method, receiver);

    /// <summary>
    /// Gets the async lambda that wraps <paramref name="group" />, which is the
    /// correction the message tells the caller to type.
    /// </summary>
    private static string Wrap(IMethodSymbol group) =>
        group.Parameters.Length == 1
            ? "async " + group.Parameters[0].Name + " => await " + group.Name + "("
          + group.Parameters[0].Name + ")"
            : "async () => await " + group.Name + "()";

    /// <summary>
    /// Gets the method group an argument names together with the
    /// <c>Task</c>-of-a-monad it returns, or null when the argument is not one.
    /// </summary>
    /// <remarks>
    /// A group taking more than one argument is excluded, because no chaining step
    /// accepts one — the call would not bind however the step returned, so the
    /// message's advice would not fix it.
    /// <para>
    /// The syntax check is what excludes a lambda. A lambda's <c>GetSymbolInfo</c>
    /// answers with the <c>IMethodSymbol</c> Roslyn synthesises for it, so a test on
    /// the symbol alone would report the very shape this rule tells the caller to
    /// write.
    /// </para>
    /// <para>
    /// The candidates are read without also asking for a bound symbol. Measured: a
    /// method group named in an argument of a call that failed overload resolution
    /// reports as a member group with no symbol, every time — and this runs only on
    /// such a call, so a bound branch here would never be taken.
    /// </para>
    /// </remarks>
    private static (IMethodSymbol Group, INamedTypeSymbol Returned)?
        MethodGroupAt(
            ArgumentSyntax argument,
            SyntaxNodeAnalysisContext context,
            MonadSymbols symbols)
    {
        if (argument.Expression is not (IdentifierNameSyntax
            or MemberAccessExpressionSyntax))
        {
            return null;
        }

        var info = context.SemanticModel.GetSymbolInfo(
            argument.Expression,
            context.CancellationToken);

        foreach (var group in info.CandidateSymbols.OfType<IMethodSymbol>())
        {
            if (group.Parameters.Length < 2
             && group.ReturnType is INamedTypeSymbol returned
             && SymbolEqualityComparer.Default.Equals(
                    returned.OriginalDefinition,
                    symbols.Task)
             && symbols.IsMonad(returned.TypeArguments[0]))
            {
                return (group, returned);
            }
        }

        return null;
    }
}
