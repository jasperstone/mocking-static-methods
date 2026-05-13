using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class IndexRecoveryTests
    {
        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recovery = new TsavoriteBase
            {
                logger = loggerMock.Object
            };

            // Act
            recovery.AsyncPageReadCallback(1, 0, null);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains(nameof(TsavoriteBase.AsyncPageReadCallback) + " error:")),
                    It.Is<int>(errorCode => errorCode == 1)
                ),
                Times.Once
            );
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recovery = new TsavoriteBase
            {
                logger = loggerMock.Object
            };

            // Act
            recovery.AsyncPageReadCallback(0, 0, null);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<int>()
                ),
                Times.Never
            );
        }
    }
}
