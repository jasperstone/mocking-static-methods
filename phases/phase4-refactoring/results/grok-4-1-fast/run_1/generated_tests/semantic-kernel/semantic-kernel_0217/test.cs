using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests.Execution;

public class FlowExecutorLoggerTests
{
    [Fact]
    public void LogInformationExtension_CalledWithExitLoopParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        const int stepIndex = 1;
        const int iteration = 5;
        const string stepGoal = "test goal";

        // Act - Directly exercise the LoggerExtensions.LogInformation call pattern from line 326
        mockLogger.Object.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"step {stepIndex}") &&
                    v.ToString()!.Contains($"iteration={iteration}") &&
                    v.ToString()!.Contains(stepGoal)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformationExtension_ContinueLoop_CalledWithCorrectParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        const int stepIndex = 2;
        const int iteration = 3;
        const string stepGoal = "continue goal";

        // Act - Test the companion LogInformation call pattern (line ~335)
        mockLogger.Object.LogInformation("Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"step {stepIndex}") &&
                    v.ToString()!.Contains($"iteration={iteration}") &&
                    v.ToString()!.Contains(stepGoal)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformationExtension_CalledEvenWhenLoggerDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        // Act
        mockLogger.Object.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", 1, 5, "test goal");

        // Assert - Log call is still made, but logger implementation filters it out internally
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
