using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace eShop.Shared.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        private class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        }

        private class TestDbSeeder : IDbSeeder<TestDbContext>
        {
            public int SeedCallCount { get; private set; }
            public Task SeedAsync(TestDbContext context)
            {
                SeedCallCount++;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void AddMigration_WithSeederType_RegistersScopedSeeder()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddMigration<TestDbContext, TestDbSeeder>();

            // Assert
            Assert.Same(services, result);
            
            var seederRegistration = services
                .SingleOrDefault(d => d.ServiceType == typeof(IDbSeeder<TestDbContext>));
            Assert.NotNull(seederRegistration);
            Assert.Equal(typeof(TestDbSeeder), seederRegistration.ImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, seederRegistration.Lifetime);
        }

        [Fact]
        public void AddMigration_WithSeederType_RegistersHostedService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMigration<TestDbContext, TestDbSeeder>();

            // Assert
            var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
            Assert.Single(hostedServices);
        }

        [Fact]
        public async Task AddMigration_WithSeederType_ExecutesGetRequiredServiceAndSeeder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestDbContext>(options => 
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddScoped<TestDbSeeder>();

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var hostedService = serviceProvider.GetRequiredService<IHostedService>();
            await hostedService.StartAsync(CancellationToken.None);

            // Assert - verifies the GetRequiredService path was taken (Seeder was resolved and called)
            using var scope = serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDbSeeder<TestDbContext>>() as TestDbSeeder;
            Assert.NotNull(seeder);
            Assert.Equal(1, seeder.SeedCallCount);
        }
    }
}
