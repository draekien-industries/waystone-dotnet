namespace Waystone.Conventions.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

internal sealed record PackableProject(string Name, string Directory);

internal static class PackableProjects
{
    private static readonly string Configuration =
        typeof(PackableProjects).Assembly
                                .GetCustomAttribute<AssemblyConfigurationAttribute>()
                              ?.Configuration
     ?? "Debug";

    public static string RepositoryRoot { get; } = FindRoot();

    public static IReadOnlyList<PackableProject> All { get; } = Discover();

    public static PackableProject Named(string name) =>
        All.Single(project => project.Name == name);

    private static IReadOnlyList<PackableProject> Discover() =>
        Directory
           .EnumerateFiles(
                Path.Combine(RepositoryRoot, "src"),
                "*.csproj",
                SearchOption.AllDirectories)
           .Where(static project =>
                !File.ReadAllText(project)
                     .Contains("<IsPackable>false</IsPackable>", StringComparison.Ordinal))
           .Select(static project => new PackableProject(
                Path.GetFileNameWithoutExtension(project),
                Path.GetDirectoryName(project)!))
           .OrderBy(static project => project.Name, StringComparer.Ordinal)
           .ToList();

    public static IReadOnlyList<string> DocumentationFiles(PackableProject project)
    {
        string output = Path.Combine(project.Directory, "bin", Configuration);

        return Directory.Exists(output)
            ? Directory
             .EnumerateFiles(output, $"{project.Name}.xml", SearchOption.AllDirectories)
             .OrderBy(static path => path, StringComparer.Ordinal)
             .ToList()
            : [];
    }

    private static string FindRoot()
    {
        DirectoryInfo? directory =
            new(Path.GetDirectoryName(typeof(PackableProjects).Assembly.Location)!);

        while (directory is not null
            && !File.Exists(Path.Combine(directory.FullName, "Waystone.Net.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
         ?? throw new InvalidOperationException(
                "No Waystone.Net.slnx above the test assembly, so the packable projects cannot be located.");
    }
}
