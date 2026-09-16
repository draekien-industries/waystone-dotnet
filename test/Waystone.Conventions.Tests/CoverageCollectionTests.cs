namespace Waystone.Conventions.Tests;

using System.Linq;
using Shouldly;
using Xunit;

public sealed class CoverageCollectionTests
{
    public static TheoryData<string> InstrumentingProjectNames =>
    [
        ..TestProjects.All
                      .Where(static project => project.CompilesAgainstAnotherProject)
                      .Select(static project => project.Name),
    ];

    [Theory]
    [MemberData(nameof(InstrumentingProjectNames))]
    public void EveryTestProjectCompilingAgainstAnotherCollectsCoverage(
        string projectName)
    {
        TestProjects.Named(projectName)
                    .CollectsCoverage
                    .ShouldBeTrue(
                         $"{projectName} compiles against another project but references no "
                       + "coverlet.collector, so --collect:\"XPlat Code Coverage\" writes no "
                       + "report for it. The run still says Passed, and its subject reaches "
                       + "Codecov as whatever some other project's tests happened to load.");
    }

    [Fact]
    public void ThereAreTestProjectsToCheck() =>
        InstrumentingProjectNames.ShouldNotBeEmpty(
            "No *.Tests.csproj under test or sample carries a ProjectReference, so the theory "
          + "above passed by finding nothing to check rather than by finding nothing wrong.");
}
