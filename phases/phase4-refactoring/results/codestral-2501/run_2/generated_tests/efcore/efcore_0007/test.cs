using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContextPool_ShouldRegisterServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbContextOptionsBuilder = new Mock<DbContextOptionsBuilder>();

            // Act
            serviceCollection.AddDbContextPool<DbContext, DbContext>(
                (sp, ob) => mockDbContextOptionsBuilder.Object,
                poolSize: 10);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider.GetService<IDbContextPool<DbContext>>());
            Assert.NotNull(serviceProvider.GetService<IScopedDbContextLease<DbContext>>());
            Assert.NotNull(serviceProvider.GetService<DbContext>());
        }
    }
}
