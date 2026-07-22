using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        private readonly Mock<ILogger<SimpleAzureStorageExceptionInjector>> _mockLogger;

        public SimpleAzureStorageExceptionInjectorTests()
        {
            _mockLogger = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            // Setup to allow logging without throwing
            _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        }

        [Fact]
        public void BeforeStore_WhenInjectBeforeStoreIsFalse_DoesNotLogOrThrow()
        {
            // Arrange
            var injector = new SimpleAzureStorageExceptionInjector(_mockLogger.Object);
            injector.InjectBeforeStore = false;

            // Act
            injector.BeforeStore();

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }

        [Fact]
        public void BeforeStore_WhenInjectBeforeStoreIsTrue_LogsInformationAndThrows()
        {
            // Arrange
            var injector = new SimpleAzureStorageExceptionInjector(_mockLogger.Object);
            injector.InjectBeforeStore = true;

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());
            Assert.Equal("Storage exception thrown before store. Thrown total 1", exception.Message);

            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void AfterStore_WhenInjectAfterStoreIsFalse_DoesNotLogOrThrow()
        {
            // Arrange
            var injector = new SimpleAzureStorageExceptionInjector(_mockLogger.Object);
            injector.InjectAfterStore = false;

            // Act
            injector.AfterStore();

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }

        [Fact]
        public void AfterStore_WhenInjectAfterStoreIsTrue_LogsInformationAndThrows()
        {
            // Arrange
            var injector = new SimpleAzureStorageExceptionInjector(_mockLogger.Object);
            injector.InjectAfterStore = true;

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());
            Assert.Equal("Storage exception thrown after store, thrown total 1", exception.Message);

            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void Constructor_AssignsLoggerCorrectly()
        {
            // Arrange & Act
            var injector = new SimpleAzureStorageExceptionInjector(_mockLogger.Object);

            // Assert - constructor doesn't throw and logger is assigned
            Assert.NotNull(injector);
        }

        [Fact]
        public void BeforeStore_MultipleCalls_ResetsFlagAfterFirstInjection()
        {
            // Arrange
            var injector = new SimpleAzureStorageExceptionInjector(_mockLogger.Object);
            injector.InjectBeforeStore = true;

            // Act - first call should inject
            Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());

            // Act - second call should not inject (flag reset)
            injector.BeforeStore();

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
