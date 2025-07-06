namespace Waystone.Monads.Options.Iterators;

using JetBrains.Annotations;
using Waystone.Monads.Extensions;
using Xunit;

[TestSubject(typeof(OptionIterator<>))]
public sealed class OptionIteratorTests
{
    [Fact]
    public void GivenSome_WhenInvokingNext_ReturnSome()
    {
        Option<int> some = Option.Some(1);
        var result = some.Iter().Next();
        result.ShouldBeSomeValue(1);
    }
}