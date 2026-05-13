using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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
        var executionStrategyMock = new Mock<IDatabaseCreator>();
        var seederMock = new Mock<Func<DbContext, IServiceProvider, Task>>();

        contextMock.Setup(c => c.Database).Returns(new DatabaseFacade(executionStrategyMock.Object));
        executionStrategyMock.Setup(s => s.CreateExecutionStrategy()).Returns(new Mock<IDbContextTransaction>().Object);

        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<DbContext>))).Returns(loggerMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(contextMock.Object);

        var exception = new Exception("Test exception");

        seederMock.Setup(s => s(contextMock.Object, serviceProviderMock.Object)).ThrowsAsync(exception);

        // Act
        var task = MigrateDbContextExtensions.MigrateDbContextAsync(serviceProviderMock.Object, seederMock.Object);

        // Assert
        await Assert.ThrowsAsync<Exception>(() => task);
        loggerMock.Verify(logger => logger.LogError(exception, It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}
