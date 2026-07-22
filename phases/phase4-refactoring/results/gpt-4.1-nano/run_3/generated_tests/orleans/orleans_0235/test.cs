using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        [Fact]
        public void AfterStore_ShouldLogInformationAndThrow_WhenInjected()
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

            // Verify that LogInformation was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Storage exception thrown after store, thrown total 1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
