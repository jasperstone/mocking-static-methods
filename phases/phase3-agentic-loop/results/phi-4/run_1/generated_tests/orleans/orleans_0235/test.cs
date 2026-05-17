using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

public class SimpleAzureStorageExceptionInjectorTests
{
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

        // Verify the log message using a lambda expression
        loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Storage exception thrown after store, thrown total 1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );

        // Verify the exception message
        Assert.Equal("Storage exception thrown after store, thrown total 1", exception.Message);
    }
}
