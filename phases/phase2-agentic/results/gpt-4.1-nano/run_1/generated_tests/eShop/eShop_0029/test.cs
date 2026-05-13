using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public async Task AddMigration_WithServiceProvider_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockSeeder = new Mock<IDbSeeder<SampleDbContext>>();
            services.AddScoped(_ => mockSeeder.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var mockSeederInstance = mockSeeder.Object;

            // Setup GetRequiredService to return the mockSeeder when requested
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IDbSeeder<SampleDbContext>>())
                .Returns(mockSeederInstance);

            // Add the mocked IServiceProvider to the service collection
            services.AddSingleton(serviceProviderMock.Object);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Act
            // Call the extension method which internally calls GetRequiredService
            var resultServices = services.AddMigration<SampleDbContext, SampleDbSeeder>();

            // Create a scope to test MigrateDbContextAsync
            using var scope = provider.CreateScope();
            var scopeServices = scope.ServiceProvider;

            // Call the private method via reflection or directly if accessible
            // For simplicity, we will invoke the method directly if accessible
            // But since it's private, we need to invoke via reflection
            var methodInfo = typeof(MigrateDbContextExtensions)
                .GetMethod("MigrateDbContextAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .MakeGenericMethod(typeof(SampleDbContext));

            await (Task)methodInfo.Invoke(null, new object[] { scopeServices, new Func<SampleDbContext, IServiceProvider, Task>((context, sp) => sp.GetRequiredService<IDbSeeder<SampleDbContext>>().SeedAsync(context)) });

            // Assert
            // Verify that GetRequiredService was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IDbSeeder<SampleDbContext>>(), Times.Once);
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public SampleDbContext(Microsoft.EntityFrameworkCore.DbContextOptions options) : base(options)
        {
        }
    }

    // Sample Seeder
    public class SampleDbSeeder : IDbSeeder<SampleDbContext>
    {
        public Task SeedAsync(SampleDbContext context)
        {
            // Seed logic here
            return Task.CompletedTask;
        }
    }
}
