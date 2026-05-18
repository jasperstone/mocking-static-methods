using System;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        private readonly Mock<ILogger<SimpleAzureStorageExceptionInjector>> _loggerMock;
        private readonly SimpleAzureStorageExceptionInjector _injector;

        public SimpleAzureStorageExceptionInjectorTests()
        {
            _loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            _injector = new SimpleAzureStorageExceptionInjector(_loggerMock.Object);
        }

        [Fact]
        public void AfterStore_InjectAfterStoreTrue_LogsInformationAndThrowsException()
        {
            // Arrange
            _injector.InjectAfterStore = true;

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => _injector.AfterStore());
            Assert.Contains("Storage exception thrown after store, thrown total 1", exception.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Storage exception thrown after store, thrown total 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            
            Assert.False(_injector.InjectAfterStore);
        }

        [Fact]
        public void BeforeStore_InjectBeforeStoreTrue_LogsInformationAndThrowsException()
        {
            // Arrange
            _injector.InjectBeforeStore = true;

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => _injector.BeforeStore());
            Assert.Contains("Storage exception thrown before store. Thrown total 1", exception.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Storage exception thrown before store. Thrown total 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            
            Assert.False(_injector.InjectBeforeStore);
        }

        [Fact]
        public void AfterStore_InjectAfterStoreFalse_NoLogNoException()
        {
            // Arrange
            _injector.InjectAfterStore = false;

            // Act
            _injector.AfterStore();

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
        public void BeforeStore_InjectBeforeStoreFalse_NoLogNoException()
        {
            // Arrange
            _injector.InjectBeforeStore = false;

            // Act
            _injector.BeforeStore();

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
            _injector.InjectAfterStore = true;
            _loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                      .Verifiable();

            // Act & Assert - first injection
            Assert.Throws<SimpleAzureStorageException>(() => _injector.AfterStore());
            _loggerMock.VerifyAll();

            // Reset for second injection
            _injector.InjectAfterStore = true;

            // Act & Assert - second injection
            var exception2 = Assert.Throws<SimpleAzureStorageException>(() => _injector.AfterStore());
            Assert.Contains("Storage exception thrown after store, thrown total 2", exception2.Message);
        }
    }
}
