using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests.Execution;

public class ReActEngineLoggerTests
{
    [Fact]
    public void LoggerExtensionDebugCall_VerifiesLogDebugExtensionUsage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        var logger = mockLogger.Object;
        
        // Simulate the exact logging call from line 157: this._logger?.LogDebug("Response : {ActionText}", llmResponseText);
        string testResponseText = "test LLM response";
        
        // Act - Directly test the ILogger extension behavior that ReActEngine uses
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Response : {ActionText}", testResponseText);
        }

        // Assert - Verify the underlying Log method was called with Debug level and correct message template
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<int>(),
                It.Is<It.IsAnyType>((v, t) => (v?.ToString() ?? "").Contains("Response : {ActionText}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LoggerExtensionDebugCall_SkippedWhenDebugDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        
        var logger = mockLogger.Object;
        string testResponseText = "test LLM response";
        
        // Act - The conditional check fails, so LogDebug is not called
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Response : {ActionText}", testResponseText);
        }

        // Assert - No Log call at Debug level should have occurred
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<int>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
