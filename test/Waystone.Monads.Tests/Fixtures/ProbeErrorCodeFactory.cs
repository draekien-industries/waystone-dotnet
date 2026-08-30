namespace Waystone.Monads.Fixtures;

using Configs;

/// <summary>
/// An <see cref="ErrorCodeFactory" /> subclass that behaves exactly like its
/// base, existing only so a test can assert on the type it got back.
/// </summary>
public sealed class ProbeErrorCodeFactory : ErrorCodeFactory;
