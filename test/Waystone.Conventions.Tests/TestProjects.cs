namespace Waystone.Conventions.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

internal sealed record TestProject(
    string Name,
    bool CompilesAgainstAnotherProject,
    bool CollectsCoverage);

internal static class TestProjects
{
    private static readonly string[] Areas = ["test", "sample"];

    public static IReadOnlyList<TestProject> All { get; } = Discover();

    public static TestProject Named(string name) =>
        All.Single(project => project.Name == name);

    private static IReadOnlyList<TestProject> Discover() =>
        Areas
           .Select(area => Path.Combine(PackableProjects.RepositoryRoot, area))
           .Where(Directory.Exists)
           .SelectMany(static area => Directory.EnumerateFiles(
                area,
                "*.Tests.csproj",
                SearchOption.AllDirectories))
           .Select(Read)
           .OrderBy(static project => project.Name, StringComparer.Ordinal)
           .ToList();

    private static TestProject Read(string path)
    {
        XDocument document = XDocument.Load(path);

        return new TestProject(
            Path.GetFileNameWithoutExtension(path),
            document.Descendants("ProjectReference").Any(Compiled),
            document.Descendants("PackageReference")
                    .Any(static package =>
                         (string?)package.Attribute("Include") == "coverlet.collector"));
    }

    private static bool Compiled(XElement reference) =>
        !string.Equals(
            (string?)reference.Attribute("ReferenceOutputAssembly")
         ?? (string?)reference.Element("ReferenceOutputAssembly"),
            "false",
            StringComparison.OrdinalIgnoreCase);
}
