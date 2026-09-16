namespace Vagrant.Staffing;

/// <summary>A piece of work needing a clerk before it can be done.</summary>
/// <remarks>
/// Carries what the work is about and nothing about what the work is. Staffing does not
/// know that identifying an item takes longer than selling one, and a kind or a priority
/// here would be a rule this context has no way to apply.
/// </remarks>
/// <param name="Subject">What the work is about.</param>
public readonly record struct Errand(ErrandSubject Subject);
