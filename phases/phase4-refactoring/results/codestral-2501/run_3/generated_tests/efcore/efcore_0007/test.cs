using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContextPool_ShouldCallGetService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbContextOptionsBuilder = new Mock<DbContextOptionsBuilder>();

            // Mock the GetService method
            mockServiceProvider.Setup(sp => sp.GetService(typeof(DbContext))).Returns(new DbContext(new DbContextOptions<DbContext>()));

            // Act
            serviceCollection.AddDbContextPool<DbContext, DbContext>(
                (sp, ob) => mockDbContextOptionsBuilder.Object,
                poolSize: 10);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var dbContext = serviceProvider.GetService<DbContext>();

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(DbContext)), Times.Once);
        }
    }
}
