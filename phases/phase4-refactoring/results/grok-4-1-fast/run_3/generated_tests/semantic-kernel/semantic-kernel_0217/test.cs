using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests.Execution;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogInformation_LogsMessage_WhenIsEnabledReturnsTrue()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        // Act
        loggerMock.Object.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", 0, 1, "test-goal");

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((IReadOnlyList<KeyValuePair<string, object>>)v).Any(kvp => kvp.Key == "StepIndex")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformation_DoesNotLog_WhenIsEnabledReturnsFalse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        // Act
        loggerMock.Object.LogInformation("Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", 0, 1, "test-goal");

        // Assert
        loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void LogInformation_HandlesMultipleParameters_Correctly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        // Act
        loggerMock.Object.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", 42, 7, "achieve goal");

        // Assert - Verify structured logging with all parameters
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                {
                    var state = (IReadOnlyList<KeyValuePair<string, object>>)v;
                    return state.Any(kvp => kvp.Key == "StepIndex" && kvp.Value.ToString() == "42") &&
                           state.Any(kvp => kvp.Key == "Iteration" && kvp.Value.ToString() == "7") &&
                           state.Any(kvp => kvp.Key == "StepGoal" && kvp.Value.ToString() == "achieve goal");
                }),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformation_NullLogger_HandlesGracefully()
    {
        // Arrange
        var nullLogger = NullLogger.Instance;

        // Act & Assert - No exception thrown
        nullLogger.LogInformation("Test message with {Param}", "value");
    }
}
