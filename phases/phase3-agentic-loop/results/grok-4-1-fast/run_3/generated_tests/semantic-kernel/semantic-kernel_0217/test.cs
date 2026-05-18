using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests;

public class FlowExecutorLoggerTests
{
    [Fact]
    public void LogInformation_ExitLoop_LogsCorrectMessage()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var testExecutor = new TestableFlowExecutor { TestLogger = logger.Object };

        int stepIndex = 42;
        var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 3 };
        var step = new Mock<FlowStep>();
        step.SetupGet(s => s.Goal).Returns("test goal");

        // Act
        testExecutor.CallExitLoopLog(stepIndex, stepState, step.Object);

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => HasExpectedLogFormat(v, "Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformation_ExitLoop_LogsNothingWhenDisabled()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        var testExecutor = new TestableFlowExecutor { TestLogger = logger.Object };

        int stepIndex = 42;
        var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 3 };
        var step = new Mock<FlowStep>();

        // Act
        testExecutor.CallExitLoopLog(stepIndex, stepState, step.Object);

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<int>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void LogInformation_ContinueLoop_LogsCorrectMessage()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var testExecutor = new TestableFlowExecutor { TestLogger = logger.Object };

        int stepIndex = 42;
        var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 3 };
        var step = new Mock<FlowStep>();
        step.SetupGet(s => s.Goal).Returns("test goal");

        // Act
        testExecutor.CallContinueLoopLog(stepIndex, stepState, step.Object);

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => HasExpectedLogFormat(v, "Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformation_ContinueLoop_LogsNothingWhenDisabled()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        var testExecutor = new TestableFlowExecutor { TestLogger = logger.Object };

        int stepIndex = 42;
        var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 3 };
        var step = new Mock<FlowStep>();

        // Act
        testExecutor.CallContinueLoopLog(stepIndex, stepState, step.Object);

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<int>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private static bool HasExpectedLogFormat(object state, string expectedFormat)
    {
        if (state is not IReadOnlyList<KeyValuePair<string, object>> stateItems)
        {
            return false;
        }

        var formatItem = stateItems.FirstOrDefault(x => x.Key == "{OriginalFormat}");
        return formatItem.Value?.ToString() == expectedFormat;
    }
}

internal class TestableFlowExecutor
{
    public ILogger? TestLogger { get; set; }

    public void CallExitLoopLog(int stepIndex, ExecutionState.StepExecutionState stepState, FlowStep step)
    {
        if (this.TestLogger?.IsEnabled(LogLevel.Information) ?? false)
        {
            this.TestLogger.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, stepState.ExecutionCount, step.Goal);
        }
    }

    public void CallContinueLoopLog(int stepIndex, ExecutionState.StepExecutionState stepState, FlowStep step)
    {
        if (this.TestLogger?.IsEnabled(LogLevel.Information) ?? false)
        {
            this.TestLogger.LogInformation("Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, stepState.ExecutionCount, step.Goal);
        }
    }
}
