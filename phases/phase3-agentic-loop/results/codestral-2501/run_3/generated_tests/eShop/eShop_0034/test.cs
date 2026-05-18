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
        var serviceCollection = new ServiceCollection();
        var mockLogger = new Mock<ILogger<DbContext>>();
        var mockDbContext = new Mock<DbContext>();
        var mockSeeder = new Mock<Func<DbContext, IServiceProvider, Task>>();

        serviceCollection.AddSingleton(mockLogger.Object);
        serviceCollection.AddSingleton(mockDbContext.Object);
        serviceCollection.AddSingleton(mockSeeder.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        mockDbContext.Setup(x => x.Database.CreateExecutionStrategy()).Throws(new Exception("Test exception"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => serviceProvider.MigrateDbContextAsync(mockSeeder.Object));

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
