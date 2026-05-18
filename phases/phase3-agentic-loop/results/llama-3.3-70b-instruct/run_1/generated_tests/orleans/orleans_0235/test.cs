using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        [Fact]
        public void AfterStore_InjectAfterStoreTrue_LogsInformationAndThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectAfterStore = true;

            // Act and Assert
            Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<object, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void BeforeStore_InjectBeforeStoreTrue_LogsInformationAndThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectBeforeStore = true;

            // Act and Assert
            Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<object, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void AfterStore_InjectAfterStoreFalse_DoesNotLogOrThrow()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectAfterStore = false;

            // Act
            injector.AfterStore();

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<object, Exception, string>)It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void BeforeStore_InjectBeforeStoreFalse_DoesNotLogOrThrow()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectBeforeStore = false;

            // Act
            injector.BeforeStore();

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<object, Exception, string>)It.IsAny<object>()), Times.Never);
        }
    }
}
