namespace Waystone.Monads.Results.Errors;

using System;

/// <summary>
/// Marks an enum as the source of a generated set of <see cref="ErrorCode" />
/// and <see cref="Error" /> members, one per declared enum member.
/// </summary>
/// <remarks>
/// <para>
/// The generated class is the enum's own name with <c>Catalog</c> appended —
/// <c>OrderFailure</c> produces <c>OrderFailureCatalog</c> — and is emitted in the
/// enum's namespace, <c>public</c> for a public enum and <c>internal</c> for any
/// other accessibility. Nothing is trimmed off the name, so two enums whose names
/// differ generate two classes whose names differ.
/// </para>
/// <para>
/// The catalog carries <c>Names</c> (a string constant per member), <c>Codes</c>
/// (an <see cref="ErrorCode" /> per member) and <c>Errors</c> (a factory taking
/// the message per member), plus the extension methods
/// <c>ToErrorCodeName()</c>, <c>ToErrorCode()</c> and <c>ToError(message)</c> for
/// a value only known at run time.
/// </para>
/// <para>
/// The enum must not be marked <c>[Flags]</c> (WMG0001), must not give two
/// members the same value (WMG0002), and must not declare a member named
/// <c>Names</c>, <c>Codes</c> or <c>Errors</c> (WMG0003), which are the nested
/// classes generated into the catalog. Each is a build error and stops the
/// catalog being generated at all.
/// </para>
/// <para>
/// The generated codes follow the <c>{EnumTypeName}.{MemberName}</c> scheme
/// unless <see cref="Format" /> or
/// <see cref="ErrorCodeFormatAttribute" /> says otherwise. Whichever applies is
/// read at compile time and baked into the constants, so the codes an enum
/// produces are fixed by its source and cannot be changed from configuration —
/// <see cref="Configs.MonadOptions.UseErrorCodeFactory" /> reaches only the codes
/// derived from exceptions.
/// </para>
/// <para>
/// Because the code is derived from the enum's name and its members' names,
/// renaming either is a breaking change to the code a consumer of the error
/// observes.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class ErrorCodeCatalogAttribute : Attribute
{
    /// <summary>
    /// The scheme the generated codes of this enum follow, overriding any
    /// <see cref="ErrorCodeFormatAttribute" /> on the assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Takes the placeholders <c>{enum}</c> and <c>{member}</c>, each optionally
    /// followed by a casing: <c>kebab</c>, <c>snake</c>, <c>lower</c> or
    /// <c>upper</c>. Write a literal brace as <c>{{</c> or <c>}}</c>; everything
    /// else is literal text.
    /// </para>
    /// <para>
    /// For example <c>"order.{member:kebab}"</c> gives the member
    /// <c>NotFound</c> the code <c>order.not-found</c>.
    /// </para>
    /// <para>
    /// Default: <see langword="null" />, which falls back to the assembly's
    /// <see cref="ErrorCodeFormatAttribute" />, or to <c>"{enum}.{member}"</c> when
    /// the assembly has none — the scheme the default
    /// <see cref="Configs.ErrorCodeFactory" /> produces.
    /// </para>
    /// <para>
    /// A format the generator cannot parse is a build error (WMG0005), as is one
    /// with no <c>{member}</c> placeholder, since every member would then get the
    /// same code (WMG0006). Neither generates a catalog.
    /// </para>
    /// </remarks>
    public string? Format { get; set; }
}
