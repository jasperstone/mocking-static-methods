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
        private readonly Mock<ILogger<SimpleAzureStorageExceptionInjector>> _loggerMock;

        public SimpleAzureStorageExceptionInjectorTests()
        {
            _loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
        }

        [Fact]
        public void AfterStore_WhenInjectAfterStoreIsTrue_LogsInformationAndThrowsException()
        {
            // Arrange
            var injector = new SimpleAzureStorageExceptionInjector(_loggerMock.Object);
            injector.InjectAfterStore = true;

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());
            Assert.Contains("Storage exception thrown after store, thrown total 1", exception.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Storage exception thrown after store, thrown total 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void BeforeStore_WhenInjectBeforeStoreIsTrue_LogsInformationAndThrowsException()
        {
            // Arrange
            var injector = new SimpleAzureStorageExceptionInjector(_loggerMock.Object);
            injector.InjectBeforeStore = true;

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());
            Assert.Contains("Storage exception thrown before store. Thrown total 1", exception.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Storage exception thrown before store. Thrown total 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void AfterStore_WhenInjectAfterStoreIsFalse_DoesNotLogOrThrow()
        {
            // Arrange
            var injector = new SimpleAzureStorageExceptionInjector(_loggerMock.Object);
            injector.InjectAfterStore = false;

            // Act
            injector.AfterStore();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void BeforeStore_WhenInjectBeforeStoreIsFalse_DoesNotLogOrThrow()
        {
            // Arrange
            var injector = new SimpleAzureStorageExceptionInjector(_loggerMock.Object);
            injector.InjectBeforeStore = false;

            // Act
            injector.BeforeStore();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void AfterStore_MultipleInjections_IncrementsCounterCorrectly()
        {
            // Arrange
            var injector = new SimpleAzureStorageExceptionInjector(_loggerMock.Object);
            injector.InjectAfterStore = true;

            // Act & Assert - first injection
            var ex1 = Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());
            Assert.Contains("thrown total 1", ex1.Message);

            // Reset for second injection
            injector.InjectAfterStore = true;
            var ex2 = Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());
            Assert.Contains("thrown total 2", ex2.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("thrown total 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("thrown total 2")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
