using System;
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
    public async Task AddMigration_WithSeeder_CallsGetRequiredService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var mockSeeder = new Mock<IDbSeeder<DbContext>>();
        mockSeeder.Setup(seeder => seeder.SeedAsync(It.IsAny<DbContext>())).Returns(Task.CompletedTask);

        var serviceProvider = serviceCollection
            .AddSingleton(mockSeeder.Object)
            .BuildServiceProvider();

        // Act
        serviceCollection.AddMigration<DbContext>((context, sp) => sp.GetRequiredService<IDbSeeder<DbContext>>().SeedAsync(context));
        var serviceProviderWithMigration = serviceCollection.BuildServiceProvider();

        // Assert
        mockSeeder.Verify(seeder => seeder.SeedAsync(It.IsAny<DbContext>()), Times.Once);
    }

    [Fact]
    public async Task MigrateDbContextAsync_LogsInformationAndMigratesDatabase()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var mockDbContext = new Mock<DbContext>();
        var mockLogger = new Mock<ILogger<DbContext>>();
        var mockExecutionStrategy = new Mock<IDbContextExecutionStrategy>();

        serviceCollection.AddSingleton(mockDbContext.Object);
        serviceCollection.AddSingleton(mockLogger.Object);
        serviceCollection.AddSingleton(mockExecutionStrategy.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Act
        await serviceProvider.MigrateDbContextAsync<DbContext>((context, sp) => Task.CompletedTask);

        // Assert
        mockLogger.Verify(logger => logger.Log(
            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Migrating database associated with context")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
