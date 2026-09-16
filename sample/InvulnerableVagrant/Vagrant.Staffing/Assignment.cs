namespace Vagrant.Staffing;

/// <summary>A clerk held to an errand until it is finished.</summary>
/// <param name="Clerk">Which clerk took it.</param>
/// <param name="Errand">What they took.</param>
/// <param name="Since">When they took it.</param>
public readonly record struct Assignment(
    ClerkId Clerk,
    Errand Errand,
    DateTimeOffset Since);
