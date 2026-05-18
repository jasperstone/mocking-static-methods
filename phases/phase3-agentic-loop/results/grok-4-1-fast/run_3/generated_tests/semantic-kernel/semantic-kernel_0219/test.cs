using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_ThreeParameters_CallsUnderlyingLog_WhenEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            // Act - Matches exact pattern from FlowExecutor line 377
            mockLogger.Object.LogInformation(
                "Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.",
                1,      // stepIndex
                2,      // stepState.ExecutionCount
                "test goal"  // step.Goal
            );

            // Assert - Verifies the extension method works as used in FlowExecutor
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void LogInformation_ThreeParameters_NoLogCall_WhenDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

            // Act - Matches FlowExecutor guard clause pattern
            mockLogger.Object.LogInformation(
                "Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.",
                1, 2, "test goal"
            );

            // Assert - No underlying Log call when IsEnabled returns false
            mockLogger.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Never
            );
        }

        [Fact]
        public void LogInformation_MatchesFlowExecutorUsagePattern()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            // Act - Exact message and parameter types from line 377
            mockLogger.Object.LogInformation(
                "Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.",
                42,           // int stepIndex
                7,            // int stepState.ExecutionCount
                "Achieve objective"  // string step.Goal
            );

            // Assert
            mockLogger.VerifyAll();
        }
    }
}
