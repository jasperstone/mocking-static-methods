using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace eShop.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public void AddMigration_WithSeeder_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockSeeder = new Mock<IDbSeeder<SampleDbContext>>();
            services.AddScoped(_ => mockSeeder.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeServiceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(s => s.ServiceProvider).Returns(scopeServiceProvider);

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockScopeFactory.Object)
                .BuildServiceProvider();

            // Act
            services.AddMigration<SampleDbContext, SampleSeeder>();

            var provider = services.BuildServiceProvider();

            // Call the extension method that internally calls GetRequiredService
            var migrateTask = provider.MigrateDbContextAsync<SampleDbContext>(async (context, sp) =>
            {
                var seeder = sp.GetRequiredService<IDbSeeder<SampleDbContext>>();
                await seeder.SeedAsync(context);
            });
            migrateTask.GetAwaiter().GetResult();

            // Assert
            mockSeeder.Verify(s => s.SeedAsync(It.IsAny<SampleDbContext>()), Times.Once);
        }
    }

    // Sample implementations for testing
    public class SampleDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public SampleDbContext(Microsoft.EntityFrameworkCore.DbContextOptions options) : base(options) { }
        public Microsoft.EntityFrameworkCore.DatabaseFacade Database => base.Database;
    }

    public class SampleSeeder : IDbSeeder<SampleDbContext>
    {
        public Task SeedAsync(SampleDbContext context)
        {
            return Task.CompletedTask;
        }
    }
}
