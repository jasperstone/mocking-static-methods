using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

public class SimpleAzureStorageExceptionInjectorTests
{
    [Fact]
    public void AfterStore_ShouldLogInformation_WhenInjectAfterStoreIsTrue()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
        var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object)
        {
            InjectAfterStore = true
        };

        // Act
        var exception = Record.Exception(() => injector.AfterStore());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<SimpleAzureStorageException>(exception);

        loggerMock.Verify(
            x => x.LogInformation(It.Is<string>(s => s.Contains("Storage exception thrown after store"))),
            Times.Once);
    }
}
