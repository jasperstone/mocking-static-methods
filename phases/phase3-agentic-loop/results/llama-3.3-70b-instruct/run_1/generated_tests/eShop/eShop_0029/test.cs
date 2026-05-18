using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public async Task AddMigration_WithSeeder_CallsMigrateDbContextAsync()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var seederMock = new Mock<Func<DbContext, IServiceProvider, Task>>();
            var contextMock = new Mock<DbContext>();

            // Act
            services.AddMigration<DbContext>(seederMock.Object);
            var hostedService = services.BuildServiceProvider().GetRequiredService<IHostedService>() as MigrationHostedService<DbContext>;

            // Assert
            await hostedService.StartAsync(default);
            seederMock.Verify(s => s(contextMock.Object, serviceProvider), Times.Once);
        }

        [Fact]
        public async Task AddMigration_WithDbSeeder_CallsSeedAsync()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var dbSeederMock = new Mock<IDbSeeder<DbContext>>();
            var contextMock = new Mock<DbContext>();

            services.AddScoped<IDbSeeder<DbContext>>(dbSeederMock.Object);

            // Act
            services.AddMigration<DbContext, IDbSeeder<DbContext>>();
            var hostedService = services.BuildServiceProvider().GetRequiredService<IHostedService>() as MigrationHostedService<DbContext>;

            // Assert
            await hostedService.StartAsync(default);
            dbSeederMock.Verify(s => s.SeedAsync(contextMock.Object), Times.Once);
        }

        [Fact]
        public async Task MigrateDbContextAsync_WithSeeder_CallsSeeder()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var seederMock = new Mock<Func<DbContext, IServiceProvider, Task>>();
            var contextMock = new Mock<DbContext>();

            services.AddTransient<DbContext>(provider => contextMock.Object);

            // Act
            await serviceProvider.MigrateDbContextAsync(seederMock.Object);

            // Assert
            seederMock.Verify(s => s(contextMock.Object, serviceProvider), Times.Once);
        }
    }
}
