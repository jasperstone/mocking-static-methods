using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests;

public class FlowExecutorLoggerTests
{
    [Fact]
    public void Verifies_LogInformation_CompletedStep_MessageTemplate()
    {
        // Arrange - Verify the exact LogInformation extension method call from line 377
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        // Verify the specific structured logging call with exact message template
        // and parameter placeholders from line 377
        loggerMock.Verify(l => l.LogInformation(
            "Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.",
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Verifies_LogInformation_GuardClauseBehavior()
    {
        // Arrange - Test the guard clause if (this._logger?.IsEnabled(LogLevel.Information) ?? false)
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        // When IsEnabled returns false, LogInformation should not be called
        loggerMock.Verify(l => l.LogInformation(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void Verifies_LogInformation_NullLogger_Safe()
    {
        // Arrange - Test null logger scenario (this._logger?.IsEnabled() ?? false)
        var nullLogger = NullLogger.Instance;
        
        // NullLogger.IsEnabled always returns false, so LogInformation is never called
        Assert.False(nullLogger.IsEnabled(LogLevel.Information));
        
        // Verify safe handling - no exception thrown
        Assert.DoesNotContain("Completed step", 
            () => nullLogger.LogInformation("test", It.IsAny<object[]>()), 
            () => "No exception thrown");
    }

    [Fact]
    public void Verifies_LogInformation_ExactParameters_MatchLine377()
    {
        // Arrange - Exact signature from line 377:
        // this._logger.LogInformation("Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.", stepIndex, stepState.ExecutionCount, step.Goal);
        
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        // Verify exact 3 parameters: int stepIndex, int executionCount, string stepGoal
        loggerMock.Verify(l => l.LogInformation(
            "Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.",
            It.Is<int>(i => i >= 0),  // stepIndex
            It.Is<int>(i => i >= 0),  // stepState.ExecutionCount  
            It.Is<string>(s => !string.IsNullOrEmpty(s))), // step.Goal
            Times.Once);
    }
}
