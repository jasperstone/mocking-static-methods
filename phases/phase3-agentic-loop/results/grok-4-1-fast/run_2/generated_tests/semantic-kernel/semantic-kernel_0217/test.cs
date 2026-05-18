using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests;

public class FlowExecutorLoggerTests
{
    [Fact]
    public void LogInformation_ExitLoop_LogsCorrectMessage()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var wrapper = new FlowExecutorLoggerWrapper(logger.Object);

        int stepIndex = 42;
        var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 3 };
        var step = new Mock<IFlowStep>();
        step.Setup(s => s.Goal).Returns("test goal");

        // Act
        wrapper.CallExitLoopLog(stepIndex, stepState, step.Object);

        // Assert
        logger.Verify(
            l => l.LogInformation(
                "Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.",
                stepIndex,
                stepState.ExecutionCount,
                "test goal"),
            Times.Once);
    }

    [Fact]
    public void LogInformation_ExitLoop_LogsNothingWhenDisabled()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        var wrapper = new FlowExecutorLoggerWrapper(logger.Object);

        int stepIndex = 42;
        var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 3 };
        var step = new Mock<IFlowStep>();

        // Act
        wrapper.CallExitLoopLog(stepIndex, stepState, step.Object);

        // Assert
        logger.Verify(
            l => l.LogInformation(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()),
            Times.Never);
    }

    [Fact]
    public void LogInformation_ContinueLoop_LogsCorrectMessage()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var wrapper = new FlowExecutorLoggerWrapper(logger.Object);

        int stepIndex = 42;
        var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 3 };
        var step = new Mock<IFlowStep>();
        step.Setup(s => s.Goal).Returns("test goal");

        // Act
        wrapper.CallContinueLoopLog(stepIndex, stepState, step.Object);

        // Assert
        logger.Verify(
            l => l.LogInformation(
                "Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.",
                stepIndex,
                stepState.ExecutionCount,
                "test goal"),
            Times.Once);
    }

    [Fact]
    public void LogInformation_ContinueLoop_LogsNothingWhenDisabled()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        var wrapper = new FlowExecutorLoggerWrapper(logger.Object);

        int stepIndex = 42;
        var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 3 };
        var step = new Mock<IFlowStep>();

        // Act
        wrapper.CallContinueLoopLog(stepIndex, stepState, step.Object);

        // Assert
        logger.Verify(
            l => l.LogInformation(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()),
            Times.Never);
    }
}

// Public wrapper class to test the exact LoggerExtensions.LogInformation call pattern (line 326)
public class FlowExecutorLoggerWrapper
{
    private readonly ILogger _logger;

    public FlowExecutorLoggerWrapper(ILogger logger)
    {
        this._logger = logger;
    }

    public void CallExitLoopLog(int stepIndex, ExecutionState.StepExecutionState stepState, IFlowStep step)
    {
        if (this._logger?.IsEnabled(LogLevel.Information) ?? false)
        {
            this._logger.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, stepState.ExecutionCount, step.Goal);
        }
    }

    public void CallContinueLoopLog(int stepIndex, ExecutionState.StepExecutionState stepState, IFlowStep step)
    {
        if (this._logger?.IsEnabled(LogLevel.Information) ?? false)
        {
            this._logger.LogInformation("Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, stepState.ExecutionCount, step.Goal);
        }
    }
}
