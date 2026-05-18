using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace EFCore.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContextPool_ResolvesCorrectImplementation()
        {
            // Arrange
            var serviceCollection = new Mock<IServiceCollection>();
            var serviceProvider = new Mock<IServiceProvider>();

            var mockContextService = new Mock<DbContext>();
            var mockContextImplementation = new Mock<DbContext>();

            serviceCollection
                .Setup(s => s.TryAddScoped(It.IsAny<Type>(), It.IsAny<Func<IServiceProvider, object>>()))
                .Callback<Type, Func<IServiceProvider, object>>((type, factory) =>
                {
                    if (type == typeof(DbContext))
                    {
                        serviceProvider
                            .Setup(sp => sp.GetService(type))
                            .Returns(factory(serviceProvider.Object));
                    }
                });

            // Act
            EntityFrameworkServiceCollectionExtensions.AddDbContextPool<DbContext, DbContext>(
                serviceCollection.Object,
                (sp, ob) => { },
                10);

            // Assert
            serviceProvider.Verify(sp => sp.GetService(typeof(DbContext)), Times.Once);
            serviceProvider.Verify(sp => sp.GetService(typeof(DbContext)), Times.Returns(mockContextImplementation.Object));
        }
    }
}
