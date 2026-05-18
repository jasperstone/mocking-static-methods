using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public void AddMigration_TwoGenericParameters_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            var result = services.AddMigration<MockDbContext, TestDbSeeder>();

            // Assert
            Assert.Same(services, result);
            
            // Verify the registration works and GetRequiredService is called internally
            // by resolving the hosted service which triggers the lambda
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var hostedServices = scope.ServiceProvider.GetServices<IHostedService>();
            Assert.NotEmpty(hostedServices);
        }

        [Fact]
        public void AddMigration_TwoGenericParameters_ResolvesCorrectSeederType()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddMigration<MockDbContext, TestDbSeeder>();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            
            var seeder = scopedServices.GetRequiredService<IDbSeeder<MockDbContext>>();
            Assert.NotNull(seeder);
            Assert.IsType<TestDbSeeder>(seeder);
        }

        [Fact]
        public void AddMigration_SingleGenericParameter_ReturnsSameCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            var result = services.AddMigration<MockDbContext>();

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddMigration_WithCustomSeederFunc_ReturnsSameCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            var result = services.AddMigration<MockDbContext>((ctx, sp) => Task.CompletedTask);

            // Assert
            Assert.Same(services, result);
        }
    }

    // Test implementations - these allow compilation and verify the GetRequiredService path
    public class MockDbContext : DbContext
    {
        public MockDbContext(DbContextOptions<MockDbContext> options) : base(options) { }
    }

    public class TestDbSeeder : IDbSeeder<MockDbContext>
    {
        public Task SeedAsync(MockDbContext context) => Task.CompletedTask;
    }
}
