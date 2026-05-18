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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbSeederMock = new Mock<IDbSeeder<TestDbContext>>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IDbSeeder<TestDbContext>>()).Returns(dbSeederMock.Object);

            // Act
            services.AddMigration<TestDbContext, TestDbSeeder>();

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IDbSeeder<TestDbContext>>(), Times.Once);
        }

        [Fact]
        public async Task AddMigration_WithSeeder_CallsSeedAsync()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbSeederMock = new Mock<IDbSeeder<TestDbContext>>();
            var testDbContextMock = new Mock<TestDbContext>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IDbSeeder<TestDbContext>>()).Returns(dbSeederMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<TestDbContext>()).Returns(testDbContextMock.Object);

            // Act
            services.AddMigration<TestDbContext, TestDbSeeder>();

            // Assert
            await dbSeederMock.Object.SeedAsync(testDbContextMock.Object);
            dbSeederMock.Verify(ds => ds.SeedAsync(testDbContextMock.Object), Times.Once);
        }

        public class TestDbContext : DbContext
        {
        }

        public interface IDbSeeder<TContext> where TContext : DbContext
        {
            Task SeedAsync(TContext context);
        }

        public class TestDbSeeder : IDbSeeder<TestDbContext>
        {
            public Task SeedAsync(TestDbContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
