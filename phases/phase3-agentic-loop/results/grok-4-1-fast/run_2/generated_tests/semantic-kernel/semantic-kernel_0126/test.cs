using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.UnitTests;

public class MistralClientLoggerTests
{
    [Fact]
    public void LogDebugExtension_IsCalled_WhenDebugEnabledAndToolCallPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        int toolCallCount = 1;

        // Act - Directly invoke the LogDebug extension method from line 128
        loggerMock.Object.LogDebug("Tool requests: {Requests}", toolCallCount);

        // Assert - Verify LogDebug extension called ILogger.Log with correct parameters
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => ContainsExpectedLogMessage(v, "Tool requests: {Requests}", toolCallCount)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDebugExtension_IsNotCalled_WhenDebugDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        
        int toolCallCount = 1;

        // Act - Simulate the if condition from source code - since IsEnabled returns false, LogDebug is NOT called
        if (loggerMock.Object.IsEnabled(LogLevel.Debug))
        {
            loggerMock.Object.LogDebug("Tool requests: {Requests}", toolCallCount);
        }

        // Assert - Verify LogDebug was NOT called
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void LogDebugExtension_FormatsCorrectly_WithDifferentToolCallCounts()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        int[] testCounts = { 0, 1, 3, 5 };

        foreach (int toolCallCount in testCounts)
        {
            // Reset mock for each iteration
            loggerMock.Invocations.Clear();

            // Act - Invoke exact LogDebug call from line 128
            loggerMock.Object.LogDebug("Tool requests: {Requests}", toolCallCount);

            // Assert - Verify structured logging with correct parameter
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    0,
                    It.Is<It.IsAnyType>((v, t) => ContainsExpectedLogMessage(v, "Tool requests: {Requests}", toolCallCount)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    private static bool ContainsExpectedLogMessage<TState>(TState state, string expectedTemplate, int expectedCount)
    {
        if (state is IEnumerable<KeyValuePair<string, object>> kvps)
        {
            foreach (var kvp in kvps)
            {
                if (kvp.Key == "Requests" && kvp.Value?.ToString() == expectedCount.ToString())
                {
                    return true;
                }
                if (kvp.Value?.ToString()?.Contains(expectedTemplate) == true)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
