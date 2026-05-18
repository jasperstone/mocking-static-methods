using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class IndexRecoveryTests
    {
        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TsavoriteBase>>();
            var tsavorite = new FakeTsavoriteBase();
            tsavorite.SetLogger(loggerMock.Object);

            // Act
            unsafe
            {
                tsavorite.CallAsyncPageReadCallback(123, 0, null);
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncPageReadCallback error: {errorCode}") && v.ToString().Contains("123")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TsavoriteBase>>();
            var tsavorite = new FakeTsavoriteBase();
            tsavorite.SetLogger(loggerMock.Object);

            // Act
            unsafe
            {
                tsavorite.CallAsyncPageReadCallback(0, 0, null);
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void AsyncPageReadCallback_NoLogger_NoErrorThrown()
        {
            // Arrange
            var tsavorite = new FakeTsavoriteBase();
            tsavorite.SetLogger(null);

            // Act
            unsafe
            {
                tsavorite.CallAsyncPageReadCallback(123, 0, null);
            }

            // Assert - no exception thrown
            Assert.True(true);
        }
    }

    // Fake for testability - makes protected members accessible
    internal class FakeTsavoriteBase : TsavoriteBase
    {
        public void SetLogger(ILogger<TsavoriteBase>? logger)
        {
            this.logger = logger;
        }

        public unsafe void CallAsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
        {
            AsyncPageReadCallback(errorCode, numBytes, overlap);
        }
    }
}
