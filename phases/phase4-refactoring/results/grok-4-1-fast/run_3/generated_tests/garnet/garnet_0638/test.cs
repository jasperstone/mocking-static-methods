using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class IndexRecoveryTests
    {
        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            var tsavorite = new FakeTsavorite();
            tsavorite.SetLogger(mockLogger.Object);
            
            uint errorCode = 123;
            uint numBytes = 0;
            object overlap = new();

            // Act
            tsavorite.CallAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncPageReadCallback error: 123")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            var tsavorite = new FakeTsavorite();
            tsavorite.SetLogger(mockLogger.Object);
            
            uint errorCode = 0;
            uint numBytes = 456;
            object overlap = new();

            // Act
            tsavorite.CallAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLog_WhenLoggerIsNull()
        {
            // Arrange
            var tsavorite = new FakeTsavorite();
            tsavorite.SetLogger(null);
            
            uint errorCode = 123;
            uint numBytes = 0;
            object overlap = new();

            // Act
            tsavorite.CallAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert - no exception thrown
            Assert.True(true);
        }
    }

    // Test subclass that exposes the internal method via public wrapper and provides logger access
    public class FakeTsavorite : TsavoriteBase
    {
        public void SetLogger(ILogger? logger)
        {
            // Use reflection to set the private logger field
            var field = typeof(TsavoriteBase).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(this, logger);
        }

        public void CallAsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
        {
            // Public wrapper that calls the internal method
            InvokeAsyncPageReadCallback(errorCode, numBytes, overlap);
        }

        private unsafe void InvokeAsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
        {
            // Since it's unsafe, provide a safe wrapper that matches the signature
            fixed (byte* dummyPtr = new byte[1])
            {
                // Call the actual method - the unsafe pointer isn't used in the logging logic
                ((TsavoriteBase)this).AsyncPageReadCallback(errorCode, numBytes, overlap);
            }
        }
    }
}
