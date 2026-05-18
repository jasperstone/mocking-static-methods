using System;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class IndexRecoveryTests
    {
        [Fact]
        public unsafe void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TsavoriteBase>>();
            loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("AsyncPageReadCallback error: 123") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
            
            var tsavorite = new TestableTsavoriteBase();
            tsavorite.SetLogger(loggerMock.Object);

            // Act
            tsavorite.AsyncPageReadCallback(123, 0, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("AsyncPageReadCallback error: 123") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public unsafe void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TsavoriteBase>>();
            var tsavorite = new TestableTsavoriteBase();
            tsavorite.SetLogger(loggerMock.Object);

            // Act
            tsavorite.AsyncPageReadCallback(0, 0, null);

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
    }

    // Test subclass that makes the private method public and allows logger injection
    public unsafe class TestableTsavoriteBase : TsavoriteBase
    {
        public new unsafe void AsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
        {
            base.AsyncPageReadCallback(errorCode, numBytes, overlap);
        }

        public void SetLogger(ILogger<TsavoriteBase> logger)
        {
            // Use reflection to set the private logger field
            var field = typeof(TsavoriteBase).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(this, logger);
        }
    }
}
