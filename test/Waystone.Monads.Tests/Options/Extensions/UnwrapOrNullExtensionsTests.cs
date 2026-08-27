namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(OptionExtensions))]
public sealed class UnwrapOrNullExtensionsTests
{
    [Fact]
    public void GivenSome_WhenUnwrapOrNull_ThenReturnTheValue() =>
        Option.Some(1).UnwrapOrNull().ShouldBe(1);

    [Fact]
    public void GivenNone_WhenUnwrapOrNull_ThenReturnNull() =>
        Option.None<int>().UnwrapOrNull().ShouldBeNull();

    [Fact]
    public void
        GivenNone_WhenUnwrapOrNull_ThenTheAbsenceIsDistinctFromUnwrapOrDefault()
    {
        Option<int> none = Option.None<int>();

        none.UnwrapOrDefault().ShouldBe(0);
        none.UnwrapOrNull().ShouldBeNull();
    }

    [Fact]
    public async Task GivenSomeTask_WhenUnwrapOrNullAsync_ThenReturnTheValue()
    {
        int? value = await Task.FromResult(Option.Some(1))
           .UnwrapOrNullAsync();

        value.ShouldBe(1);
    }

    [Fact]
    public async Task GivenNoneTask_WhenUnwrapOrNullAsync_ThenReturnNull()
    {
        int? value = await Task.FromResult(Option.None<int>())
           .UnwrapOrNullAsync();

        value.ShouldBeNull();
    }

    [Fact]
    public async Task
        GivenSomeValueTask_WhenUnwrapOrNullAsync_ThenReturnTheValue()
    {
        int? value = await new ValueTask<Option<int>>(Option.Some(1))
           .UnwrapOrNullAsync();

        value.ShouldBe(1);
    }

    [Fact]
    public async Task GivenNoneValueTask_WhenUnwrapOrNullAsync_ThenReturnNull()
    {
        int? value = await new ValueTask<Option<int>>(Option.None<int>())
           .UnwrapOrNullAsync();

        value.ShouldBeNull();
    }
}
