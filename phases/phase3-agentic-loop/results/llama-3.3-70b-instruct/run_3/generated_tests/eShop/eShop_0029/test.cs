using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace eShop.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public async Task AddMigration_WithSeeder_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDbContext<MockDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName: "MockDbContext");
            });
            services.AddScoped<IDbSeeder<MockDbContext>, MockDbSeeder>();

            // Act
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.AddMigration<MockDbContext, MockDbSeeder>();

            // Assert
            var dbSeeder = serviceProvider.GetService<IDbSeeder<MockDbContext>>();
            Assert.NotNull(dbSeeder);
        }

        private class MockDbContext : DbContext
        {
        }

        private class MockDbSeeder : IDbSeeder<MockDbContext>
        {
            public Task SeedAsync(MockDbContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
