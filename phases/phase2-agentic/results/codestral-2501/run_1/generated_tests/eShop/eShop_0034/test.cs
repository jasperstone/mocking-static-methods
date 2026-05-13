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

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(scope => scope.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock
            .Setup(factory => factory.CreateScope())
            .Returns(scopeMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);

        var exception = new Exception("Test exception");
        contextMock.Setup(c => c.Database.CreateExecutionStrategy().ExecuteAsync(It.IsAny<Func<Task>>()))
            .ThrowsAsync(exception);

        // Act
        var task = MigrateDbContextExtensions.MigrateDbContextAsync(serviceProviderMock.Object, seederMock.Object);

        // Assert
        await Assert.ThrowsAsync<Exception>(() => task);
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
