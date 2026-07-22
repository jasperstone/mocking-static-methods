using Xunit;
using Orleans.Transactions.TestKit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        [Fact]
        public void BeforeStore_InjectsException_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object)
            {
                InjectBeforeStore = true
            };

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);

            Assert.Equal("Storage exception thrown before store. Thrown total 1", exception.Message);
        }

        [Fact]
        public void AfterStore_InjectsException_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object)
            {
                InjectAfterStore = true
            };

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);

            Assert.Equal("Storage exception thrown after store, thrown total 1", exception.Message);
        }
    }
}
