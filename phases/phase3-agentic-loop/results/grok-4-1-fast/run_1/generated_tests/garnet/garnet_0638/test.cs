using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Tsavorite.core;
using System.Reflection;

namespace Tsavorite.Tests
{
    public class IndexRecoveryTests
    {
        private static readonly Mock<ILogger<TsavoriteBase>> loggerMock = new();
        
        [Fact]
        public unsafe void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
            var tsavorite = CreateTsavoriteWithLogger();
            
            uint errorCode = 123;
            uint numBytes = 0;
            object overlap = new();

            // Act
            tsavorite.AsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v?.ToString().Contains($"AsyncPageReadCallback error: {errorCode}") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public unsafe void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
            var tsavorite = CreateTsavoriteWithLogger();
            
            uint errorCode = 0;
            uint numBytes = 1024;
            object overlap = new();

            // Act
            tsavorite.AsyncPageReadCallback(errorCode, numBytes, overlap);

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
        public unsafe void AsyncPageReadCallback_DoesNotLog_WhenLoggerIsNull()
        {
            // Arrange
            var tsavorite = new Mock<TsavoriteBase>() { CallBase = true }.Object;
            var loggerField = typeof(TsavoriteBase).GetField("logger", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField?.SetValue(tsavorite, null);
            
            uint errorCode = 123;
            uint numBytes = 0;
            object overlap = new();

            // Act
            tsavorite.AsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert - no exception thrown, no log call
            Assert.True(true);
        }

        private TsavoriteBase CreateTsavoriteWithLogger()
        {
            var tsavorite = new Mock<TsavoriteBase>() { CallBase = true }.Object;
            var loggerField = typeof(TsavoriteBase).GetField("logger", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField?.SetValue(tsavorite, loggerMock.Object);
            return tsavorite;
        }
    }
}
