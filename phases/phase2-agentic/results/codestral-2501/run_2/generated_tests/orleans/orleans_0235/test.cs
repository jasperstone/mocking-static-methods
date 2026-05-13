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

            // Act
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Storage exception thrown before store")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
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

            // Act
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Storage exception thrown after store")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.Equal("Storage exception thrown after store, thrown total 1", exception.Message);
        }
    }
}
