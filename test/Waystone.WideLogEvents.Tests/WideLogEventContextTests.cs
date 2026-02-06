namespace Waystone.WideLogEvents.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

public class WideLogEventContextTests
{
    [Fact]
    public void PushProperty_WithoutScope_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            WideLogEventContext.PushProperty("test", "value"));
    }

    [Fact]
    public void BeginScope_CreatesNewScope()
    {
        // Act
        using (WideLogEventContext.BeginScope())
        {
            WideLogEventContext.PushProperty("test", "value");

            IReadOnlyDictionary<string, object?> properties =
                WideLogEventContext.GetScopedProperties();

            // Assert
            properties.ContainsKey("test").ShouldBeTrue();
            properties["test"].ShouldBe("value");
        }
    }

    [Fact]
    public void GetScopedProperties_OutsideScope_ReturnsEmpty()
    {
        // Act
        IReadOnlyDictionary<string, object?> properties =
            WideLogEventContext.GetScopedProperties();

        // Assert
        properties.ShouldBeEmpty();
    }

    [Fact]
    public async Task ChildTask_CanPushToParentScope()
    {
        // Arrange
        using (WideLogEventContext.BeginScope())
        {
            WideLogEventContext.PushProperty("parent", "value");

            // Act
            await Task.Run(
                () => { WideLogEventContext.PushProperty("child", "value"); },
                TestContext.Current.CancellationToken);

            IReadOnlyDictionary<string, object?> properties =
                WideLogEventContext.GetScopedProperties();

            // Assert
            properties.ContainsKey("parent").ShouldBeTrue();
            properties.ContainsKey("child").ShouldBeTrue();
        }
    }

    [Fact]
    public void NestedScopes_StillProvideIsolation()
    {
        // Arrange & Act
        using (WideLogEventContext.BeginScope())
        {
            WideLogEventContext.PushProperty("outer", "1");

            using (WideLogEventContext.BeginScope())
            {
                // New scope should be empty (current behavior maintained)
                IReadOnlyDictionary<string, object?> innerPropertiesAtStart =
                    WideLogEventContext.GetScopedProperties();

                innerPropertiesAtStart.ShouldBeEmpty();

                WideLogEventContext.PushProperty("inner", "2");

                IReadOnlyDictionary<string, object?> innerProperties =
                    WideLogEventContext.GetScopedProperties();

                innerProperties.ContainsKey("inner").ShouldBeTrue();
                innerProperties["inner"].ShouldBe("2");
                innerProperties.ContainsKey("outer").ShouldBeFalse();
            }

            IReadOnlyDictionary<string, object?> outerProperties =
                WideLogEventContext.GetScopedProperties();

            outerProperties.ContainsKey("outer").ShouldBeTrue();
            outerProperties["outer"].ShouldBe("1");
            outerProperties.ContainsKey("inner").ShouldBeFalse();
        }
    }

    [Fact]
    public async Task ParallelTasks_ShareSameParentScope()
    {
        // Arrange
        using (WideLogEventContext.BeginScope())
        {
            // Act
            Task[] tasks = Enumerable.Range(0, 10)
               .Select(i => Task.Run(() =>
                {
                    WideLogEventContext.PushProperty($"key_{i}", i);
                }))
               .ToArray();

            await Task.WhenAll(tasks);

            IReadOnlyDictionary<string, object?> properties =
                WideLogEventContext.GetScopedProperties();

            // Assert
            for (var i = 0; i < 10; i++)
            {
                properties.ContainsKey($"key_{i}").ShouldBeTrue();
                properties[$"key_{i}"].ShouldBe(i);
            }
        }
    }

    [Fact]
    public async Task NestedScope_InChildTask_IsStillIsolated()
    {
        // Arrange
        using (WideLogEventContext.BeginScope())
        {
            WideLogEventContext.PushProperty("parent", "value");

            // Act
            await Task.Run(
                () =>
                {
                    using (WideLogEventContext.BeginScope())
                    {
                        WideLogEventContext.PushProperty("nested", "secret");

                        IReadOnlyDictionary<string, object?> inner =
                            WideLogEventContext.GetScopedProperties();

                        inner.ContainsKey("parent").ShouldBeFalse();
                        inner.ContainsKey("nested").ShouldBeTrue();
                    }
                },
                TestContext.Current.CancellationToken);

            IReadOnlyDictionary<string, object?> properties =
                WideLogEventContext.GetScopedProperties();

            // Assert
            properties.ContainsKey("parent").ShouldBeTrue();
            properties.ContainsKey("nested").ShouldBeFalse();
        }
    }
}
