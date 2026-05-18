using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

public class MigrateDbContextExtensionsTests
{
    [Fact]
    public async Task AddMigration_ShouldCallGetRequiredService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var mockDbContext = new Mock<DbContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockSeeder = new Mock<IDbSeeder<DbContext>>();

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<IDbSeeder<DbContext>>())
            .Returns(mockSeeder.Object);

        serviceCollection.AddSingleton(mockDbContext.Object);
        serviceCollection.AddSingleton(mockServiceProvider.Object);

        // Act
        serviceCollection.AddMigration<DbContext, IDbSeeder<DbContext>>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var migrationHostedService = serviceProvider.GetRequiredService<IHostedService>();

        await migrationHostedService.StartAsync(default);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<IDbSeeder<DbContext>>(), Times.Once);
    }
}
