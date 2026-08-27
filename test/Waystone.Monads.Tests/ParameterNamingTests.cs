namespace Waystone.Monads;

using Options;
using Results;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// DRA-110 settled a naming convention across the public surface and applied it
/// once. A one-time sweep decays, so these tests hold the four halves of it that
/// a rename would silently reintroduce: no keyword, no <c>create</c> prefix, a
/// lazy delegate named for what it produces, and a chain step named for the monad
/// it produces.
/// </remarks>
public sealed class ParameterNamingTests
{
    /// <summary>
    /// A parameter named for a C# keyword has to be declared <c>@escaped</c>, and
    /// so does every named argument that reaches it. Renaming one is a source
    /// break with no deprecation route, so the cost is paid once or never.
    /// </summary>
    [Fact]
    public void GivenAPublicParameter_ThenItShouldNotBeNamedForAKeyword()
    {
        List<string> offenders = PublicParameters()
                                .Where(
                                     parameter => Keywords.Contains(
                                         parameter.Name ?? string.Empty))
                                .Select(Describe)
                                .Distinct()
                                .OrderBy(name => name, StringComparer.Ordinal)
                                .ToList();

        offenders.ShouldBeEmpty();
    }

    /// <summary>
    /// The convention names a lazy delegate for what it produces, so
    /// <c>createOther</c> became <c>resultFactory</c>. A <c>create</c> prefix says
    /// only that the delegate is lazy, which its type already says.
    /// </summary>
    [Fact]
    public void GivenAPublicDelegateParameter_ThenItShouldNotUseACreatePrefix()
    {
        List<string> offenders = PublicParameters()
                                .Where(
                                     parameter =>
                                         parameter.Name?.StartsWith(
                                             "create",
                                             StringComparison.Ordinal)
                                      == true)
                                .Select(Describe)
                                .Distinct()
                                .OrderBy(name => name, StringComparer.Ordinal)
                                .ToList();

        offenders.ShouldBeEmpty();
    }

    /// <summary>
    /// A <c>Func</c> suffix names the parameter's own type. The convention names
    /// it for what the delegate produces instead, so <c>errFunc</c> became
    /// <c>errorFactory</c>.
    /// </summary>
    [Fact]
    public void GivenAPublicDelegateParameter_ThenItShouldNotUseAFuncSuffix()
    {
        List<string> offenders = PublicParameters()
                                .Where(
                                     parameter =>
                                         parameter.Name?.EndsWith(
                                             "Func",
                                             StringComparison.Ordinal)
                                      == true)
                                .Select(Describe)
                                .Distinct()
                                .OrderBy(name => name, StringComparer.Ordinal)
                                .ToList();

        offenders.ShouldBeEmpty();
    }

    /// <summary>
    /// A bare <c>factory</c> says the delegate is lazy without saying what it
    /// produces, which is the whole content of the name.
    /// </summary>
    /// <remarks>
    /// <c>Try</c> keeps it, because its delegate is the operation rather than a
    /// fallback value, and <c>UseErrorCodeFactory</c> keeps it because its
    /// argument is not a delegate at all. Both are listed here rather than
    /// pattern-matched, so adding a third exception is a deliberate edit.
    /// </remarks>
    [Fact]
    public void GivenABareFactoryParameter_ThenOnlyTheKnownMembersShouldHaveOne()
    {
        List<string> offenders =
            PublicParameters()
               .Where(parameter => parameter.Name == "factory")
               .Where(
                    parameter => parameter.Member.Name is not ("Try"
                        or "UseErrorCodeFactory"))
               .Select(Describe)
               .Distinct()
               .OrderBy(name => name, StringComparer.Ordinal)
               .ToList();

        offenders.ShouldBeEmpty();
    }

    /// <summary>
    /// A delegate returning one of the library's own monads is a chain step, and
    /// the chain scheme names it for the monad it produces. The name is the only
    /// thing distinguishing it from a boundary delegate at a call site, and the
    /// two want different awaitables: <c>MapAsync</c> takes a
    /// <see cref="Task{TResult}" /> and <c>AndThenAsync</c> a
    /// <see cref="ValueTask{TResult}" />, yet both called the parameter
    /// <c>map</c> until DRA-110.
    /// </summary>
    [Fact]
    public void
        GivenAChainShapedDelegateParameter_ThenItShouldBeNamedForItsMonad()
    {
        List<string> offenders =
            PublicParameters()
               .Where(
                    parameter => ChainNameFor(parameter) is { } expected
                              && parameter.Name != expected)
               .Select(Describe)
               .Distinct()
               .OrderBy(name => name, StringComparer.Ordinal)
               .ToList();

        offenders.ShouldBeEmpty();
    }

    /// <summary>
    /// Guards the chain test above against passing because no parameter matched
    /// its filter rather than because every match was named correctly.
    /// </summary>
    [Fact]
    public void GivenThePublicSurface_ThenThereShouldBeChainStepsToInspect()
    {
        PublicParameters()
           .Count(parameter => ChainNameFor(parameter) is not null)
           .ShouldBeGreaterThan(20);
    }

    /// <summary>
    /// Guards the two tests above against passing because they inspected nothing.
    /// </summary>
    [Fact]
    public void GivenThePublicSurface_ThenThereShouldBeParametersToInspect()
    {
        PublicParameters().Count().ShouldBeGreaterThan(500);
    }

    private static readonly HashSet<string> Keywords =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
        "char", "checked", "class", "const", "continue", "decimal", "default",
        "delegate", "do", "double", "else", "enum", "event", "explicit",
        "extern", "false", "finally", "fixed", "float", "for", "foreach",
        "goto", "if", "implicit", "in", "int", "interface", "internal", "is",
        "lock", "long", "namespace", "new", "null", "object", "operator", "out",
        "override", "params", "private", "protected", "public", "readonly",
        "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
        "static", "string", "struct", "switch", "this", "throw", "true", "try",
        "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
        "virtual", "void", "volatile", "while",
    ];

    private static string? ChainNameFor(ParameterInfo parameter)
    {
        Type type = parameter.ParameterType;

        if (!type.IsGenericType
         || type.GetGenericTypeDefinition().FullName?.StartsWith(
                "System.Func`",
                StringComparison.Ordinal)
         != true)
        {
            return null;
        }

        Type[] arguments = type.GetGenericArguments();
        Type produced = Unwrap(arguments[arguments.Length - 1]);

        if (!produced.IsGenericType) return null;

        Type definition = produced.GetGenericTypeDefinition();

        if (definition == typeof(Option<>)) return "optionFactory";

        return definition == typeof(Result<,>) ? "resultFactory" : null;
    }

    private static Type Unwrap(Type type)
    {
        if (!type.IsGenericType) return type;

        Type definition = type.GetGenericTypeDefinition();

        return definition == typeof(Task<>) || definition == typeof(ValueTask<>)
            ? type.GetGenericArguments()[0]
            : type;
    }

    private static IEnumerable<ParameterInfo> PublicParameters() =>
        typeof(Option<>).Assembly.GetExportedTypes()
                        .SelectMany(
                             type => type.GetMethods(
                                          BindingFlags.Public
                                        | BindingFlags.Static
                                        | BindingFlags.Instance)
                                     .Cast<MethodBase>()
                                     .Concat(type.GetConstructors()))
                        .SelectMany(method => method.GetParameters());

    private static string Describe(ParameterInfo parameter) =>
        $"{parameter.Member.DeclaringType!.Name}.{parameter.Member.Name}({parameter.Name})";
}
