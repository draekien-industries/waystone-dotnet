namespace Waystone.Monads.Shouldly.Analyzers;

using Microsoft.CodeAnalysis.Diagnostics;

public abstract class AssertionAnalyzer : DiagnosticAnalyzer
{
    public sealed override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();

        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(
            start =>
            {
                var symbols = AssertionSymbols.TryCreate(start.Compilation);

                if (symbols is null)
                {
                    return;
                }

                Register(start, symbols);
            });
    }

    protected abstract void Register(
        CompilationStartAnalysisContext context,
        AssertionSymbols symbols);
}
