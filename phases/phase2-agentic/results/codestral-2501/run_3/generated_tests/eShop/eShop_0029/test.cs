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
        mockSeeder.Setup(seeder => seeder.SeedAsync(It.IsAny<DbContext>())).Returns(Task.CompletedTask);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IDbSeeder<DbContext>))).Returns(mockSeeder.Object);

        serviceCollection.AddSingleton(mockServiceProvider.Object);

        // Act
        serviceCollection.AddMigration<DbContext, IDbSeeder<DbContext>>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostedService = serviceProvider.GetRequiredService<IHostedService>();

        await hostedService.StartAsync(CancellationToken.None);

        // Assert
        mockSeeder.Verify(seeder => seeder.SeedAsync(It.IsAny<DbContext>()), Times.Once);
    }
}
