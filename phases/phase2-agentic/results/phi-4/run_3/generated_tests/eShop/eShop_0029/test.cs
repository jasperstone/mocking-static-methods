using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public void AddMigration_WithDbSeeder_RegistersCorrectSeeder()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLogger = new Mock<ILogger<MockDbContext>>();
            var mockSeeder = new Mock<IDbSeeder<MockDbContext>>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<ILogger<MockDbContext>>())
                .Returns(mockLogger.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IDbSeeder<MockDbContext>>())
                .Returns(mockSeeder.Object);

            services.AddSingleton(mockServiceProvider.Object);

            // Act
            services.AddMigration<MockDbContext, MockDbSeeder>();

            var serviceProvider = services.BuildServiceProvider();
            var migrationHostedService = serviceProvider.GetRequiredService<MigrationHostedService<MockDbContext>>();

            // Assert
            Assert.NotNull(migrationHostedService);
            mockSeeder.Verify(seeder => seeder.SeedAsync(It.IsAny<MockDbContext>(), It.IsAny<IServiceProvider>()), Times.Once);
        }

        private class MockDbContext : DbContext
        {
        }

        private class MockDbSeeder : IDbSeeder<MockDbContext>
        {
            public Task SeedAsync(MockDbContext context, IServiceProvider serviceProvider)
            {
                return Task.CompletedTask;
            }
        }
    }
}
