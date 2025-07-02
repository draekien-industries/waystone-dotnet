namespace Waystone.Monads.Extensions;

using Shouldly;
using Waystone.Monads.Options;
using Waystone.Monads.Results;

public static class AssertionExtensions
{
    public static void ShouldBeOk<TOk, TErr>(this Result<TOk, TErr> result)
        where TOk : notnull
        where TErr : notnull
    {
        result.IsOk.ShouldBeTrue();
        result.IsErr.ShouldBeFalse();
        result.ShouldBeOfType<Ok<TOk, TErr>>();
    }

    public static void ShouldBeErr<TOk, TErr>(this Result<TOk, TErr> result)
        where TOk : notnull
        where TErr : notnull
    {
        result.IsOk.ShouldBeFalse();
        result.IsErr.ShouldBeTrue();
        result.ShouldBeOfType<Err<TOk, TErr>>();
    }

    public static void ShouldBeSome<T>(this Option<T> option)
        where T : notnull
    {
        option.IsSome.ShouldBeTrue();
        option.IsNone.ShouldBeFalse();
        option.ShouldBeOfType<Some<T>>();
    }

    public static void ShouldBeNone<T>(this Option<T> option)
        where T : notnull
    {
        option.IsSome.ShouldBeTrue();
        option.IsNone.ShouldBeFalse();
        option.ShouldBeOfType<None<T>>();
    }

    public static void ShouldWrapSomeValue<T>(this Option<T> option, T value)
        where T : notnull
    {
        option.ShouldBeSome();
        option.Unwrap().ShouldBe(value);
    }

    public static void ShouldWrapOkValue<TOk, TErr>(this Result<TOk, TErr> result, TOk value)
        where TOk : notnull
        where TErr : notnull
    {
        result.ShouldBeOk();
        result.Unwrap().ShouldBe(value);
    }

    public static void ShouldWrapErrValue<TOk, TErr>(this Result<TOk, TErr> result, TErr value)
        where TOk : notnull
        where TErr : notnull
    {
        result.ShouldBeErr();
        result.UnwrapErr().ShouldBe(value);
    }
}