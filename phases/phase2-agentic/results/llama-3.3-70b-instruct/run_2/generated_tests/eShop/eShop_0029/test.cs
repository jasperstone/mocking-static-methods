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
        public async Task AddMigration_WithSeeder_CallsMigrateDbContextAsync()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var seederMock = new Mock<Func<DbContext, IServiceProvider, Task>>();
            var contextMock = new Mock<DbContext>();

            services.AddDbContext<DbContext>(options => options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            services.AddSingleton(seederMock.Object);

            // Act
            services.AddMigration<DbContext>(seederMock.Object);
            var hostedService = serviceProvider.GetService<IHostedService>() as MigrationHostedService<DbContext>;

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

            services.AddDbContext<DbContext>(options => options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            services.AddSingleton(dbSeederMock.Object);

            // Act
            services.AddMigration<DbContext, DbSeeder>();
            var hostedService = serviceProvider.GetService<IHostedService>() as MigrationHostedService<DbContext>;

            // Assert
            await hostedService.StartAsync(default);
            dbSeederMock.Verify(s => s.SeedAsync(contextMock.Object), Times.Once);
        }

        private class DbSeeder : IDbSeeder<DbContext>
        {
            public Task SeedAsync(DbContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
