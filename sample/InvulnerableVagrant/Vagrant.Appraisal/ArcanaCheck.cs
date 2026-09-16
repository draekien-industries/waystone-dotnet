namespace Vagrant.Appraisal;

/// <summary>How well the shop read a specimen on one attempt.</summary>
/// <remarks>
/// The total is signed, which is the one number in this sample that is. Everywhere else
/// a negative value is meaningless and the type forbids it; here a low roll against a
/// penalty genuinely lands below zero, and rounding that up to zero would make two
/// different attempts indistinguishable.
/// </remarks>
/// <param name="Total">The roll and its modifiers, added up.</param>
public readonly record struct ArcanaCheck(int Total);
