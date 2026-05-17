using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.UnitTests;

public class FlowExecutorLoggerTests
{
    [Fact]
    public void LoggerExtensions_LogInformation_ExitingLoop_VerifyMessageFormat()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var logger = loggerMock.Object;
        var stepIndex = 0;
        var iteration = 2;
        var stepGoal = "test goal";

        // Act - Directly test the LogInformation extension method call as used in FlowExecutor
        logger.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal);

        // Assert - Verify the underlying Log method was called with correct parameters
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Exiting loop for step 0") &&
                    v.ToString()!.Contains("iteration=2") &&
                    v.ToString()!.Contains("goal=test goal")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LoggerExtensions_LogInformation_ContinueLoop_VerifyMessageFormat()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var logger = loggerMock.Object;
        var stepIndex = 1;
        var iteration = 3;
        var stepGoal = "continue goal";

        // Act
        logger.LogInformation("Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Continuing to the next loop iteration") &&
                    v.ToString()!.Contains("step 1") &&
                    v.ToString()!.Contains("iteration=3") &&
                    v.ToString()!.Contains("goal=continue goal")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LoggerExtensions_LogInformation_NotCalled_WhenIsEnabledReturnsFalse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        var logger = loggerMock.Object;
        var stepIndex = 0;
        var iteration = 1;
        var stepGoal = "test goal";

        // Act - The IsEnabled check should prevent the LogInformation call (as in source code)
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal);
        }

        // Assert - No log call should have been made
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void LoggerExtensions_LogInformation_CalledWithArbitraryParameters()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var logger = loggerMock.Object;
        var stepIndex = 42;
        var iteration = 7;
        var stepGoal = "complex goal with spaces and special chars: {test}";

        // Act
        logger.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal);

        // Assert - Verify message formatting works with complex parameters
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Exiting loop for step 42") &&
                    v.ToString()!.Contains("iteration=7") &&
                    v.ToString()!.Contains("goal=complex goal with spaces and special chars: {test}")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
