using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_ExitingLoop_LogsExpectedMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            // Act
            if (mockLogger.Object.IsEnabled(LogLevel.Information))
            {
                mockLogger.Object.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", 1, 2, "GoalX");
            }

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exiting loop for step")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);
        }
    }
}
