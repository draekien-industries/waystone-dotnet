namespace Waystone.Monads.Analyzers;

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NestedChainAnalyzer : MonadAnalyzer
{
    internal const string DepthOption =
        "dotnet_code_quality.WM2025.max_chain_depth";

    internal const int DefaultMaxChainDepth = 2;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.NestedMonadChain);

    private protected override void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols) =>
        context.RegisterOperationAction(
            operation => Analyze(operation, symbols),
            OperationKind.Invocation);

    private static void Analyze(
        OperationAnalysisContext context,
        MonadSymbols symbols)
    {
        var invocation = (IInvocationOperation)context.Operation;

        if (!Semantics.IsChainCall(invocation, symbols)
         || invocation.Syntax is not InvocationExpressionSyntax
         || ReceiverIsAChainCall(invocation, symbols))
        {
            return;
        }

        int threshold = MaxChainDepth(context);

        if (DepthOf(invocation, symbols, threshold) != threshold)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.NestedMonadChain,
                Semantics.NameLocationOf(invocation),
                invocation.TargetMethod.Name));
    }

    /// <remarks>
    /// One for the call itself, and one more for every monad delegate it sits
    /// inside. A delegate passed to anything else counts for nothing, so a chain
    /// inside a <c>Select</c> lambda is as shallow as one at statement level.
    /// <para>
    /// Stops counting once <paramref name="threshold" /> is passed. The caller
    /// wants to know whether the depth *equals* the threshold, so every value
    /// above it is the same answer, and the walk would otherwise climb to the root
    /// of the tree on every call in the compilation.
    /// </para>
    /// </remarks>
    private static int DepthOf(
        IOperation operation,
        MonadSymbols symbols,
        int threshold)
    {
        int depth = 1;

        for (var parent = operation.Parent;
             parent is not null && depth <= threshold;
             parent = parent.Parent)
        {
            if (parent is IAnonymousFunctionOperation lambda
             && IsChainDelegate(lambda, symbols))
            {
                depth++;
            }
        }

        return depth;
    }

    private static bool IsChainDelegate(
        IAnonymousFunctionOperation lambda,
        MonadSymbols symbols)
    {
        IOperation current = lambda;

        while (current.Parent is IConversionOperation
            or IDelegateCreationOperation)
        {
            current = current.Parent;
        }

        return current.Parent is IArgumentOperation
            {
                Parent: IInvocationOperation invocation,
            }
            && Semantics.IsChainCall(invocation, symbols);
    }

    /// <remarks>
    /// The receiver reports instead, so a flat run of calls inside one delegate
    /// is one diagnostic on the call that opens it rather than one per call.
    /// </remarks>
    private static bool ReceiverIsAChainCall(
        IInvocationOperation invocation,
        MonadSymbols symbols) =>
        Semantics.ReceiverOf(invocation) is { } receiver
     && Semantics.Unconverted(receiver) is IInvocationOperation call
     && Semantics.IsChainCall(call, symbols);

    private static int MaxChainDepth(OperationAnalysisContext context) =>
        context.Options.AnalyzerConfigOptionsProvider
           .GetOptions(context.Operation.Syntax.SyntaxTree)
           .TryGetValue(DepthOption, out string? raw)
     && int.TryParse(
            raw,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out int parsed)
     && parsed > 0
            ? parsed
            : DefaultMaxChainDepth;
}
