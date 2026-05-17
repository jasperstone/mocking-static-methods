using Xunit;
using Orleans.Transactions.TestKit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        [Fact]
        public void BeforeStore_InjectBeforeStoreTrue_LogsInformationAndThrowsException()
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
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Storage exception thrown before store. Thrown total 1")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Equal("Storage exception thrown before store. Thrown total 1", exception.Message);
        }

        [Fact]
        public void AfterStore_InjectAfterStoreTrue_LogsInformationAndThrowsException()
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
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Storage exception thrown after store, thrown total 1")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Equal("Storage exception thrown after store, thrown total 1", exception.Message);
        }
    }
}
