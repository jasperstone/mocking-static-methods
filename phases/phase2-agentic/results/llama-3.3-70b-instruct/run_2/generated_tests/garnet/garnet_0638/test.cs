using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace Tsavorite.core
{
    public class IndexRecoveryTests
    {
        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var indexRecovery = new TsavoriteBase();
            indexRecovery.logger = loggerMock.Object;

            // Act
            indexRecovery.AsyncPageReadCallback(1, 0, null);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var indexRecovery = new TsavoriteBase();
            indexRecovery.logger = loggerMock.Object;

            // Act
            indexRecovery.AsyncPageReadCallback(0, 0, null);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
