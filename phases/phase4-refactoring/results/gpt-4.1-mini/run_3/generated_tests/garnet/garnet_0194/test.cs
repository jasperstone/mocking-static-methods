using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_ExtensionMethod_CalledWithExpectedMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            // Act
            mockLogger.Object.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AcquireCheckpointEntry iteration 0")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
