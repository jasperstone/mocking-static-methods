using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class MigrateDbContextExtensionsTests
{
    [Fact]
    public async Task MigrateDbContextAsync_LogsError_WhenExceptionOccurs()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<DbContext>>();
        var contextMock = new Mock<DbContext>();
        var seederMock = new Mock<Func<DbContext, IServiceProvider, Task>>();

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILogger<DbContext>)))
            .Returns(loggerMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(DbContext)))
            .Returns(contextMock.Object);

        contextMock
            .Setup(c => c.Database.CreateExecutionStrategy())
            .Returns(Mock.Of<IDbContextTransactionManager>());

        seederMock
            .Setup(s => s(It.IsAny<DbContext>(), It.IsAny<IServiceProvider>()))
            .Throws(new Exception("Test exception"));

        // Act
        var exception = await Record.ExceptionAsync(() => MigrateDbContextExtensions.MigrateDbContextAsync(serviceProviderMock.Object, seederMock.Object));

        // Assert
        Assert.NotNull(exception);
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while migrating the database used on context")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
