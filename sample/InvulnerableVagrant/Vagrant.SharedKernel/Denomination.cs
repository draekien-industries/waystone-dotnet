namespace Vagrant.SharedKernel;

/// <summary>One of the four kinds of coin the shop deals in.</summary>
/// <remarks>
/// Ordered smallest first, and the numeric value of each member is how many copper
/// pieces one of it is worth. <see cref="Coin" /> relies on that, so reordering the
/// members or assigning different values changes what every price in the shop means.
/// </remarks>
public enum Denomination
{
    /// <summary>The lightest denomination, worth a hundredth of a gold piece.</summary>
    Copper = 1,

    /// <summary>Worth a tenth of a gold piece.</summary>
    Silver = 10,

    /// <summary>The denomination the shop quotes its prices in.</summary>
    Gold = 100,

    /// <summary>The heaviest denomination, worth ten gold pieces.</summary>
    Platinum = 1000,
}
