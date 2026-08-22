namespace Waystone.Monads.Results.Errors;

using System;

/// <summary>
/// Marks an enum as the source of a generated set of <see cref="ErrorCode" />
/// and <see cref="Error" /> members, one per declared enum member.
/// </summary>
/// <remarks>
/// <para>
/// The generated class is named after the enum with a trailing <c>Error</c> or
/// <c>ErrorCode</c> deduplicated — <c>OrderError</c> produces
/// <c>OrderErrorProvider</c> — and is emitted in the enum's namespace at the
/// enum's declared accessibility.
/// </para>
/// <para>
/// The generated codes follow the same <c>{EnumTypeName}.{MemberName}</c>
/// scheme as the default <see cref="Configs.ErrorCodeFactory" />, so a generated
/// constant and a call to <see cref="ErrorCode.FromEnum" /> return the same
/// string. Installing a custom factory through
/// <see cref="Configs.MonadOptions.UseErrorCodeFactory" /> changes the
/// runtime string and not the generated one.
/// </para>
/// <para>
/// Because the code is derived from the enum's name and its members' names,
/// renaming either is a breaking change to the code a consumer of the error
/// observes.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class ErrorCodeProviderAttribute : Attribute
{
    /// <summary>
    /// The scheme the generated codes of this enum follow, overriding any
    /// <see cref="ErrorCodeFormatAttribute" /> on the assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Takes the placeholders <c>{enum}</c> and <c>{member}</c>, each optionally
    /// followed by a casing: <c>kebab</c>, <c>snake</c>, <c>lower</c> or
    /// <c>upper</c>. Write a literal brace as <c>{{</c> or <c>}}</c>. Defaults to
    /// <c>"{enum}.{member}"</c>, which is the scheme the default
    /// <see cref="Configs.ErrorCodeFactory" /> produces.
    /// </para>
    /// <para>
    /// For example <c>"order.{member:kebab}"</c> gives the member
    /// <c>NotFound</c> the code <c>order.not-found</c>.
    /// </para>
    /// </remarks>
    public string? Format { get; set; }
}
