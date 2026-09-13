namespace Waystone.Conventions.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

public sealed class PackagedDocumentationTests
{
    public static TheoryData<string> PackableProjectNames =>
        [..PackableProjects.All.Select(static project => project.Name)];

    [Theory]
    [MemberData(nameof(PackableProjectNames))]
    public void NoPackagedDocCommentNamesAnInternalRuleId(string projectName)
    {
        PackableProject project = PackableProjects.Named(projectName);

        foreach (string documentation in PackableProjects.DocumentationFiles(project))
        {
            IReadOnlyList<Citation> citations =
                InternalRuleIds.In(File.ReadAllText(documentation));

            citations.ShouldBeEmpty(
                $"{Path.GetFileName(documentation)} ships {citations.Count} citation(s) of a rule "
              + "that never leaves this repository: "
              + string.Join(
                    ", ",
                    citations.Select(static citation => $"{citation.RuleId} on {citation.Member}"))
              + ". State the constraint and name the compiler error the caller sees instead.");
        }
    }

    [Theory]
    [MemberData(nameof(PackableProjectNames))]
    public void EveryPackableProjectHasDocumentationToScan(string projectName)
    {
        PackableProjects.DocumentationFiles(PackableProjects.Named(projectName))
                        .ShouldNotBeEmpty(
                             $"{projectName} produced no XML documentation under bin, so the scan "
                           + "above passed by finding nothing. src/Directory.Build.props turns "
                           + "GenerateDocumentationFile on for every project here, so this means "
                           + "the project was not built rather than that it documents nothing.");
    }

    [Fact]
    public void TheInternalNamespaceExemptionCoversOnlyTheGeneratorAttributes()
    {
        IEnumerable<string> exempted =
            PackableProjects.All
                            .SelectMany(PackableProjects.DocumentationFiles)
                            .SelectMany(static file =>
                                 InternalRuleIds.ExemptedIn(File.ReadAllText(file)))
                            .Select(static citation => citation.Member)
                            .Distinct(StringComparer.Ordinal)
                            .OrderBy(static member => member, StringComparer.Ordinal);

        exempted.ShouldBe(
            [
                "T:Waystone.Internal.SourceGenerators.GenerateAwaitedMemberAttribute",
                "T:Waystone.Internal.SourceGenerators.GenerateAwaitedReceiversAttribute",
            ],
            "The exemption skips a member by the namespace in its doc id, which is a "
          + "proxy for the type not being public rather than a reading of its "
          + "accessibility. It holds because the only exempted members are the two "
          + "attributes the awaited-receiver generator injects, and both are internal. "
          + "This list is pinned so that a third member arriving under Waystone.Internal.* "
          + "has to be confirmed internal by hand rather than silently widening the "
          + "exemption.");
    }
}
