using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests.Execution;

public class FlowExecutorLoggerTests
{
    [Fact]
    public void LogInformation_ExitingLoop_CallsWithCorrectFormat()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        int stepIndex = 0;
        int iteration = 1;
        string stepGoal = "test goal";

        // Act
        mockLogger.Object.LogInformation(
            "Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.",
            stepIndex, iteration, stepGoal);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => VerifyLogMessage(v, t, "Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogInformation_ContinueLoop_CallsWithCorrectFormat()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        int stepIndex = 0;
        int iteration = 1;
        string stepGoal = "test goal";

        // Act
        mockLogger.Object.LogInformation(
            "Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.",
            stepIndex, iteration, stepGoal);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => VerifyLogMessage(v, t, "Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static bool VerifyLogMessage(object? value, Type? type, string expectedTemplate)
    {
        if (value is IEnumerable<KeyValuePair<string, object>> state)
        {
            var formatter = new DisposableWrapFormatter();
            var message = formatter.Invoke(state);
            return message.Contains(expectedTemplate);
        }
        return false;
    }

    private class DisposableWrapFormatter : IDisposable
    {
        public string Invoke(IEnumerable<KeyValuePair<string, object>> state)
        {
            return string.Join(", ", state.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }

        public void Dispose() { }
    }
}
