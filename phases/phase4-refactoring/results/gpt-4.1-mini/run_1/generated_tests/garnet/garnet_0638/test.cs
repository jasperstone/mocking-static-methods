using System;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Xunit;
using Moq;

namespace Tsavorite.Tests
{
    public class TsavoriteBaseTests
    {
        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var tsavorite = new TestableTsavoriteBase();
            var loggerMock = new Mock<ILogger>();
            tsavorite.SetLogger(loggerMock.Object);

            uint errorCode = 123;
            uint numBytes = 0;
            object overlap = null;

            // Act
            tsavorite.InvokeAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncPageReadCallback error:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var tsavorite = new TestableTsavoriteBase();
            var loggerMock = new Mock<ILogger>();
            tsavorite.SetLogger(loggerMock.Object);

            uint errorCode = 0;
            uint numBytes = 0;
            object overlap = null;

            // Act
            tsavorite.InvokeAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        // Helper subclass to expose AsyncPageReadCallback for testing and allow logger injection
        private class TestableTsavoriteBase : TsavoriteBase
        {
            public void SetLogger(ILogger logger)
            {
                this.logger = logger;
            }

            public void InvokeAsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
            {
                // Call the private method via reflection since it's private
                var method = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null) throw new InvalidOperationException("AsyncPageReadCallback method not found");
                method.Invoke(this, new object[] { errorCode, numBytes, overlap });
            }
        }
    }
}
