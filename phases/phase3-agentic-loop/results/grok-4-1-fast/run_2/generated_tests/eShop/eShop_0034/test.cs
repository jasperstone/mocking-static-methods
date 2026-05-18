using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting;

public class MigrateDbContextExtensionsTests
{
    [Fact]
    public async Task MigrateDbContextAsync_WhenMigrationThrowsException_LogsErrorAndRethrows()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger<MockDbContext>>();
        var contextMock = new Mock<MockDbContext>();
        
        var strategyMock = new Mock<IExecutionStrategy>();
        strategyMock.Setup(s => s.ExecuteAsync(It.IsAny<Func<Task>>()))
                   .ThrowsAsync(new InvalidOperationException("Migration failed"));
        contextMock.Setup(c => c.Database.CreateExecutionStrategy()).Returns(strategyMock.Object);
        
        services.AddLogging();
        services.AddSingleton(loggerMock.Object);
        services.AddSingleton(contextMock.Object);
        
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => serviceProvider.MigrateDbContextAsync(async (_, __) => Task.CompletedTask));

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while migrating the database used on context MockDbContext")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class MockDbContext : DbContext
    {
        public MockDbContext(DbContextOptions<MockDbContext> options) : base(options) { }
    }
}
