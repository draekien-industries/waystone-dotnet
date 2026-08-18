namespace Waystone.Monads.Options.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using Xunit;

    public sealed class OptionsCollectionExtensionsTests
    {
        private static readonly List<Option<int>> Values =
            new List<Option<int>>
            {
                11,
                12,
                Option.None<int>(),
                2,
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

            result.IsSome.ShouldBeTrue();
            result.ShouldBe(Option.Some(11));
        }

        [Fact]
        public void
            GivenCollectionOfOptions_AndNoMatch_WhenInvokingFirstOrNone_ThenReturnMatch()
        {
            Option<int> result = Values.FirstOrNone(x => x > 20);

            result.IsNone.ShouldBeTrue();
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

            result.IsSome.ShouldBeTrue();
            result.ShouldBe(Option.Some(12));
        }

        [Fact]
        public void
            GivenCollectionOfOptions_AndNoMatch_WhenInvokingLastOrNone_ThenReturnMatch()
        {
            Option<int> result = Values.LastOrNone(x => x > 20);

            result.IsNone.ShouldBeTrue();
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
            results.First(x => x.IsSome).Unwrap().ShouldBe("11");
            results.First(x => x.IsSome).Unwrap().ShouldBe("11");
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

        private static IEnumerable<Option<int>> ThrowingSource()
        {
            yield return Option.Some(1);

            throw new InvalidOperationException("Enumerated too far.");
        }
    }
}
