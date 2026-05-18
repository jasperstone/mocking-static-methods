using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class DeltaLogTests
    {
        [Fact]
        public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DeltaLog>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
            loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var mockDevice = new Mock<IDevice>();
            mockDevice.Setup(d => d.SectorSize).Returns(512u);

            // Create real DeltaLog instance
            var deltaLog = new Tsavorite.core.DeltaLog(mockDevice.Object, 12, 0L, loggerMock.Object);

            // Use reflection to set up required fields and invoke private method
            var issuedFlushField = typeof(Tsavorite.core.DeltaLog).GetField("issuedFlush", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var completedSemaphoreField = typeof(Tsavorite.core.DeltaLog).GetField("completedSemaphore", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var disposedField = typeof(Tsavorite.core.DeltaLog).GetField("disposed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            issuedFlushField?.SetValue(deltaLog, 1);
            disposedField?.SetValue(deltaLog, false);

            var callbackMethod = typeof(Tsavorite.core.DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var context = new { count = 1 }; // Mock PageAsyncFlushResult

            // Act
            callbackMethod?.Invoke(deltaLog, new object[] { 123u, 1024u, context });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>((v, t) => 
                        ((string)v).Contains("AsyncFlushPageToDeviceCallback error:") && 
                        ((string)v).Contains("123")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DeltaLog>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            var mockDevice = new Mock<IDevice>();
            mockDevice.Setup(d => d.SectorSize).Returns(512u);

            var deltaLog = new Tsavorite.core.DeltaLog(mockDevice.Object, 12, 0L, loggerMock.Object);

            var disposedField = typeof(Tsavorite.core.DeltaLog).GetField("disposed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            disposedField?.SetValue(deltaLog, false);

            var callbackMethod = typeof(Tsavorite.core.DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            callbackMethod?.Invoke(deltaLog, new object[] { 0u, 1024u, null });

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
        public void LoggerExtensions_LogError_NullLogger_DoesNotThrow()
        {
            // Arrange & Act & Assert
            Tsavorite.core.DeltaLog deltaLog = null;
            Assert.DoesNotThrow(() => deltaLog?.Logger?.LogError("test {errorCode}", 123u));
        }
    }
}
