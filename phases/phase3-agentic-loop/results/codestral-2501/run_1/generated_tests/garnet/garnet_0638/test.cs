using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using System;

namespace Tsavorite.Tests
{
    public class IndexRecoveryWrapper : TsavoriteBase
    {
        public new void AsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
        {
            base.AsyncPageReadCallback(errorCode, numBytes, overlap);
        }

        public new ILogger<TsavoriteBase> logger
        {
            get => base.logger;
            set => base.logger = value;
        }
    }

    public class IndexRecoveryTests
    {
        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TsavoriteBase>>();
            var indexRecovery = new IndexRecoveryWrapper
            {
                logger = mockLogger.Object
            };

            uint errorCode = 1;
            uint numBytes = 1024;
            object overlap = new HashIndexPageAsyncReadResult();

            // Act
            indexRecovery.AsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
