using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        [Fact]
        public void AfterStore_ShouldLogInformationAndThrow_WhenInjectAfterStoreIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object)
            {
                InjectAfterStore = true
            };

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());
            Assert.Contains("Storage exception thrown after store", exception.Message);
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(msg => msg.Contains("Storage exception thrown after store"))),
                Times.Once);
            Assert.False(injector.InjectAfterStore);
        }

        [Fact]
        public void BeforeStore_ShouldLogInformationAndThrow_WhenInjectBeforeStoreIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object)
            {
                InjectBeforeStore = true
            };

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());
            Assert.Contains("Storage exception thrown before store", exception.Message);
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(msg => msg.Contains("Storage exception thrown before store"))),
                Times.Once);
            Assert.False(injector.InjectBeforeStore);
        }
    }
}
