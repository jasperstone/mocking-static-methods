using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class RecoveryTests
    {
        [Fact]
        public void LogInformation_Called_When_Recovery_Called_On_Non_Empty_Log()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Recovery>>();
            var recovery = new Recovery(loggerMock.Object);

            // Act
            recovery.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
