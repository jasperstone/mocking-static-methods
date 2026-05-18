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
    public void AddMigration_WithSeeder_ShouldAddHostedService()
    {
        // Arrange
        var services = new ServiceCollection();
        Func<DbContext, IServiceProvider, Task> seeder = (context, sp) => Task.CompletedTask;

        // Act
        var result = MigrateDbContextExtensions.AddMigration(services, seeder);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedService = serviceProvider.GetService<IHostedService>();
        Assert.NotNull(hostedService);
    }

    [Fact]
    public void AddMigration_WithSeeder_ShouldAddOpenTelemetry()
    {
        // Arrange
        var services = new ServiceCollection();
        Func<DbContext, IServiceProvider, Task> seeder = (context, sp) => Task.CompletedTask;

        // Act
        var result = MigrateDbContextExtensions.AddMigration(services, seeder);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var openTelemetry = serviceProvider.GetService<OpenTelemetry>();
        Assert.NotNull(openTelemetry);
    }

    [Fact]
    public async Task MigrateDbContextAsync_ShouldLogInformation()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger<DbContext>>();
        var dbContextMock = new Mock<DbContext>();
        var executionStrategyMock = new Mock<IDbContextExecutionStrategy>();

        services.AddSingleton(loggerMock.Object);
        services.AddSingleton(dbContextMock.Object);
        services.AddSingleton(executionStrategyMock.Object);

        var serviceProvider = services.BuildServiceProvider();
        Func<DbContext, IServiceProvider, Task> seeder = (context, sp) => Task.CompletedTask;

        // Act
        await MigrateDbContextExtensions.MigrateDbContextAsync(serviceProvider, seeder);

        // Assert
        loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
