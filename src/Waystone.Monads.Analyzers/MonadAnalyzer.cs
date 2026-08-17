namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.Diagnostics;

public abstract class MonadAnalyzer : DiagnosticAnalyzer
{
    public sealed override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();

        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(
            start =>
            {
                var symbols = MonadSymbols.TryCreate(start.Compilation);

                if (symbols is null)
                {
                    return;
                }

                Register(start, symbols);
            });
    }

    protected abstract void Register(
        CompilationStartAnalysisContext context,
        MonadSymbols symbols);
}
