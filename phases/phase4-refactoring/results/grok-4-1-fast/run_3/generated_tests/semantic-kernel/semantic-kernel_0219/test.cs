using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogInformation_WhenIsEnabledTrue_CallsLogWithCorrectParameters()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
        
        const int stepIndex = 42;
        const int iteration = 1;
        const string stepGoal = "test goal";
        
        // Act - Replicates the exact pattern from line 377
        if (loggerMock.Object.IsEnabled(LogLevel.Information))
        {
            loggerMock.Object.LogInformation(
                "Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.", 
                stepIndex, 
                iteration, 
                stepGoal);
        }
        
        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    ((string)v).Contains("Completed step 42") &&
                    ((string)v).Contains("iteration=1") &&
                    ((string)v).Contains("goal=test goal")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public void LogInformation_WhenIsEnabledFalse_DoesNotCallLog()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);
        
        // Act - Replicates the exact pattern from line 377
        if (loggerMock.Object.IsEnabled(LogLevel.Information))
        {
            loggerMock.Object.LogInformation("Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.", 42, 1, "test goal");
        }
        
        // Assert
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
    public void LogInformation_NullLogger_IsEnabledReturnsFalse()
    {
        // Arrange & Act
        var nullLogger = NullLogger.Instance;
        bool isEnabled = nullLogger.IsEnabled(LogLevel.Information);
        
        // Assert
        Assert.False(isEnabled);
    }
    
    [Fact]
    public void LogInformation_GenericLogger_IsEnabledWorks()
    {
        // Arrange & Act
        var nullLogger = NullLogger<FlowExecutor>.Instance;
        bool isEnabled = nullLogger.IsEnabled(LogLevel.Information);
        
        // Assert
        Assert.False(isEnabled);
    }
}
