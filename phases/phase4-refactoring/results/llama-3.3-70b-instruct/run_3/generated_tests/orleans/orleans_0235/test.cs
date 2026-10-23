using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.TestKit.Base.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        [Fact]
        public void AfterStore_InjectAfterStoreTrue_LogsAndThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectAfterStore = true;

            // Act and Assert
            Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<object, Exception, string>)((state, exception) => "")), Times.Once);
        }

        [Fact]
        public void BeforeStore_InjectBeforeStoreTrue_LogsAndThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectBeforeStore = true;

            // Act and Assert
            Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<object, Exception, string>)((state, exception) => "")), Times.Once);
        }

        [Fact]
        public void AfterStore_InjectAfterStoreFalse_DoesNotLogOrThrowException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectAfterStore = false;

            // Act
            injector.AfterStore();

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<object, Exception, string>)((state, exception) => "")), Times.Never);
        }

        [Fact]
        public void BeforeStore_InjectBeforeStoreFalse_DoesNotLogOrThrowException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectBeforeStore = false;

            // Act
            injector.BeforeStore();

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<object, Exception, string>)((state, exception) => "")), Times.Never);
        }
    }
}
