namespace Waystone.Monads.Benchmarks;

using BenchmarkDotNet.Attributes;
using Configs;
using FluentValidation.Configs;
using Results.Errors;

[MemoryDiagnoser]
public class MonadOptionsScopeBenchmarks
{
    [GlobalSetup(
        Targets =
        [
            nameof(ReadFallbackCode),
            nameof(ScopeEntryAndExit),
            nameof(ConfigureTheGlobal),
        ])]
    public void SetupWithoutSatellite()
    {
        MonadOptions.Configure(options => options.UseFallbackErrorCode("bench"));
    }

    [GlobalSetup(Target = nameof(ScopeEntryAndExitWithSatellite))]
    public void SetupWithSatellite()
    {
        MonadOptions.Configure(
            options => options.UseFallbackErrorCode("bench")
               .UseValidationErrorCode("bench.validation"));
    }

    [Benchmark(Baseline = true)]
    public string ReadFallbackCode() => new ErrorCode(" ").Value;

    [Benchmark]
    public void ScopeEntryAndExit()
    {
        using (MonadOptions.BeginScope(
            static options => options.UseFallbackErrorCode("scoped")))
        { }
    }

    [Benchmark]
    public void ScopeEntryAndExitWithSatellite()
    {
        using (MonadOptions.BeginScope(
            static options => options.UseFallbackErrorCode("scoped")))
        { }
    }

    [Benchmark]
    public void ConfigureTheGlobal()
    {
        MonadOptions.Configure(
            static options => options.UseFallbackErrorCode("reconfigured"));
    }
}
