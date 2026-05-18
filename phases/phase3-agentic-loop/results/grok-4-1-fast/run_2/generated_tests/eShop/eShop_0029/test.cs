using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public void AddMigration_TDbSeeder_ReturnsSameServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddMigration<TestDbContext, TestSeeder>();

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddMigration_TDbSeeder_RegistersHostedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase();

            // Act
            services.AddMigration<TestDbContext, TestSeeder>();
            var provider = services.BuildServiceProvider();
            var hostedServices = provider.GetServices<IHostedService>();

            // Assert
            Assert.Single(hostedServices);
        }

        [Fact]
        public async Task AddMigration_TDbSeeder_ExecutesGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase();
            services.AddLogging();
            
            int getRequiredServiceCalled = 0;
            var mockSeeder = new MockSeeder(() => getRequiredServiceCalled++);
            services.AddScoped<IDbSeeder<TestDbContext>>(_ => mockSeeder);

            // Act
            services.AddMigration<TestDbContext, TestSeeder>();
            var provider = services.BuildServiceProvider();
            var hostedService = provider.GetRequiredService<IHostedService>();

            // Assert - execution will call GetRequiredService via the registered lambda
            await hostedService.StartAsync(CancellationToken.None);
            await hostedService.StopAsync(CancellationToken.None);
            
            Assert.Equal(1, getRequiredServiceCalled);
        }
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    }

    public class TestSeeder : IDbSeeder<TestDbContext>
    {
        public Task SeedAsync(TestDbContext context) => Task.CompletedTask;
    }
}

public interface IDbSeeder<TContext>
{
    Task SeedAsync(TContext context);
}

class MockSeeder : IDbSeeder<TestDbContext>
{
    private readonly Action _onGet;
    public MockSeeder(Action onGet) => _onGet = onGet;
    public Task SeedAsync(TestDbContext context) => Task.CompletedTask;
}
