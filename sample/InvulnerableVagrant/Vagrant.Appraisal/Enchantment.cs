namespace Vagrant.Appraisal;

/// <summary>The effect an identified specimen carries.</summary>
/// <param name="Name">What the effect is called.</param>
/// <param name="Effect">What it does when used.</param>
public readonly record struct Enchantment(string Name, string Effect);
