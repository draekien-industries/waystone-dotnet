namespace Waystone.Monads.Iterators.Fixtures;

using System;

internal sealed class TestCloneable : ICloneable
{
    public TestCloneable()
    {
#if NET6_0_OR_GREATER
        Value = Random.Shared.Next();
#else
        var random = new Random();
        Value = random.Next();
#endif
    }

    public TestCloneable(TestCloneable source)
    {
        Value = source.Value;
    }

    public int Value { get; }

    /// <inheritdoc />
    public object Clone() => new TestCloneable(this);
}
