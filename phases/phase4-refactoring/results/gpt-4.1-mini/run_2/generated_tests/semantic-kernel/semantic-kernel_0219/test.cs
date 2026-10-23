using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_ExtensionMethod_LogsExpectedMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            // Act
            if (mockLogger.Object.IsEnabled(LogLevel.Information))
            {
                mockLogger.Object.LogInformation("Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.", 1, 2, "TestGoal");
            }

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);
        }
    }
}
