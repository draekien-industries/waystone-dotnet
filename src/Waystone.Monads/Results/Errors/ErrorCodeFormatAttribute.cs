namespace Waystone.Monads.Results.Errors;

using System;

/// <summary>
/// Sets the scheme every generated error code in this project follows, for each
/// enum that does not set its own through
/// <see cref="ErrorCodeCatalogAttribute.Format" />.
/// </summary>
/// <remarks>
/// <para>
/// Apply once, at assembly level: <c>[assembly: ErrorCodeFormat("{enum:kebab}.{member:kebab}")]</c>.
/// </para>
/// <para>
/// Takes the placeholders <c>{enum}</c> and <c>{member}</c>, each optionally
/// followed by a casing: <c>kebab</c>, <c>snake</c>, <c>lower</c> or <c>upper</c>.
/// Write a literal brace as <c>{{</c> or <c>}}</c>. Without this attribute the
/// scheme is <c>"{enum}.{member}"</c>, which is what the default
/// <see cref="Configs.ErrorCodeFactory" /> produces.
/// </para>
/// <para>
/// The scheme is baked into the generated members at build time, so it applies
/// per project. An enum in a referenced assembly keeps the scheme that assembly
/// was built with.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ErrorCodeFormatAttribute : Attribute
{
    /// <summary>
    /// Sets the scheme every generated error code in this project follows.
    /// </summary>
    /// <param name="format">
    /// The scheme, for example <c>"{enum:kebab}.{member:kebab}"</c>.
    /// </param>
    public ErrorCodeFormatAttribute(string format)
    {
        Format = format;
    }

    /// <summary>The scheme the generated codes follow.</summary>
    public string Format { get; }
}
