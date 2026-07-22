using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContextPool_ShouldAddServicesToCollection()
        {
            // Arrange
            var serviceCollection = new Mock<IServiceCollection>();
            var optionsAction = new Action<IServiceProvider, DbContextOptionsBuilder>((sp, ob) => { });

            // Act
            EntityFrameworkServiceCollectionExtensions.AddDbContextPool<DbContext, DbContext>(
                serviceCollection.Object,
                optionsAction,
                10);

            // Assert
            serviceCollection.Verify(
                x => x.TryAddSingleton(typeof(IDbContextPool<DbContext>), typeof(DbContextPool<DbContext>)),
                Times.Once);

            serviceCollection.Verify(
                x => x.TryAddScoped(typeof(IScopedDbContextLease<DbContext>), typeof(ScopedDbContextLease<DbContext>)),
                Times.Once);

            serviceCollection.Verify(
                x => x.TryAddScoped(typeof(DbContext), It.IsAny<Func<IServiceProvider, DbContext>>()),
                Times.Once);

            serviceCollection.Verify(
                x => x.TryAddScoped(It.IsAny<Func<IServiceProvider, DbContext>>()),
                Times.Once);
        }
    }
}
