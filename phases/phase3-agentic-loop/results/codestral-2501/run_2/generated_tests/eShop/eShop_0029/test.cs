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
    public async Task AddMigration_WithSeeder_CallsGetRequiredService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var mockSeeder = new Mock<IDbSeeder<DbContext>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDbContext = new Mock<DbContext>();
        var mockLogger = new Mock<ILogger<DbContext>>();

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<IDbSeeder<DbContext>>())
            .Returns(mockSeeder.Object);

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<DbContext>())
            .Returns(mockDbContext.Object);

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<ILogger<DbContext>>())
            .Returns(mockLogger.Object);

        serviceCollection.AddSingleton(mockServiceProvider.Object);

        // Act
        serviceCollection.AddMigration<DbContext, IDbSeeder<DbContext>>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostedService = serviceProvider.GetRequiredService<IHostedService>();

        await hostedService.StartAsync(CancellationToken.None);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<IDbSeeder<DbContext>>(), Times.Once);
    }
}
