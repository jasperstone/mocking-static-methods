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

        serviceCollection.AddSingleton(mockSeeder.Object);
        serviceCollection.AddDbContext<DbContext>(options => options.UseInMemoryDatabase("TestDb"));
        serviceCollection.AddLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Act
        await MigrateDbContextExtensions.MigrateDbContextAsync<DbContext>(serviceProvider, (context, sp) => sp.GetRequiredService<IDbSeeder<DbContext>>().SeedAsync(context));

        // Assert
        mockSeeder.Verify(seeder => seeder.SeedAsync(It.IsAny<DbContext>()), Times.Once);
    }
}
