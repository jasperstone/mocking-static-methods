using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace eShop.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public void AddMigration_WithSeeder_ServiceProviderIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var seederMock = new Mock<Func<DbContext, IServiceProvider, Task>>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            services.AddMigration<DbContext>(seederMock.Object);

            // Assert
            seederMock.Verify(s => s(It.IsAny<DbContext>(), It.IsAny<IServiceProvider>()), Times.Once);
        }

        [Fact]
        public void AddMigration_WithDbSeeder_ServiceProviderIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var dbSeederMock = new Mock<IDbSeeder<DbContext>>();

            services.AddScoped<IDbSeeder<DbContext>, Mock<IDbSeeder<DbContext>>>();
            services.AddMigration<DbContext, Mock<IDbSeeder<DbContext>>>();

            // Act
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            dbSeederMock.Verify(s => s.SeedAsync(It.IsAny<DbContext>()), Times.Once);
        }
    }
}
