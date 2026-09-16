namespace Vagrant.SharedKernel;

using System.Globalization;
using System.Text;
using Waystone.Monads.Options;

/// <summary>Money of the Dwendalian Empire, reckoned in four denominations.</summary>
/// <remarks>
/// A coin amount is never negative. The type has no subtraction operator for that
/// reason — <see cref="Less" /> returns <see cref="Option{T}" /> so that taking more
/// than is there produces no value rather than a debt.
/// </remarks>
public readonly record struct Coin : IComparable<Coin>
{
    private readonly long _copper;

    private Coin(long copper)
    {
        _copper = copper;
    }

    /// <summary>Nothing at all — an empty purse, or a price of zero.</summary>
    public static Coin Nothing => new(0);

    /// <summary>Whether this amount is nothing at all.</summary>
    public bool IsNothing => _copper == 0;

    /// <summary>An amount in gold pieces alone.</summary>
    /// <param name="gold">How many gold pieces. Zero is allowed.</param>
    /// <returns>The amount.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="gold" /> is negative. Coin cannot represent a debt, so this is a
    /// mistake at the call site rather than an outcome a caller could act on.
    /// </exception>
    public static Coin FromGold(int gold) => From(0, gold, 0, 0);

    /// <summary>An amount counted out in each of the four denominations.</summary>
    /// <param name="platinum">How many platinum pieces.</param>
    /// <param name="gold">How many gold pieces.</param>
    /// <param name="silver">How many silver pieces.</param>
    /// <param name="copper">How many copper pieces.</param>
    /// <returns>
    /// The total the four components come to. The components need not be reduced —
    /// twelve silver and no gold is the same amount as one gold and two silver.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Any component is negative.
    /// </exception>
    public static Coin From(int platinum, int gold, int silver, int copper)
    {
        Require(platinum, nameof(platinum));
        Require(gold, nameof(gold));
        Require(silver, nameof(silver));
        Require(copper, nameof(copper));

        return new Coin(
            (platinum * (long)Denomination.Platinum)
          + (gold * (long)Denomination.Gold)
          + (silver * (long)Denomination.Silver)
          + copper);
    }

    /// <summary>Adds two amounts together.</summary>
    /// <param name="left">One amount.</param>
    /// <param name="right">The other.</param>
    /// <returns>The sum. Always representable, since neither operand is negative.</returns>
    public static Coin operator +(Coin left, Coin right) =>
        new(left._copper + right._copper);

    /// <summary>Repeats an amount, for a line of several of the same item.</summary>
    /// <param name="coin">The amount to repeat.</param>
    /// <param name="quantity">How many times. Zero gives <see cref="Nothing" />.</param>
    /// <returns>The product.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="quantity" /> is negative.
    /// </exception>
    public static Coin operator *(Coin coin, int quantity)
    {
        Require(quantity, nameof(quantity));

        return new Coin(coin._copper * quantity);
    }

    /// <summary>Takes one amount away from another.</summary>
    /// <param name="other">The amount to take away.</param>
    /// <returns>
    /// What is left, or <c>None</c> when <paramref name="other" /> is the larger — there
    /// is no amount of coin that represents owing someone money.
    /// </returns>
    public Option<Coin> Less(Coin other) =>
        _copper >= other._copper
            ? Option.Some(new Coin(_copper - other._copper))
            : Option.None<Coin>();

    /// <summary>Whether this amount covers another.</summary>
    /// <param name="other">The amount to cover.</param>
    /// <returns>True when this amount is the same or larger.</returns>
    public bool IsAtLeast(Coin other) => _copper >= other._copper;

    /// <summary>Compares two amounts by what they are worth.</summary>
    /// <param name="other">The amount to compare against.</param>
    /// <returns>
    /// Negative when this amount is the smaller, zero when they are equal, positive when
    /// this amount is the larger.
    /// </returns>
    public int CompareTo(Coin other) => _copper.CompareTo(other._copper);

    /// <summary>Whether one amount is worth less than another.</summary>
    /// <param name="left">One amount.</param>
    /// <param name="right">The other.</param>
    /// <returns>True when <paramref name="left" /> is the smaller.</returns>
    public static bool operator <(Coin left, Coin right) => left._copper < right._copper;

    /// <summary>Whether one amount is worth more than another.</summary>
    /// <param name="left">One amount.</param>
    /// <param name="right">The other.</param>
    /// <returns>True when <paramref name="left" /> is the larger.</returns>
    public static bool operator >(Coin left, Coin right) => left._copper > right._copper;

    /// <summary>Whether one amount is worth no more than another.</summary>
    /// <param name="left">One amount.</param>
    /// <param name="right">The other.</param>
    /// <returns>True unless <paramref name="left" /> is the larger.</returns>
    public static bool operator <=(Coin left, Coin right) => left._copper <= right._copper;

    /// <summary>Whether one amount is worth no less than another.</summary>
    /// <param name="left">One amount.</param>
    /// <param name="right">The other.</param>
    /// <returns>True unless <paramref name="left" /> is the smaller.</returns>
    public static bool operator >=(Coin left, Coin right) => left._copper >= right._copper;

    /// <summary>How many copper pieces this amount comes to in total.</summary>
    /// <remarks>
    /// The storage unit, exposed for the host to persist an amount as a single column.
    /// Prefer the denomination-aware members for anything else.
    /// </remarks>
    /// <returns>The total in copper.</returns>
    public long InCopper() => _copper;

    /// <summary>Rebuilds an amount from a total in copper pieces.</summary>
    /// <param name="copper">The total, as <see cref="InCopper" /> reported it.</param>
    /// <returns>The amount.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="copper" /> is negative.
    /// </exception>
    public static Coin FromCopper(long copper)
    {
        if (copper < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(copper),
                copper,
                "Coin cannot be negative.");
        }

        return new Coin(copper);
    }

    /// <summary>Writes the amount the way a price is written on a shelf.</summary>
    /// <returns>
    /// The largest denominations first and empty ones omitted, so 45103 copper reads
    /// <c>45pp 1gp 3cp</c>. An amount of nothing reads <c>0gp</c>, because a price has to
    /// say something.
    /// </returns>
    public override string ToString()
    {
        if (_copper == 0) return "0gp";

        var written = new StringBuilder();
        long left = _copper;

        Write(written, ref left, Denomination.Platinum, "pp");
        Write(written, ref left, Denomination.Gold, "gp");
        Write(written, ref left, Denomination.Silver, "sp");
        Write(written, ref left, Denomination.Copper, "cp");

        return written.ToString();
    }

    private static void Write(
        StringBuilder written,
        ref long left,
        Denomination denomination,
        string suffix)
    {
        long worth = (long)denomination;
        long count = left / worth;

        if (count == 0) return;

        if (written.Length > 0) written.Append(' ');

        written.Append(count.ToString(CultureInfo.InvariantCulture)).Append(suffix);
        left -= count * worth;
    }

    private static void Require(int component, string name)
    {
        if (component < 0)
        {
            throw new ArgumentOutOfRangeException(
                name,
                component,
                "Coin cannot be negative.");
        }
    }
}
