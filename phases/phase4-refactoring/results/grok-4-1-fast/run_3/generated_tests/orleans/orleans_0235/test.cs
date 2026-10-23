using System;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        private readonly Mock<ILogger<SimpleAzureStorageExceptionInjector>> _mockLogger;
        private readonly SimpleAzureStorageExceptionInjector _injector;

        public SimpleAzureStorageExceptionInjectorTests()
        {
            _mockLogger = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            _injector = new SimpleAzureStorageExceptionInjector(_mockLogger.Object);
        }

        [Fact]
        public void BeforeStore_WhenInjectBeforeStoreIsFalse_DoesNotLogOrThrow()
        {
            // Arrange
            _injector.InjectBeforeStore = false;

            // Act
            _injector.BeforeStore();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void BeforeStore_WhenInjectBeforeStoreIsTrue_LogsInformationAndThrows()
        {
            // Arrange
            _injector.InjectBeforeStore = true;

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => _injector.BeforeStore());
            Assert.Equal("Storage exception thrown before store. Thrown total 1", exception.Message);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Storage exception thrown before store. Thrown total 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void AfterStore_WhenInjectAfterStoreIsFalse_DoesNotLogOrThrow()
        {
            // Arrange
            _injector.InjectAfterStore = false;

            // Act
            _injector.AfterStore();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void AfterStore_WhenInjectAfterStoreIsTrue_LogsInformationAndThrows()
        {
            // Arrange
            _injector.InjectAfterStore = true;

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => _injector.AfterStore());
            Assert.Equal("Storage exception thrown after store, thrown total 1", exception.Message);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Storage exception thrown after store, thrown total 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void BeforeStore_MultipleCalls_OnlyLogsOnce()
        {
            // Arrange
            _injector.InjectBeforeStore = true;

            // Act
            Assert.Throws<SimpleAzureStorageException>(() => _injector.BeforeStore()); // First call logs and throws
            _injector.BeforeStore(); // Second call does nothing

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void AfterStore_MultipleCalls_OnlyLogsOnce()
        {
            // Arrange
            _injector.InjectAfterStore = true;

            // Act
            Assert.Throws<SimpleAzureStorageException>(() => _injector.AfterStore()); // First call logs and throws
            _injector.AfterStore(); // Second call does nothing

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
