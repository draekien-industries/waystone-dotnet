#if NETFRAMEWORK
namespace System.Runtime.CompilerServices;

using System;

/// <summary>
/// Declared for the subject compilations rather than for this assembly's own code.
/// </summary>
/// <remarks>
/// <see cref="Verify" /> builds a subject's references out of the test host's loaded
/// assemblies, so what a subject can name is whatever the framework running the tests
/// carries. .NET Framework carries no <c>CallerArgumentExpression</c>, and the only
/// declaration reachable there is PolySharp's <c>internal</c> one inside
/// <c>Waystone.Monads</c> — which a subject sees as <c>CS0122</c> rather than as a
/// missing type, and which no amount of qualifying gets past.
/// <para>
/// Declaring it here rather than in the subject source is what keeps the two apart.
/// <c>Verify.Preamble</c> opens with a file-scoped namespace, so a subject cannot
/// declare a second namespace at all, and a duplicate declaration would be a
/// <c>CS0436</c> on the three frameworks that already have the real one.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    /// <summary>Names the parameter whose source text the compiler supplies.</summary>
    /// <param name="parameterName">
    /// The parameter to read the caller's expression from. Matching nothing leaves the
    /// annotated parameter at its default rather than failing.
    /// </param>
    public CallerArgumentExpressionAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    /// <summary>The parameter the caller's expression is read from.</summary>
    public string ParameterName { get; }
}
#endif
