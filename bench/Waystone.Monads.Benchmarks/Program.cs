namespace Waystone.Monads.Benchmarks;

using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

public static class Program
{
    private const string LabelVariable = "WAYSTONE_BENCH_LABEL";
    private const string SolutionFile = "Waystone.Net.sln";

    public static void Main(string[] args)
    {
        IConfig config =
            DefaultConfig.Instance.WithArtifactsPath(ResolveArtifactsPath());

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                         .Run(args, config);
    }

    private static string ResolveArtifactsPath()
    {
        string label =
            Environment.GetEnvironmentVariable(LabelVariable) is
                { Length: > 0 } set
                ? set
                : "local";

        return Path.Combine(FindRepositoryRoot(), "artifacts", label);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFile)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
