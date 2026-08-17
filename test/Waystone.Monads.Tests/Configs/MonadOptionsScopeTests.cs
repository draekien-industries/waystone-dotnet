namespace Waystone.Monads.Configs;

using System;
using System.Threading.Tasks;
using FluentValidation.Configs;
using JetBrains.Annotations;
using Results.Errors;
using Shouldly;
using Xunit;

[TestSubject(typeof(MonadOptionsScope))]
public sealed class MonadOptionsScopeTests
{
    private enum TestErrorCodes
    {
        Failure,
    }

    private static string ResolveFallbackCode() => new ErrorCode(" ").Value;

    private static string ResolveFallbackMessage() =>
        new Error("code", " ").Message;

    [Fact]
    public void GivenScope_WhenResolvingOptions_ThenUseScopedValue()
    {
        using (MonadOptions.CreateScope(o => o.UseFallbackErrorCode("scoped")))
        {
            ResolveFallbackCode().ShouldBe("scoped");
        }
    }

    [Fact]
    public void GivenScopeHasEnded_WhenResolvingOptions_ThenUseGlobalValue()
    {
        string global = ResolveFallbackCode();

        using (MonadOptions.CreateScope(o => o.UseFallbackErrorCode("scoped")))
        {
            ResolveFallbackCode().ShouldBe("scoped");
        }

        ResolveFallbackCode().ShouldBe(global);
    }

    [Fact]
    public void GivenScope_WhenOptionIsNotOverridden_ThenInheritGlobalValue()
    {
        string globalMessage = ResolveFallbackMessage();

        using (MonadOptions.CreateScope(o => o.UseFallbackErrorCode("scoped")))
        {
            ResolveFallbackCode().ShouldBe("scoped");
            ResolveFallbackMessage().ShouldBe(globalMessage);
        }
    }

    [Fact]
    public void GivenNestedScopes_WhenInnerEnds_ThenRestoreOuterScope()
    {
        using (MonadOptions.CreateScope(o => o.UseFallbackErrorCode("outer")))
        {
            ResolveFallbackCode().ShouldBe("outer");

            using (MonadOptions.CreateScope(
                o => o.UseFallbackErrorCode("inner")))
            {
                ResolveFallbackCode().ShouldBe("inner");
            }

            ResolveFallbackCode().ShouldBe("outer");
        }
    }

    [Fact]
    public async Task GivenScope_WhenAwaiting_ThenScopeFlowsAcrossAwait()
    {
        using (MonadOptions.CreateScope(o => o.UseFallbackErrorCode("scoped")))
        {
            await Task.Yield();
            ResolveFallbackCode().ShouldBe("scoped");

            await Task.Run(
                () => ResolveFallbackCode().ShouldBe("scoped"),
                TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task
        GivenConcurrentScopes_WhenResolvingOptions_ThenEachFlowIsIsolated()
    {
        async Task<string> Resolve(string code)
        {
            using (MonadOptions.CreateScope(o => o.UseFallbackErrorCode(code)))
            {
                await Task.Yield();
                await Task.Delay(5, TestContext.Current.CancellationToken);

                return ResolveFallbackCode();
            }
        }

        string[] resolved = await Task.WhenAll(
            Resolve("first"),
            Resolve("second"),
            Resolve("third"));

        resolved.ShouldBe(["first", "second", "third"]);
    }

    [Fact]
    public void GivenScope_WhenScopeEnds_ThenGlobalIsNotMutated()
    {
        string global = ResolveFallbackCode();

        using (MonadOptions.CreateScope(o => o.UseFallbackErrorCode("scoped")))
        { }

        MonadOptions.Global.FallbackErrorCode.ShouldBe(global);
    }

    [Fact]
    public void GivenScope_WhenOverridingErrorCodeFactory_ThenUseScopedFactory()
    {
        string global = ErrorCode.FromEnum(TestErrorCodes.Failure).Value;

        using (MonadOptions.CreateScope(
            o => o.UseErrorCodeFactory(new PrefixingErrorCodeFactory())))
        {
            ErrorCode.FromEnum(TestErrorCodes.Failure)
               .Value.ShouldBe("scoped.Failure");
        }

        ErrorCode.FromEnum(TestErrorCodes.Failure).Value.ShouldBe(global);
    }

    [Fact]
    public void
        GivenScope_WhenOverridingValidationOptions_ThenUseScopedValidationOptions()
    {
        using (MonadOptions.CreateScope(
            o => o.UseValidationErrorCode("scoped.validation")))
        {
            MonadValidationOptions.Current.ValidationErrorCode.ShouldBe(
                "scoped.validation");
        }

        MonadValidationOptions.Current.ValidationErrorCode.ShouldNotBe(
            "scoped.validation");
    }

    [Fact]
    public void GivenPrebuiltOptions_WhenCreatingScope_ThenUseThoseOptions()
    {
        MonadOptions options =
            MonadOptions.Create(o => o.UseFallbackErrorCode("prebuilt"));

        using (MonadOptions.CreateScope(options))
        {
            ResolveFallbackCode().ShouldBe("prebuilt");
        }

        using (MonadOptions.CreateScope(options))
        {
            ResolveFallbackCode().ShouldBe("prebuilt");
        }
    }

    [Fact]
    public void
        GivenScopeIsOpen_WhenConfiguringGlobal_ThenScopeKeepsItsSnapshot()
    {
        string original = MonadOptions.Global.FallbackErrorCode;

        try
        {
            using (MonadOptions.CreateScope(
                o => o.UseFallbackErrorCode("scoped")))
            {
                MonadOptions.Configure(
                    o => o.UseFallbackErrorCode("reconfigured"));

                ResolveFallbackCode().ShouldBe("scoped");
            }

            ResolveFallbackCode().ShouldBe("reconfigured");
        }
        finally
        {
            MonadOptions.Configure(o => o.UseFallbackErrorCode(original));
        }
    }

    private sealed class PrefixingErrorCodeFactory : ErrorCodeFactory
    {
        public override ErrorCode FromEnum(Enum @enum) =>
            new($"scoped.{@enum}");
    }
}
