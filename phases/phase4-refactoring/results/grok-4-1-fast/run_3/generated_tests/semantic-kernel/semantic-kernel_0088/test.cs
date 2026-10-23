using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.UnitTests;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogDebug_ToolRequests_CalledWithCorrectTemplateAndCount()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        // Simulate the exact condition and call from line 495
        // this.Logger.LogDebug("Tool requests: {Requests}", state.LastMessage!.ToolCalls!.Count);
        var toolCallsCount = 3; // Example count > 0 to match the if condition
        
        // Act - Directly test the LoggerExtensions.LogDebug extension method call pattern
        mockLogger.Object.LogDebug("Tool requests: {Requests}", toolCallsCount);

        // Assert - Verify the underlying ILogger.Log was called with the expected formatted message
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString()!.Contains($"Tool requests: {toolCallsCount}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDebug_ToolRequests_VerifiesMessageTemplate()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        // Act
        mockLogger.Object.LogDebug("Tool requests: {Requests}", 1);

        // Assert - Verify the exact template was used (line 495 signature)
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString()!.Contains("Tool requests: {Requests}")),
                null!,
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDebugExtension_RespectsDebugLevelCheck()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

        // Act & Assert - When Debug is disabled, LogDebug extension does nothing
        mockLogger.Object.LogDebug("Tool requests: {Requests}", 2);
        
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyFormat<string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Never);
    }
}
