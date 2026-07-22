using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.TestKit.Base.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        [Fact]
        public void AfterStore_InjectAfterStoreTrue_ThrowsSimpleAzureStorageException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectAfterStore = true;

            // Act and Assert
            Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());
        }

        [Fact]
        public void BeforeStore_InjectBeforeStoreTrue_ThrowsSimpleAzureStorageException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectBeforeStore = true;

            // Act and Assert
            Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());
        }

        [Fact]
        public void AfterStore_InjectAfterStoreFalse_DoesNotThrow()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectAfterStore = false;

            // Act and Assert
            injector.AfterStore();
        }

        [Fact]
        public void BeforeStore_InjectBeforeStoreFalse_DoesNotThrow()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectBeforeStore = false;

            // Act and Assert
            injector.BeforeStore();
        }

        [Fact]
        public void AfterStore_InjectAfterStoreTrue_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectAfterStore = true;

            // Act
            try
            {
                injector.AfterStore();
            }
            catch (SimpleAzureStorageException)
            {
            }

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((v, e) => $"Storage exception thrown after store, thrown total {injector.InjectionAfterStoreCounter}")),
                Times.Once);
        }

        [Fact]
        public void BeforeStore_InjectBeforeStoreTrue_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object);
            injector.InjectBeforeStore = true;

            // Act
            try
            {
                injector.BeforeStore();
            }
            catch (SimpleAzureStorageException)
            {
            }

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((v, e) => $"Storage exception thrown before store. Thrown total {injector.InjectionBeforeStoreCounter}")),
                Times.Once);
        }
    }
}
