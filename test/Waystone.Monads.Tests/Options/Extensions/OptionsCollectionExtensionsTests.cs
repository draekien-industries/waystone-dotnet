namespace Waystone.Monads.Options.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    public sealed class OptionsCollectionExtensionsTests
    {
        private static readonly List<Option<int>> Values =
            new List<Option<int>>
            {
                Option.Some(11),
                Option.Some(12),
                Option.None<int>(),
                Option.Some(2),
            };

        [Fact]
        public void
            GivenCollectionOfOptions_WhenInvokingFilter_ThenApplyFilter()
        {
            List<Option<int>> result = Values.Filter(x => x > 10).ToList();

            result.Count.ShouldBe(4);
            result.Count(x => x.IsSome).ShouldBe(2);
            result.Count(x => x.IsNone).ShouldBe(2);
        }

        [Fact]
        public void
            GivenCollectionOfOptions_AndMatchingPredicate_WhenInvokingFirstOrNone_ThenReturnMatch()
        {
            Option<int> result = Values.FirstOrNone(x => x > 10);

            result.ShouldBeSome();
            result.ShouldBe(Option.Some(11));
        }

        [Fact]
        public void
            GivenCollectionOfOptions_AndNoMatch_WhenInvokingFirstOrNone_ThenReturnMatch()
        {
            Option<int> result = Values.FirstOrNone(x => x > 20);

            result.ShouldBeNone();
            result.ShouldBe(Option.None<int>());
        }

        [Theory]
        [InlineData(10, 20, 11)]
        [InlineData(20, 20, 20)]
        public void
            GivenCollectionOfOptions_WhenInvokingFirstOr_ThenReturnExpected(
                int minValue,
                int @default,
                int expected)
        {
            int results = Values.FirstOr(x => x > minValue, @default);

            results.ShouldBe(expected);
        }

        [Theory]
        [InlineData(10, 20, 11)]
        [InlineData(20, 20, 20)]
        public void
            GivenCollectionOfOptions_WhenInvokingFirstOrElse_ThenReturnExpected(
                int minValue,
                int @default,
                int expected)
        {
            int results = Values.FirstOrElse(x => x > minValue, () => @default);

            results.ShouldBe(expected);
        }

        [Fact]
        public void
            GivenCollectionOfOptions_AndMatchingPredicate_WhenInvokingLastOrNone_ThenReturnMatch()
        {
            Option<int> result = Values.LastOrNone(x => x > 10);

            result.ShouldBeSome();
            result.ShouldBe(Option.Some(12));
        }

        [Fact]
        public void
            GivenCollectionOfOptions_AndNoMatch_WhenInvokingLastOrNone_ThenReturnMatch()
        {
            Option<int> result = Values.LastOrNone(x => x > 20);

            result.ShouldBeNone();
            result.ShouldBe(Option.None<int>());
        }

        [Theory]
        [InlineData(10, 20, 12)]
        [InlineData(20, 20, 20)]
        public void
            GivenCollectionOfOptions_WhenInvokingLastOr_ThenReturnExpected(
                int minValue,
                int @default,
                int expected)
        {
            int results = Values.LastOr(x => x > minValue, @default);

            results.ShouldBe(expected);
        }

        [Theory]
        [InlineData(10, 20, 12)]
        [InlineData(20, 20, 20)]
        public void
            GivenCollectionOfOptions_WhenInvokingLastOrElse_ThenReturnExpected(
                int minValue,
                int @default,
                int expected)
        {
            int results = Values.LastOrElse(x => x > minValue, () => @default);

            results.ShouldBe(expected);
        }

        [Fact]
        public void
            GivenCollectionOfOptions_WhenInvokingMap_ThenReturnMappedOptions()
        {
            List<Option<string>> results =
                Values.Map(x => x.ToString()).ToList();

            results.Count(x => x.IsSome).ShouldBe(3);
            results.First(x => x.IsSome).ShouldBeSomeValue("11");
        }

        [Fact]
        public void
            GivenCollectionOfOptions_WhenInvokingFlatten_ThenReturnTheSomeValues()
        {
            List<int> results = Values.Flatten().ToList();

            results.ShouldBe(new[] { 11, 12, 2 });
        }

        [Fact]
        public void
            GivenEmptyCollection_WhenInvokingFlatten_ThenReturnAnEmptySequence() =>
            new List<Option<int>>().Flatten().ShouldBeEmpty();

        [Fact]
        public void GivenCollectionOfNone_WhenInvokingFlatten_ThenReturnAnEmptySequence() =>
            new List<Option<int>> { Option.None<int>(), Option.None<int>() }
               .Flatten()
               .ShouldBeEmpty();

        [Fact]
        public void GivenThrowingSource_WhenInvokingFlatten_ThenDoNotEnumerate()
        {
            IEnumerable<int> flattened = ThrowingSource().Flatten();

            flattened.Take(1).ToList().ShouldBe(new[] { 1 });
            Should.Throw<InvalidOperationException>(() => flattened.ToList());
        }

        [Fact]
        public void
            GivenCollectionOfNestedOptions_WhenInvokingFlatten_ThenTheSequenceOverloadIsChosen()
        {
            List<Option<Option<int>>> nested = new List<Option<Option<int>>>
            {
                Option.Some(Option.Some(1)),
                Option.None<Option<int>>(),
            };

            List<Option<int>> results = nested.Flatten().ToList();

            results.ShouldBe(new[] { Option.Some(1) });
        }

        [Fact]
        public void
            GivenAllSome_WhenInvokingCollect_ThenReturnSomeOfEveryValueInOrder()
        {
            List<Option<int>> options = new List<Option<int>>
            {
                Option.Some(1),
                Option.Some(2),
                Option.Some(3),
            };

            options.Collect().ShouldBeSomeValue(new[] { 1, 2, 3 });
        }

        [Fact]
        public void
            GivenEmptyCollection_WhenInvokingCollect_ThenReturnSomeOfAnEmptyList() =>
            new List<Option<int>>().Collect().Unwrap().ShouldBeEmpty();

        [Fact]
        public void GivenANone_WhenInvokingCollect_ThenReturnNone() =>
            Values.Collect().ShouldBeNone();

        [Fact]
        public void
            GivenThrowingSource_WhenInvokingCollect_ThenStopAtTheFirstNone() =>
            ThrowingAfterNoneSource().Collect().ShouldBeNone();

        [Fact]
        public async Task
            GivenAllSomeStream_WhenInvokingCollectAsync_ThenReturnSomeOfEveryValueInOrder()
        {
            Option<IReadOnlyList<int>> result = await SomeStream()
               .CollectAsync(TestContext.Current.CancellationToken);

            result.ShouldBeSomeValue(new[] { 1, 2, 3 });
        }

        [Fact]
        public async Task
            GivenEmptyStream_WhenInvokingCollectAsync_ThenReturnSomeOfAnEmptyList()
        {
            Option<IReadOnlyList<int>> result = await EmptyStream()
               .CollectAsync(TestContext.Current.CancellationToken);

            result.Unwrap().ShouldBeEmpty();
        }

        [Fact]
        public async Task
            GivenThrowingStream_WhenInvokingCollectAsync_ThenStopAtTheFirstNone()
        {
            Option<IReadOnlyList<int>> result = await ThrowingStream()
               .CollectAsync(TestContext.Current.CancellationToken);

            result.ShouldBeNone();
        }

#pragma warning disable CS1998
        private static async IAsyncEnumerable<Option<int>> SomeStream()
        {
            yield return Option.Some(1);
            yield return Option.Some(2);
            yield return Option.Some(3);
        }

        private static async IAsyncEnumerable<Option<int>> EmptyStream()
        {
            yield break;
        }

        private static async IAsyncEnumerable<Option<int>> ThrowingStream()
        {
            yield return Option.Some(1);
            yield return Option.None<int>();

            throw new InvalidOperationException("Enumerated too far.");
        }
#pragma warning restore CS1998

        private static IEnumerable<Option<int>> ThrowingSource()
        {
            yield return Option.Some(1);

            throw new InvalidOperationException("Enumerated too far.");
        }

        private static IEnumerable<Option<int>> ThrowingAfterNoneSource()
        {
            yield return Option.Some(1);
            yield return Option.None<int>();

            throw new InvalidOperationException("Enumerated too far.");
        }
    }
}
