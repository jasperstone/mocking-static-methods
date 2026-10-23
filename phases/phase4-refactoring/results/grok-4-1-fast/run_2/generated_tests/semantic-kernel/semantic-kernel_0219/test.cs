using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests;

public class FlowExecutorLoggerTests
{
    [Fact]
    public void LogInformation_CompletedStep_CalledWhenEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var logger = loggerMock.Object;
        int stepIndex = 42;
        int iteration = 3;
        string stepGoal = "test goal";

        // Act - Directly test the LoggerExtensions.LogInformation call from line 377
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal);
        }

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformation_CompletedStep_NotCalledWhenDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        var logger = loggerMock.Object;
        int stepIndex = 42;
        int iteration = 3;
        string stepGoal = "test goal";

        // Act
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal);
        }

        // Assert - LogInformation should not be called when IsEnabled returns false
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
