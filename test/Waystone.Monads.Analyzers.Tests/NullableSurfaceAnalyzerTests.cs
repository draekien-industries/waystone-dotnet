namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class NullableSurfaceAnalyzerTests
{
    [Fact]
    public Task FlagsANullableMemberInATypeThatUsesOption() =>
        Verify.AnalyzerAsync<NullableSurfaceAnalyzer>(
            """
            internal class Repository
            {
                internal Option<int> Find(int id) => Option.None<int>();

                internal {|#0:string?|} Describe(int id) => null;
            }
            """,
            Verify.Diagnostic(Rules.NullableMemberAlongsideMonads)
               .WithLocation(0)
               .WithArguments("Describe", "Find"));

    [Fact]
    public Task FlagsANullablePropertyAlongsideAResultMember() =>
        Verify.AnalyzerAsync<NullableSurfaceAnalyzer>(
            """
            internal class Repository
            {
                internal Result<int, string> Save(int id) =>
                    Result.Ok<int, string>(id);

                internal {|#0:string?|} Name { get; set; }
            }
            """,
            Verify.Diagnostic(Rules.NullableMemberAlongsideMonads)
               .WithLocation(0)
               .WithArguments("Name", "Save"));

    [Fact]
    public Task IgnoresATypeWithNoMonadMembers() =>
        Verify.NoDiagnosticAsync<NullableSurfaceAnalyzer>(
            """
            internal class Repository
            {
                internal string? Describe(int id) => null;
            }
            """);

    [Fact]
    public Task IgnoresATypeWhereEveryMemberUsesTheMonads() =>
        Verify.NoDiagnosticAsync<NullableSurfaceAnalyzer>(
            """
            internal class Repository
            {
                internal Option<int> Find(int id) => Option.None<int>();

                internal Option<string> Describe(int id) =>
                    Option.None<string>();
            }
            """);

    [Fact]
    public Task FlagsANullableReturnUnderTheMigrationRule() =>
        Verify.AnalyzerAsync<NullableReturnAnalyzer>(
            """
            internal class Formatter
            {
                internal {|#0:string?|} Describe(int id) => null;
            }
            """,
            Verify.Diagnostic(Rules.NullableReturnCouldBeOption)
               .WithLocation(0)
               .WithArguments("Describe", "string?", "string"));
}
